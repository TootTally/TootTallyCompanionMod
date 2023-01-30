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
        public const int BUILDDATE = 20230130;
        public ConfigEntry<string> APIKey { get; private set; }
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
                #region graphics
                GameObject PlayBtnContainer = __instance.btncontainers[(int)HomeScreenButtonIndexes.Play];
                RectTransform PlayFGRectTransform = PlayBtnContainer.GetComponent<RectTransform>();
                PlayFGRectTransform.anchoredPosition += new Vector2(0, 100);
                PlayFGRectTransform.sizeDelta += new Vector2(0, -100);
                GameObject PlayOutline = __instance.allbtnoutlines[(int)HomeScreenButtonIndexes.Play];
                RectTransform PlayOutlineRectTransform = PlayOutline.GetComponent<RectTransform>();
                PlayOutlineRectTransform.sizeDelta += new Vector2(-5, -100); //I believe the base game made the sizeX 5 pixels too large, removing 5 makes it look a lot nicer


                //Play and collect buttons are programmed differently... for some reasons
                GameObject CollectBtnContainer = __instance.btncontainers[(int)HomeScreenButtonIndexes.Collect];
                GameObject CollectFG = CollectBtnContainer.transform.Find("FG").gameObject;
                RectTransform CollectFGRectTransform = CollectFG.GetComponent<RectTransform>();
                CollectBtnContainer.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -50);
                CollectFGRectTransform.sizeDelta += new Vector2(0, -100);
                GameObject CollectOutline = __instance.allbtnoutlines[(int)HomeScreenButtonIndexes.Collect];
                RectTransform CollectOutlineRectTransform = CollectOutline.GetComponent<RectTransform>();
                CollectOutlineRectTransform.sizeDelta += new Vector2(-5, -100); //dito here

                GameObject QuitBtnContainer = __instance.btncontainers[(int)HomeScreenButtonIndexes.Quit];
                QuitBtnContainer.GetComponent<RectTransform>().anchoredPosition += new Vector2(-415, 0);
                __instance.allpaneltxt.transform.Find("imgQuit").GetComponent<RectTransform>().anchoredPosition += new Vector2(-415, 0);
                #endregion

                #region hitboxes
                GameObject MainCanvas = GameObject.Find("MainCanvas").gameObject;
                GameObject MainMenu = MainCanvas.transform.Find("MainMenu").gameObject;

                GameObject ButtonPlay = MainMenu.transform.Find("Button1").gameObject;
                RectTransform ButtonPlayTransform = ButtonPlay.GetComponent<RectTransform>();
                ButtonPlayTransform.anchoredPosition += new Vector2(12, 10);
                ButtonPlayTransform.sizeDelta += new Vector2(0, -130);
                ButtonPlayTransform.Rotate(0, 0, -12f);

                GameObject ButtonCollect = MainMenu.transform.Find("Button2").gameObject;
                RectTransform ButtonCollectTransform = ButtonCollect.GetComponent<RectTransform>();
                ButtonCollectTransform.anchoredPosition += new Vector2(0, -20);
                ButtonCollectTransform.sizeDelta += new Vector2(0, -80);
                ButtonCollectTransform.Rotate(0, 0, 10f);

                GameObject ButtonQuit = MainMenu.transform.Find("Button3").gameObject;
                RectTransform ButtonQuitTransform = ButtonQuit.GetComponent<RectTransform>();
                ButtonQuitTransform.anchoredPosition += new Vector2(-400, 0);
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
