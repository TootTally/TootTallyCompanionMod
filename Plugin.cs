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
using BepInEx.Logging;
using TootTally.Utils.TootTallySettings;
using Mono.Security.X509.Extensions;
using System;
using TootTally.Utils.APIServices;
using TootTally.TootTallyOverlay;

namespace TootTally
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("AutoToot", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("org.crispykevin.hovertoot", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("TrombLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource GetLogger() => Instance.Logger;

        public const string CONFIG_NAME = "TootTally.cfg";
        public const string PLUGIN_FOLDER_NAME = "TootTally-TootTally";
        public static Plugin Instance;
        public static SerializableClass.User userInfo; //Temporary public
        public const int BUILDDATE = 20230724;
        internal ConfigEntry<string> APIKey { get; private set; }
        public ConfigEntry<bool> AllowTMBUploads { get; private set; }
        public ConfigEntry<bool> ShouldDisplayToasts { get; private set; }

        public ConfigEntry<bool> DebugMode { get; private set; }
        public ConfigEntry<bool> ShowLeaderboard { get; private set; }
        public ConfigEntry<bool> SyncDuringSong { get; private set; }

        public static List<ITootTallyModule> tootTallyModules { get; private set; }

        public object moduleSettings { get; private set; }
        private Harmony _harmony;
        private object settingsPage = null;

        private static TootTallySettingPage _tootTallyMainPage;
        private static TootTallySettingPage _tootTallyModulePage;

        private void Awake()
        {
            if (Instance != null) return; // Make sure that this is a singleton (even though it's highly unlikely for duplicates to happen)
            Instance = this;

            _harmony = new Harmony(Info.Metadata.GUID);
            TootTallyLogger.Initialize();

            // Config
            APIKey = Config.Bind("API Setup", "API Key", "SignUpOnTootTally.com", "API Key for Score Submissions");
            AllowTMBUploads = Config.Bind("API Setup", "Allow Unknown Song Uploads", false, "Should this mod send unregistered charts to the TootTally server?");
            ShouldDisplayToasts = Config.Bind("General", "Display Toasts", true, "Activate toast notifications for important events.");
            DebugMode = Config.Bind("General", "Debug Mode", false, "Add extra logging information for debugging.");
            ShowLeaderboard = Config.Bind("General", "Show Leaderboard", true, "Show TootTally Leaderboard on Song Select");
            SyncDuringSong = Config.Bind("General", "Sync During Song", false, "Allow the game to sync during a song, may cause lags but prevent desyncs.");

            tootTallyModules = new List<ITootTallyModule>();
            _tootTallyMainPage = TootTallySettingsManager.AddNewPage("TootTally", "TootTally", 40f, new Color(.1f, .1f, .1f, .3f));
            _tootTallyModulePage = TootTallySettingsManager.AddNewPage("TTModules", "TTModules", 20f, new Color(.1f, .1f, .1f, .3f));

            GameInitializationEvent.Register(Info, TryInitialize);
        }

        private void TryInitialize()
        {
            if (_tootTallyMainPage != null)
            {
                _tootTallyMainPage.AddToggle("AllowTmbUploads", AllowTMBUploads);
                _tootTallyMainPage.AddToggle("ShouldDisplayToasts", ShouldDisplayToasts);
                _tootTallyMainPage.AddToggle("DebugMode", DebugMode);
                _tootTallyMainPage.AddToggle("ShowLeaderboard", ShowLeaderboard);
                _tootTallyMainPage.AddToggle("SyncDuringSong", SyncDuringSong);
                _tootTallyMainPage.AddButton("OpenTromBuddiesButton", new Vector2(350, 100), "Open TromBuddies", TootTallyOverlayManager.TogglePanel);
            }

            AssetManager.LoadAssets();
            GameThemeManager.Initialize();
            _harmony.PatchAll(typeof(UserLogin));
            _harmony.PatchAll(typeof(GameObjectFactory));
            _harmony.PatchAll(typeof(GameThemeManager));
            _harmony.PatchAll(typeof(TootTallySettingsManager));
            _harmony.PatchAll(typeof(ReplaySystemManager));
            _harmony.PatchAll(typeof(GlobalLeaderboardManager));
            _harmony.PatchAll(typeof(DiscordRPCManager));
            _harmony.PatchAll(typeof(UserStatusUpdater));
            
            //Managers
            gameObject.AddComponent<PopUpNotifManager>();
            gameObject.AddComponent<AnimationManager>();
            gameObject.AddComponent<DiscordRPCManager>();

            TootTallyLogger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} [Build {BUILDDATE}] is loaded!");
            TootTallyLogger.LogInfo($"Game Version: {Application.version}");
        }

        public static void AddModule(ITootTallyModule module)
        {
            if (tootTallyModules == null) TootTallyLogger.LogInfo("tootTallyModules IS NULL");
            tootTallyModules.Add(module);
            if (!module.IsConfigInitialized)
            {
                    module.ModuleConfigEnabled.SettingChanged += delegate { ModuleConfigEnabled_SettingChanged(module); };
                    _tootTallyModulePage.AddToggle(module.Name.Split('.')[1], module.ModuleConfigEnabled); // Holy shit this sucks why did I do this LMFAO

                    module.IsConfigInitialized = true;
                
            }
            if (module.ModuleConfigEnabled.Value)
            {
                try
                {
                    module.LoadModule();
                    TootTallyLogger.AddLoggerToListener(module.GetLogger);
                }
                catch (Exception e)
                {
                    TootTallyLogger.LogError($"Module {module.Name} couldn't be loaded.");
                    TootTallyLogger.LogError(e.Message);
                    TootTallyLogger.LogError(e.StackTrace);
                }
            }
        }

        private static void ModuleConfigEnabled_SettingChanged(ITootTallyModule module)
        {
            if (module.ModuleConfigEnabled.Value)
            {
                module.LoadModule();
                TootTallyLogger.AddLoggerToListener(module.GetLogger);
                PopUpNotifManager.DisplayNotif($"Module {module.Name} Enabled.", GameTheme.themeColors.notification.defaultText);
            }
            else
            {
                TootTallyLogger.RemoveLoggerFromListener(module.GetLogger);
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
                    TootTallyLogger.LogInfo("Messages received: " + messages.results.Count);
                    foreach (SerializableClass.Message message in messages.results)
                    {
                        if (_messagesReceived.FindAll(m => m.sent_on == message.sent_on).Count == 0)
                        {
                            _messagesReceived.Add(message);
                            PopUpNotifManager.DisplayNotif($"<size=14>From:{message.author} ({message.sent_on})</size>\n{message.message}", GameTheme.themeColors.notification.defaultText, 16f);
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
                Plugin.Instance.gameObject.AddComponent<TootTallyOverlayManager>();
                Plugin.Instance.gameObject.AddComponent<UserStatusManager>();
                UserStatusManager.SetUserStatus(UserStatusManager.UserStatus.Online);
            }


        }
    }
}
