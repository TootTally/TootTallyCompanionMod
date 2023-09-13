using BaboonAPI.Hooks.Initializer;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using TootTally.CustomLeaderboard;
using TootTally.Discord;
using TootTally.GameplayModifier;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Replays;
using TootTally.SongDownloader;
using TootTally.TootTallyOverlay;
using TootTally.Utils;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using TootTally.Utils.TootTallySettings;
using UnityEngine;

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
        public static int BUILDDATE = 20230913;

        internal ConfigEntry<string> APIKey { get; private set; }
        public ConfigEntry<bool> ShouldDisplayToasts { get; private set; }

        public ConfigEntry<bool> DebugMode { get; private set; }
        public ConfigEntry<bool> ShowLeaderboard { get; private set; }
        public ConfigEntry<bool> ShowCoolS { get; private set; }
        public ConfigEntry<bool> AllowSpectate { get; private set; }

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

            TootTallyModules = new List<ITootTallyModule>();
            _tootTallyMainPage = TootTallySettingsManager.AddNewPage("TootTally", "TootTally", 40f, new Color(.1f, .1f, .1f, .3f));
            _tootTallyModulePage = TootTallySettingsManager.AddNewPage("TTModules", "TTModules", 20f, new Color(.1f, .1f, .1f, .3f));
            _songDownloadPage = TootTallySettingsManager.AddNewPage(new SongDownloadPage()) as SongDownloadPage; //Prevents the same page from being accidently created twice

            GameInitializationEvent.Register(Info, TryInitialize);
        }

        private void TryInitialize()
        {
            if (_tootTallyMainPage != null)
            {
                _tootTallyMainPage.AddToggle("ShouldDisplayToasts", new Vector2(400, 50), "Display Toasts", ShouldDisplayToasts);
                _tootTallyMainPage.AddToggle("DebugMode", new Vector2(400, 50), "Debug Mode", DebugMode);
                _tootTallyMainPage.AddToggle("ShowLeaderboard", new Vector2(400, 50), "Show Leaderboards", ShowLeaderboard);
                _tootTallyMainPage.AddToggle("ShowCoolS", new Vector2(400, 50), "Show cool-s", ShowCoolS);
                _tootTallyMainPage.AddToggle("AllowSpectate", new Vector2(400, 50), "Allow Spectate", AllowSpectate, SpectatingManager.OnAllowHostConfigChange);
                _tootTallyMainPage.AddButton("OpenTromBuddiesButton", new Vector2(400, 60), "Open TromBuddies", TootTallyOverlayManager.TogglePanel);
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
            _harmony.PatchAll(typeof(SpectatingManager));
            _harmony.PatchAll(typeof(GlobalLeaderboardManager));
            _harmony.PatchAll(typeof(GameModifierManager));
            _harmony.PatchAll(typeof(DiscordRPCManager));
            _harmony.PatchAll(typeof(UserStatusUpdater));
            //Managers
            gameObject.AddComponent<PopUpNotifManager>();
            gameObject.AddComponent<AnimationManager>();
            gameObject.AddComponent<DiscordRPCManager>();
            TootTallyLogger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} [Build {BUILDDATE}] is loaded!");
            TootTallyLogger.LogInfo($"Game Version: {Application.version}");
            TracksLoadedEvent.EVENT.Register(new UserLogin.TracksLoaderListener());
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

        private class UserLogin
        {
            [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
            [HarmonyPrefix]
            public static void OnHomeControllerStartLoginUser(HomeController __instance)
            {

                if (userInfo == null)
                {
                    var icon = GameObjectFactory.CreateLoadingIcon(__instance.fullcanvas.transform, new Vector2(840, -440), new Vector2(128, 128), AssetManager.GetSprite("icon.png"), true, "UserLoginSwirly");
                    icon.StartRecursiveAnimation();
                    Instance.StartCoroutine(TootTallyAPIService.GetUserFromAPIKey((user) =>
                    {
                        icon.Dispose();
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
                            PopUpNotifManager.DisplayNotif("New update available!\nNow available on Thunderstore", GameTheme.themeColors.notification.warningText, 8.5f);
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

                Plugin.Instance.gameObject.AddComponent<SpectatingManager>();
                Plugin.Instance.gameObject.AddComponent<TootTallyOverlayManager>();
                Plugin.Instance.gameObject.AddComponent<UserStatusManager>();
                UserStatusManager.SetUserStatus(UserStatusManager.UserStatus.Online);
            }

            /*[HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
            [HarmonyPostfix]
            public static void OnGameControllerStart(GameController __instance)
            {
                var gameplayCanvas = GameObject.Find("GameplayCanvas");
                gameplayCanvas.GetComponent<Canvas>().scaleFactor = 2f;

                var topLeftCam = GameObject.Find("GameplayCam").GetComponent<Camera>();
                var topRightCam = GameObject.Instantiate(topLeftCam);
                var bottomLeftCam = GameObject.Instantiate(topLeftCam);
                var bottomRightCam = GameObject.Instantiate(topLeftCam);
                topRightCam.pixelRect = new Rect(960, 0, 960, 540);
                topLeftCam.pixelRect = new Rect(0, 0, 960, 540);
                bottomLeftCam.pixelRect = new Rect(0, 540, 960, 540);
                bottomRightCam.pixelRect = new Rect(960, 540, 960, 540);
            }*/
        }
    }
}
