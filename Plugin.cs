using BaboonAPI.Hooks.Initializer;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using TootTally.Graphics;
using TootTally.Replays;
using TootTally.Utils;
using TootTally.CustomLeaderboard;
using TootTally.Utils.Helpers;
using TootTally.Discord;
using TootTally.Graphics.Animation;

namespace TootTally
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("AutoToot", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("org.crispykevin.hovertoot", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("TrombSettings", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("TrombLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static void LogDebug(string msg) => Instance.Logger.LogDebug(msg);
        public static void LogInfo(string msg) => Instance.Logger.LogInfo(msg);
        public static void LogError(string msg) => Instance.Logger.LogError(msg);
        public static void LogWarning(string msg) => Instance.Logger.LogWarning(msg);

        public const string CONFIG_NAME = "TootTally.cfg";
        public const string PLUGIN_FOLDER_NAME = "TootTally-TootTally";
        public static Plugin Instance;
        public static SerializableClass.User userInfo; //Temporary public
        public const int BUILDDATE = 20230417;
        internal ConfigEntry<string> APIKey { get; private set; }
        public ConfigEntry<bool> AllowTMBUploads { get; private set; }
        public ConfigEntry<bool> ShouldDisplayToasts { get; private set; }

        public static List<ITootTallyModule> tootTallyModules { get; private set; }

        public object moduleSettings { get; private set; }
        private Harmony _harmony;
        private object settingsPage = null;

        public void Log(string msg)
        {
            LogInfo(msg);
        }

        private void Awake()
        {
            if (Instance != null) return; // Make sure that this is a singleton (even though it's highly unlikely for duplicates to happen)
            Instance = this;

            _harmony = new Harmony(Info.Metadata.GUID);

            // Config
            APIKey = Config.Bind("API Setup", "API Key", "SignUpOnTootTally.com", "API Key for Score Submissions");
            AllowTMBUploads = Config.Bind("API Setup", "Allow Unknown Song Uploads", false, "Should this mod send unregistered charts to the TootTally server?");
            ShouldDisplayToasts = Config.Bind("General", "Display Toasts", true, "Activate toast notifications for important events.");

            tootTallyModules = new List<ITootTallyModule>();
            settingsPage = OptionalTrombSettings.GetConfigPage("TootTally"); // create the TootTally settings page
            moduleSettings = OptionalTrombSettings.GetConfigPage("TTModules"); // create the Modules page
            
            GameInitializationEvent.Register(Info, TryInitialize);
        }

        private void TryInitialize()
        {
            if (settingsPage != null)
            {
                OptionalTrombSettings.Add(settingsPage, AllowTMBUploads);
                OptionalTrombSettings.Add(settingsPage, APIKey);
                OptionalTrombSettings.Add(settingsPage, ShouldDisplayToasts);
            }

            AssetManager.LoadAssets();
            GameThemeManager.Initialize();

            _harmony.PatchAll(typeof(UserLogin));
            _harmony.PatchAll(typeof(AnimationManager));
            _harmony.PatchAll(typeof(GameObjectFactory));
            _harmony.PatchAll(typeof(GameThemeManager));
            _harmony.PatchAll(typeof(PopUpNotifManager));
            _harmony.PatchAll(typeof(ReplaySystemManager));
            _harmony.PatchAll(typeof(GlobalLeaderboardManager));
            _harmony.PatchAll(typeof(DiscordRPC));

            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} [Build {BUILDDATE}] is loaded!");
            LogInfo($"Game Version: {GlobalVariables.version}");
        }

        public void Update()
        {

        }

        public static void AddModule(ITootTallyModule module)
        {
            if (tootTallyModules == null) LogInfo("tootTallyModules IS NULL");
            tootTallyModules.Add(module);
            if (!module.IsConfigInitialized)
            {
                module.ModuleConfigEnabled.SettingChanged += delegate { ModuleConfigEnabled_SettingChanged(module); };
                module.IsConfigInitialized = true;
            }
            if (module.ModuleConfigEnabled.Value)
            {
                module.LoadModule();
            }
        }

        private static void ModuleConfigEnabled_SettingChanged(ITootTallyModule module)
        {
            if (module.ModuleConfigEnabled.Value)
            {
                module.LoadModule();
                PopUpNotifManager.DisplayNotif($"Module {module.Name} Enabled.", GameTheme.themeColors.notification.defaultText);
            }
            else
            {
                module.UnloadModule();
                PopUpNotifManager.DisplayNotif($"Module {module.Name} Disabled.", GameTheme.themeColors.notification.defaultText);
            }
        }

        private class UserLogin
        {
            [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
            [HarmonyPrefix]
            public static void OnHomeControllerStartLoginUser(HomeController __instance)
            {
                if (userInfo == null)
                {
                    Instance.StartCoroutine(TootTallyAPIService.GetUserFromAPIKey((user) =>
                    {
                        if (user != null)
                        {
                            OnUserLogin(user);

                            if (user.id == 0)
                            {
                                GameObject loginPanel = GameObjectFactory.CreateLoginPanel(__instance);
                                loginPanel.transform.Find("FSLatencyPanel").GetComponent<RectTransform>().localScale = Vector2.zero;
                                AnimationManager.AddNewScaleAnimation(loginPanel.transform.Find("FSLatencyPanel").gameObject, Vector2.one, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                            }

                        }
                    }));

                    Instance.StartCoroutine(ThunderstoreAPIService.GetMostRecentModVersion(version =>
                    {
                        if (version.CompareTo(PluginInfo.PLUGIN_VERSION) > 0)
                        {
                            PopUpNotifManager.DisplayNotif("New update available!\nNow available on Thunderstore", GameTheme.themeColors.notification.warningText, 8.5f);
                        }
                    }));
                }
            }

            private static List<SerializableClass.Message> _messagesReceived;

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
            [HarmonyPostfix]
            public static void OnLevelSelectScreenGetMessagesFromServer(LevelSelectController __instance)
            {
                if (userInfo == null || userInfo.id == 0) return; //Do not receive massages if not logged in

                Instance.StartCoroutine(TootTallyAPIService.GetMessageFromAPIKey((messages) =>
                {
                    Plugin.LogInfo("Messages received: " + messages.results.Count);
                    foreach (SerializableClass.Message message in messages.results)
                    {
                        if (_messagesReceived.FindAll(m => m.sent_on == message.sent_on).Count == 0)
                        {
                            _messagesReceived.Add(message);
                            PopUpNotifManager.DisplayNotif($"<size=14>From:{message.author} ({message.sent_on})</size>\n{message.message}", GameTheme.themeColors.notification.defaultText, 12f);
                        }
                    }
                }));
            }

            [HarmonyPatch(typeof(HomeController), nameof(HomeController.doFastScreenShake))]
            [HarmonyPrefix]
            public static bool GetRidOfThatScreenShakePls(HomeController __instance) => false; //THANKS GOD

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
            [HarmonyPrefix]
            public static void UpdateUserInfoOnLevelSelect()
            {
                //in case they failed to login. Try logging in again
                if (userInfo == null || userInfo.id == 0)
                    Instance.StartCoroutine(TootTallyAPIService.GetUserFromAPIKey((user) =>
                    {
                        if (user != null)
                            OnUserLogin(user);
                    }));
            }

            private static void OnUserLogin(SerializableClass.User user)
            {
                _messagesReceived = new List<SerializableClass.Message>();
                userInfo = user;
                Instance.StartCoroutine(TootTallyAPIService.SendModInfo(Chainloader.PluginInfos, allowSubmit =>
                {
                    userInfo.allowSubmit = allowSubmit;
                }));
            }
        }
    }
}
