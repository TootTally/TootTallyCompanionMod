using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using TrombLoader.Helpers;
using UnityEngine.UI;
using TootTally.Graphics;
using TootTally.Replays;
using TootTally.Utils;
using TootTally.CustomLeaderboard;
using TootTally.Utils.Helpers;
using TootTally.Discord;
using BepInEx.Bootstrap;

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
        public static Plugin Instance;
        public static SerializableClass.User userInfo; //Temporary public
        public const int BUILDDATE = 20230201;
        internal ConfigEntry<string> APIKey { get; private set; }
        public ConfigEntry<bool> AllowTMBUploads { get; private set; }
        public ConfigEntry<bool> ShouldDisplayToasts { get; private set; }

        public void Log(string msg)
        {
            LogInfo(msg);
        }

        private void Awake()
        {
            if (Instance != null) return; // Make sure that this is a singleton (even though it's highly unlikely for duplicates to happen)
            Instance = this;

            // Config
            APIKey = Config.Bind("API Setup", "API Key", "SignUpOnTootTally.com", "API Key for Score Submissions");
            AllowTMBUploads = Config.Bind("API Setup", "Allow Unknown Song Uploads", false, "Should this mod send unregistered charts to the TootTally server?");
            ShouldDisplayToasts = Config.Bind("General", "Display Toasts", true, "Activate toast notifications for important events.");
            object settings = OptionalTrombSettings.GetConfigPage("TootTally");
            if (settings != null)
            {
                OptionalTrombSettings.Add(settings, AllowTMBUploads);
                OptionalTrombSettings.Add(settings, APIKey);
                OptionalTrombSettings.Add(settings, ShouldDisplayToasts);
            }

            AssetManager.LoadAssets();
            GameThemeManager.Initialize();

            Harmony.CreateAndPatchAll(typeof(UserLogin));
            Harmony.CreateAndPatchAll(typeof(GameThemeManager));
            Harmony.CreateAndPatchAll(typeof(ReplaySystemManager));
            Harmony.CreateAndPatchAll(typeof(GameObjectFactory));
            Harmony.CreateAndPatchAll(typeof(GlobalLeaderboardManager));
            Harmony.CreateAndPatchAll(typeof(PopUpNotifManager));
            Harmony.CreateAndPatchAll(typeof(DiscordRPC));

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
                    Instance.StartCoroutine(TootTallyAPIService.GetUser((user) =>
                    {
                        if (user != null)
                        {
                            userInfo = user;
                            Instance.StartCoroutine(TootTallyAPIService.SendModInfo(Chainloader.PluginInfos));
                        }
                    }));

                    Instance.StartCoroutine(ThunderstoreAPIService.GetMostRecentModVersion((version) =>
                    {
                        if (version.CompareTo(PluginInfo.PLUGIN_VERSION) > 0)
                        {
                            PopUpNotifManager.DisplayNotif("New update available!\nNow available on Thunderstore", GameTheme.themeColors.notification.warningText, 8.5f);
                        }
                    }));
                }
            }





            [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
            [HarmonyPostfix]
            public static void OnHomeControllerStartPostFixAddMultiplayerButton(HomeController __instance)
            {
                GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
                GameObject mainMenu = mainCanvas.transform.Find("MainMenu").gameObject;

                #region MultiplayerButton
                GameObject multiplayerButton = GameObject.Instantiate(__instance.btncontainers[(int)HomeScreenButtonIndexes.Collect], mainMenu.transform);
                GameObject multiplayerHitbox = GameObject.Instantiate(mainMenu.transform.Find("Button2").gameObject, mainMenu.transform);
                multiplayerButton.name = "MULTIContainer";
                multiplayerHitbox.name = "MULTIButton";
                GameThemeManager.OverwriteGameObjectSpriteAndColor(multiplayerButton.transform.Find("FG").gameObject, "MultiplayerButtonV2.png", Color.white);
                multiplayerButton.transform.SetSiblingIndex(0);

                #endregion
                #region graphics

                //Play and collect buttons are programmed differently... for some reasons
                GameObject collectBtnContainer = __instance.btncontainers[(int)HomeScreenButtonIndexes.Collect];
                GameThemeManager.OverwriteGameObjectSpriteAndColor(collectBtnContainer.transform.Find("FG").gameObject, "CollectButtonV2.png", Color.white);
                GameObject collectFG = collectBtnContainer.transform.Find("FG").gameObject;
                RectTransform collectFGRectTransform = collectFG.GetComponent<RectTransform>();
                collectBtnContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(900, 475);
                collectBtnContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 190);
                collectFGRectTransform.sizeDelta = new Vector2(320, 190);
                GameObject collectOutline = __instance.allbtnoutlines[(int)HomeScreenButtonIndexes.Collect];
                RectTransform collectOutlineRectTransform = collectOutline.GetComponent<RectTransform>();
                collectOutlineRectTransform.sizeDelta = new Vector2(470, 230);
                GameObject textCollect = __instance.allpaneltxt.transform.Find("imgCOLLECT").gameObject;
                textCollect.GetComponent<RectTransform>().anchoredPosition = new Vector2(680, 410);
                textCollect.GetComponent<RectTransform>().sizeDelta = new Vector2(285, 48);

                GameObject improvBtnContainer = __instance.btncontainers[(int)HomeScreenButtonIndexes.Improv];
                //GameThemeManager.OverwriteGameObjectSpriteAndColor(ImprovBtnContainer.transform.Find("FG").gameObject, "ImprovButtonV2.png", Color.white);
                GameObject improvFG = improvBtnContainer.transform.Find("FG").gameObject;
                RectTransform improvFGRectTransform = improvFG.GetComponent<RectTransform>();
                improvBtnContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, 156);
                improvFGRectTransform.sizeDelta = new Vector2(450, 195);
                GameObject improvOutline = __instance.allbtnoutlines[(int)HomeScreenButtonIndexes.Improv];
                RectTransform improvOutlineRectTransform = improvOutline.GetComponent<RectTransform>();
                improvOutlineRectTransform.sizeDelta = new Vector2(470, 230);
                GameObject textImprov = __instance.allpaneltxt.transform.Find("imgImprov").gameObject;
                textImprov.GetComponent<RectTransform>().anchoredPosition = new Vector2(305, 385);
                textImprov.GetComponent<RectTransform>().sizeDelta = new Vector2(426, 54);
                #endregion

                #region hitboxes
                GameObject buttonCollect = mainMenu.transform.Find("Button2").gameObject;
                RectTransform buttonCollectTransform = buttonCollect.GetComponent<RectTransform>();
                buttonCollectTransform.anchoredPosition = new Vector2(739, 380);
                buttonCollectTransform.sizeDelta = new Vector2(320, 190);
                buttonCollectTransform.Rotate(0, 0, 15f);

                GameObject buttonImprov = mainMenu.transform.Find("Button4").gameObject;
                RectTransform buttonImprovTransform = buttonImprov.GetComponent<RectTransform>();
                buttonImprovTransform.anchoredPosition = new Vector2(310, 383);
                buttonImprovTransform.sizeDelta = new Vector2(450, 195);
                #endregion


            }

            [HarmonyPatch(typeof(HomeController), nameof(HomeController.doFastScreenShake))]
            [HarmonyPrefix]
            public static bool GetRidOfThatScreenShakePls(HomeController __instance) => false; //THANKS GOD

            public enum HomeScreenButtonIndexes
            {
                Play = 0,
                Collect = 1,
                Quit = 2,
                Improv = 3,
                Baboon = 4,
                Credit = 5,
                Settings = 6,
                Advanced = 7
            }
        }
    }
}
