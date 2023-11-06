using BaboonAPI.Hooks.Initializer;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.FSharp.Collections;
using Rewired.ComponentControls.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using TootTally.CustomLeaderboard;
using TootTally.Discord;
using TootTally.GameplayModifier;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Replays;
using TootTally.SongDownloader;
using TootTally.Spectating;
using TootTally.TootTallyOverlay;
using TootTally.Utils;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using TootTally.Utils.TootTallySettings;
using UnityEngine;
using static UnityEngine.GridBrushBase;

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
        public static int BUILDDATE = 20231029;

        internal ConfigEntry<string> APIKey { get; private set; }
        public ConfigEntry<bool> ShouldDisplayToasts { get; private set; }

        public ConfigEntry<bool> DebugMode { get; private set; }
        public ConfigEntry<bool> ShowLeaderboard { get; private set; }
        public ConfigEntry<bool> ShowCoolS { get; private set; }
        public ConfigEntry<bool> AllowSpectate { get; private set; }
        public ConfigEntry<bool> EnableLocalDiffCalc { get; private set; }
        public ConfigEntry<bool> ShowSpectatorCount { get; private set; }
        public ConfigEntry<bool> ChangePitchSpeed { get; private set; }

        public static List<ITootTallyModule> TootTallyModules { get; private set; }

        public object ModuleSettings { get; private set; }
        private Harmony _harmony;

        private static TootTallySettingPage _tootTallyMainPage;
        private static TootTallySettingPage _tootTallyModulePage;
        private static SongDownloadPage _songDownloadPage;

        private void Awake()
        {
            if (Instance != null) return; // Make sure that this is a singleton (even though it's highly unlikely for duplicates to happen)
            Instance = this;
            _harmony = new Harmony(Info.Metadata.GUID);
            gameObject.AddComponent<TootTallyLogger>();

            // Config
            APIKey = Config.Bind("API Setup", "API Key", "SignUpOnTootTally.com", "API Key for Score Submissions.");
            ShouldDisplayToasts = Config.Bind("General", "Display Toasts", true, "Activate toast notifications for important events.");
            DebugMode = Config.Bind("General", "Debug Mode", false, "Add extra logging information for debugging.");
            ShowLeaderboard = Config.Bind("General", "Show Leaderboard", true, "Show TootTally Leaderboard on Song Select.");
            ShowCoolS = Config.Bind("General", "Show Cool S", false, "Show special graphic when getting SS and SSS on a song.");
            AllowSpectate = Config.Bind("General", "Allow Spectate", true, "Allow other players to spectate you while playing.");
            EnableLocalDiffCalc = Config.Bind("General", "Enable Local Diff Calc", true, "Enable Local Difficulty Calculation");
            ShowSpectatorCount = Config.Bind("General", "Show Spectator Count", true, "Show the number of spectator while playing.");
            ChangePitchSpeed = Config.Bind("General", "Change Pitch Speed", false, "Change the pitch on speed changes");

            TootTallyModules = new List<ITootTallyModule>();
            _tootTallyMainPage = TootTallySettingsManager.AddNewPage("TootTally", "TootTally", 40f, new Color(.1f, .1f, .1f, .3f));
            _tootTallyModulePage = TootTallySettingsManager.AddNewPage("TTModules", "TTModules", 20f, new Color(.1f, .1f, .1f, .3f));
            _songDownloadPage = TootTallySettingsManager.AddNewPage(new SongDownloadPage()) as SongDownloadPage; //Prevents the same page from being accidently created twice

            GameInitializationEvent.Register(Info, TryInitialize);
        }

        private void TryInitialize()
        {
            userInfo = new SerializableClass.User() { allowSubmit = false, id = 0, username = "Guess" };

            if (_tootTallyMainPage != null)
            {
                _tootTallyMainPage.AddToggle("ShouldDisplayToasts", new Vector2(400, 50), "Display Toasts", ShouldDisplayToasts);
                _tootTallyMainPage.AddToggle("DebugMode", new Vector2(400, 50), "Debug Mode", DebugMode);
                _tootTallyMainPage.AddToggle("ShowLeaderboard", new Vector2(400, 50), "Show Leaderboards", ShowLeaderboard);
                _tootTallyMainPage.AddToggle("ShowCoolS", new Vector2(400, 50), "Show cool-s", ShowCoolS);
                _tootTallyMainPage.AddToggle("AllowSpectate", new Vector2(400, 50), "Allow Spectate", AllowSpectate, SpectatingManager.OnAllowHostConfigChange);
                //_tootTallyMainPage.AddToggle("EnableLocalDiffCalc", new Vector2(400, 50), "Enable Local Diff Calc", EnableLocalDiffCalc);
                _tootTallyMainPage.AddToggle("ShowSpectatorCount", new Vector2(400, 50), "Show Spectator Count", ShowSpectatorCount);
                _tootTallyMainPage.AddToggle("ChangePitchSpeed", new Vector2(400, 50), "Change Pitch Speed", ChangePitchSpeed);
                _tootTallyMainPage.AddButton("OpenTromBuddiesButton", new Vector2(400, 60), "Open TromBuddies", TootTallyOverlayManager.TogglePanel);
                //_tootTallyMainPage.AddButton("OpenLoginButton", new Vector2(400, 60), "Open Login Panel", TootTallyOverlayManager.TogglePanel);
                _tootTallyMainPage.AddButton("ReloadAllSongButton", new Vector2(400, 60), "Reload Songs", ReloadTracks);
                //Adding / Removing causes out of bound / index not found exceptions
            }
            AssetManager.LoadAssets();
            GameThemeManager.Initialize();
            _harmony.PatchAll(typeof(UserLogin));
            _harmony.PatchAll(typeof(GameObjectFactory));
            _harmony.PatchAll(typeof(GameThemeManager));
            _harmony.PatchAll(typeof(TootTallySettingsManager));
            _harmony.PatchAll(typeof(ReplaySystemManager));
            _harmony.PatchAll(typeof(SpectatingManager.SpectatingManagerPatches));
            _harmony.PatchAll(typeof(GlobalLeaderboardManager));
            _harmony.PatchAll(typeof(GameModifierManager));
            _harmony.PatchAll(typeof(DiscordRPCManager));
            _harmony.PatchAll(typeof(UserStatusUpdater));
            _harmony.PatchAll(typeof(TootTallyMainPatches));
            //Managers
            gameObject.AddComponent<PopUpNotifManager>();
            gameObject.AddComponent<AnimationManager>();
            gameObject.AddComponent<DiscordRPCManager>();

            TootTallyLogger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} [Build {BUILDDATE}] is loaded!");
            TootTallyLogger.LogInfo($"Game Version: {Application.version}");
            TracksLoadedEvent.EVENT.Register(new UserLogin.TracksLoaderListener());
        }

        private bool _areManagersInitialized;

        public void OnHomeControllerStartInitalizeManagers()
        {
            if (_areManagersInitialized) return;

            Instance.gameObject.AddComponent<SpectatingManager>();
            Instance.gameObject.AddComponent<TootTallyOverlayManager>();
            Instance.gameObject.AddComponent<UserStatusManager>();
            _areManagersInitialized = true;
        }

        public static void AddModule(ITootTallyModule module)
        {
            if (TootTallyModules == null) TootTallyLogger.LogInfo("tootTallyModules IS NULL");
            TootTallyModules.Add(module);
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

        public void ReloadTracks() => TrackLookup.reload();

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

        public static void OnUserLogin(SerializableClass.User user)
        {
            userInfo = user;
            if (userInfo.api_key != null && userInfo.api_key != "")
                Plugin.Instance.APIKey.Value = userInfo.api_key;
            if (userInfo.id == 0)
            {
                userInfo.allowSubmit = false;
            }
            else
            {
                Instance.StartCoroutine(TootTallyAPIService.SendModInfo(Chainloader.PluginInfos, allowSubmit =>
                {
                    userInfo.allowSubmit = allowSubmit;
                }));
                UserStatusManager.SetUserStatus(UserStatusManager.UserStatus.Online);
            }
        }

        private class TootTallyMainPatches
        {
            [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
            [HarmonyPostfix]
            public static void OnHomeControllerStartInitalize() => Instance.OnHomeControllerStartInitalizeManagers();
        }

        private class UserLogin
        {

            [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
            [HarmonyPrefix]
            public static void OnHomeControllerStartLoginUser(HomeController __instance)
            {
                _messagesReceived ??= new List<SerializableClass.Message>();
                if (userInfo.id == 0)
                {
                    Instance.StartCoroutine(TootTallyAPIService.GetUserFromAPIKey((user) =>
                    {
                        OnUserLogin(user);
                        if (user.id == 0)
                            OpenLoginPanel(__instance);
                    }));

                    Instance.StartCoroutine(ThunderstoreAPIService.GetMostRecentModVersion(version =>
                    {
                        if (version.CompareTo(PluginInfo.PLUGIN_VERSION) > 0)
                            PopUpNotifManager.DisplayNotif("New update available!\nNow available on Thunderstore", GameTheme.themeColors.notification.warningText, 8.5f);
                    }));
                }
            }

            private static void OpenLoginPanel(HomeController __instance)
            {
                GameObject loginPanel = GameObjectFactory.CreateLoginPanel(__instance);
                loginPanel.transform.Find("FSLatencyPanel").GetComponent<RectTransform>().localScale = Vector2.zero;
                AnimationManager.AddNewScaleAnimation(loginPanel.transform.Find("FSLatencyPanel").gameObject, Vector2.one, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            }

            private static List<SerializableClass.Message> _messagesReceived;

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
            [HarmonyPostfix]
            public static void OnLevelSelectScreenGetMessagesFromServer(LevelSelectController __instance)
            {
                if (userInfo.id == 0) return; //Do not receive massages if not logged in

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

            private static bool _isReloadingSongs;

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Update))]
            [HarmonyPostfix]
            public static void OnLevelSelectControllerUpdateDetectRefreshSongs(LevelSelectController __instance)
            {
                if (!_isReloadingSongs)
                {
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
                    {
                        _isReloadingSongs = true;
                        PopUpNotifManager.DisplayNotif("Reloading tracks... Lag is normal.");
                        Plugin.Instance.Invoke("ReloadTracks", 0.5f);
                    }
                }
            }

            public class TracksLoaderListener : TracksLoadedEvent.Listener
            {
                public void OnTracksLoaded(FSharpList<TromboneTrack> value)
                {
                    _isReloadingSongs = false;
                }
            }


            [HarmonyPatch(typeof(HomeController), nameof(HomeController.doFastScreenShake))]
            [HarmonyPrefix]
            public static bool GetRidOfThatScreenShakePls() => false; //THANKS GOD

            [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.showPausePanel))]
            [HarmonyPostfix]
            public static void ChangePauseCanvasOrderingLayout(PauseCanvasController __instance) => __instance.gc.pausecanvas.GetComponent<Canvas>().sortingOrder = 0;

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
            [HarmonyPrefix]
            public static void UpdateUserInfoOnLevelSelect()
            {
                if (userInfo.id == 0)
                    Instance.StartCoroutine(TootTallyAPIService.GetUserFromAPIKey(OnUserLogin));
            }

        }
    }
}
