using BaboonAPI.Hooks.Initializer;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using TootTally.CustomLeaderboard;
using TootTally.Discord;
using TootTally.Graphics;
using TootTally.Replays;
using TootTally.Utils;

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
        public const int BUILDDATE = 20230315;
        internal ConfigEntry<string> APIKey { get; private set; }
        public ConfigEntry<bool> AllowTMBUploads { get; private set; }
        public ConfigEntry<bool> ShouldDisplayToasts { get; private set; }

        private Harmony _harmony;

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
            
            GameInitializationEvent.Register(Info, TryInitialize);
        }

        private void TryInitialize()
        {
            var settings = OptionalTrombSettings.GetConfigPage("TootTally");
            if (settings != null)
            {
                OptionalTrombSettings.Add(settings, AllowTMBUploads);
                OptionalTrombSettings.Add(settings, APIKey);
                OptionalTrombSettings.Add(settings, ShouldDisplayToasts);
            }

            AssetManager.LoadAssets();
            GameThemeManager.Initialize();

            _harmony.PatchAll(typeof(UserLogin));
            _harmony.PatchAll(typeof(GameThemeManager));
            _harmony.PatchAll(typeof(ReplaySystemManager));
            _harmony.PatchAll(typeof(GameObjectFactory));
            _harmony.PatchAll(typeof(GlobalLeaderboardManager));
            _harmony.PatchAll(typeof(PopUpNotifManager));
            _harmony.PatchAll(typeof(DiscordRPC));

            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} [Build {BUILDDATE}] is loaded!");
            LogInfo($"Game Version: {GlobalVariables.version}");
        }

        public void Update()
        {

        }

        private class UserLogin
        {
            [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
            [HarmonyPrefix]
            public static void OnHomeControllerStartLoginUser()
            {
                if (userInfo == null)
                {
                    Instance.StartCoroutine(TootTallyAPIService.GetUser(user =>
                    {
                        if (user != null)
                            OnUserLogin(user);
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

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
            [HarmonyPrefix]
            public static void UpdateUserInfoOnLevelSelect()
            {
                //in case they failed to login. Try logging in again
                if (userInfo == null || userInfo.username == "Guest")
                    Instance.StartCoroutine(TootTallyAPIService.GetUser(user =>
                    {
                        if (user != null)
                            OnUserLogin(user);
                    }));
            }

            private static void OnUserLogin(SerializableClass.User user)
            {
                userInfo = user;
                Instance.StartCoroutine(TootTallyAPIService.SendModInfo(Chainloader.PluginInfos, allowSubmit =>
                {
                    userInfo.allowSubmit = allowSubmit;
                }));
            }
        }
    }
}
