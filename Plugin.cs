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

        public static Plugin Instance;
        public static SerializableClass.User userInfo; //Temporary public
        public const int BUILDDATE = 20230121;
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

            Theme.SetDefaultTheme();
            AssetManager.LoadAssets();
            Harmony.CreateAndPatchAll(typeof(UserLogin));
            Harmony.CreateAndPatchAll(typeof(GameTheme));
            Harmony.CreateAndPatchAll(typeof(ReplaySystemManager));
            Harmony.CreateAndPatchAll(typeof(GameObjectFactory));
            Harmony.CreateAndPatchAll(typeof(GlobalLeaderboardManager));
            Harmony.CreateAndPatchAll(typeof(PopUpNotifManager));
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
                        if (version.CompareTo(PluginInfo.PLUGIN_VERSION) < 0)
                        {
                            PopUpNotifManager.DisplayNotif("New update available!\nNow available on Thunderstore", Color.yellow, 8.5f);
                        }
                    }));
                }



            }
        }

        private class GameTheme
        {

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
            [HarmonyPostfix]
            public static void ChangeThemeOnLevelSelectControllerStartPostFix(LevelSelectController __instance)
            {
                if (Theme.isDefault) return;

                foreach (GameObject btn in __instance.btns)
                {
                    btn.transform.Find("ScoreText").gameObject.GetComponent<Text>().color = Theme.leaderboardTextColor;
                }
                foreach (Image img in __instance.btnbgs)
                {
                    img.color = Theme.panelBodyColor;
                }
                __instance.songtitlebar.GetComponent<Image>().color = Theme.panelBodyColor;
                __instance.scenetitle.GetComponent<Text>().color = Theme.panelBodyColor;
                __instance.songtitle.GetComponent<Text>().color = Theme.leaderboardTextColor;
                GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "title/GameObject").GetComponent<Text>().color = Theme.leaderboardTextColor;
                __instance.longsongtitle.color = Theme.leaderboardTextColor;
                GameObject lines = __instance.btnspanel.transform.Find("RightLines").gameObject;
                LineRenderer redLine = lines.transform.Find("Red").GetComponent<LineRenderer>();
                redLine.startColor = Theme.panelBodyColor;
                redLine.endColor = Theme.scoresbodyColor;
                for (int i = 1; i < 8; i++)
                {
                    LineRenderer yellowLine = lines.transform.Find("Yellow" + i).GetComponent<LineRenderer>();
                    yellowLine.startColor = Theme.scoresbodyColor;
                    yellowLine.endColor = Theme.panelBodyColor;
                }

                GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "capsules").GetComponent<Image>().color = Theme.panelBodyColor;

                __instance.backbutton.transform.Find("BG").GetComponent<Image>().color = Theme.panelBodyColor;
                __instance.playbtn.transform.Find("BG").GetComponent<Image>().color = Theme.panelBodyColor;
                __instance.btnrandom.GetComponent<Image>().color = Theme.panelBodyColor;

                __instance.songyear.color = Theme.leaderboardTextColor;
                __instance.songgenre.color = Theme.leaderboardTextColor;
                __instance.songduration.color = Theme.leaderboardTextColor;
                __instance.songcomposer.color = Theme.leaderboardTextColor;
                __instance.songtempo.color = Theme.leaderboardTextColor;
                __instance.songdesctext.color = Theme.leaderboardTextColor;
            }

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverBtn))]
            [HarmonyPrefix]
            public static bool OnHoverBtnPrefix(LevelSelectController __instance, object[] __args)
            {
                if (Theme.isDefault) return true;

                __instance.btnbgs[(int)__args[0]].color = Theme.scoresbodyColor;
                return false;
            }

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.unHoverBtn))]
            [HarmonyPrefix]
            public static bool OnUnHoverBtnPrefix(LevelSelectController __instance, object[] __args)
            {
                if (Theme.isDefault) return true;

                __instance.btnbgs[(int)__args[0]].color = Theme.panelBodyColor;
                return false;
            }
        }

    }
}
