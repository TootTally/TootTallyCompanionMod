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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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



            private static bool _isPointerOver;
            private static EasingHelper.SecondOrderDynamics _multiButtonAnimation, _multiTextAnimation;
            private static RectTransform _multiButtonOutlineRectTransform, _multiTextRectTransform;
            private static Vector2 _multiButtonTargetSize, _multiTextTargetSize;

            [HarmonyPatch(typeof(PlaytestAnims), nameof(PlaytestAnims.Start))]
            [HarmonyPostfix]
            public static void ChangePlayTestToMultiplayerScreen(PlaytestAnims __instance)
            {
                GameObject.DestroyImmediate(__instance.factpanel.transform.Find("Panelbg2").gameObject);

                GameObject panelfg = __instance.factpanel.transform.Find("panelfg").gameObject;
                GameObject.DestroyImmediate(panelfg.transform.Find("Button").gameObject);
                Text text1 = __instance.factpanel.transform.Find("top/Text (1)").gameObject.GetComponent<Text>();
                Text text2 = __instance.factpanel.transform.Find("top/Text (1)/Text (2)").gameObject.GetComponent<Text>();
                text1.text = text2.text = "Multiplayer";
            }



            [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
            [HarmonyPostfix]
            public static void OnHomeControllerStartPostFixAddMultiplayerButton(HomeController __instance)
            {

                GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
                GameObject mainMenu = mainCanvas.transform.Find("MainMenu").gameObject;
                _multiButtonAnimation = new EasingHelper.SecondOrderDynamics(3.75f, 0.80f, 1.05f);
                _multiTextAnimation = new EasingHelper.SecondOrderDynamics(3.5f, 0.65f, 1.15f);
                #region MultiplayerButton
                GameObject multiplayerButton = GameObject.Instantiate(__instance.btncontainers[(int)HomeScreenButtonIndexes.Collect], mainMenu.transform);
                GameObject multiplayerHitbox = GameObject.Instantiate(mainMenu.transform.Find("Button2").gameObject, mainMenu.transform);
                GameObject multiplayerText = GameObject.Instantiate(__instance.paneltxts[(int)HomeScreenButtonIndexes.Collect], mainMenu.transform);
                multiplayerButton.name = "MULTIContainer";
                multiplayerHitbox.name = "MULTIButton";
                multiplayerText.name = "MULTIText";
                GameThemeManager.OverwriteGameObjectSpriteAndColor(multiplayerButton.transform.Find("FG").gameObject, "MultiplayerButtonV2.png", Color.white);
                GameThemeManager.OverwriteGameObjectSpriteAndColor(multiplayerText, "MultiText.png", Color.white);
                multiplayerButton.transform.SetSiblingIndex(0);
                _multiTextRectTransform = multiplayerText.GetComponent<RectTransform>();
                _multiTextRectTransform.anchoredPosition = new Vector2(100, 100);
                _multiTextRectTransform.sizeDelta = new Vector2(334, 87);
                _multiButtonTargetSize = new Vector2(.2f, .2f);
                _multiTextTargetSize = new Vector2(0.8f, 0.8f);

                multiplayerHitbox.GetComponent<Button>().onClick.AddListener(() =>
                 {
                     //Yoinked from DNSpy KEKW
                     __instance.addWaitForClick();
                     __instance.playSfx(3);
                     __instance.musobj.Stop();
                     __instance.quickFlash(2);
                     __instance.fadeAndLoadScene(16);
                     //SceneManager.MoveGameObjectToScene(GameObject.Instantiate(multiplayerButton), scene);
                     //6 and 7 cards collection
                     //8 is LoadController
                     //9 is GameController
                     //10 is PointSceneController
                     //11 is some weird ass fucking notes
                     //12 is intro
                     //13 is boss fail animation
                     //14 is how to play
                     //15 is end scene
                     //16 is the demo scene
                 });

                _multiButtonOutlineRectTransform = multiplayerButton.transform.Find("outline").GetComponent<RectTransform>();

                EventTrigger multiBtnEvents = multiplayerHitbox.GetComponent<EventTrigger>();
                multiBtnEvents.triggers.Clear();

                EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
                pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
                pointerEnterEvent.callback.AddListener((data) =>
                {
                    _multiButtonAnimation.SetStartPosition(_multiButtonOutlineRectTransform.localScale);
                    _multiButtonTargetSize = new Vector2(1.01f, 1.01f);
                    _multiTextTargetSize = new Vector2(1f, 1f);
                    __instance.playSfx(2); // btn sound effect KEKW
                    multiplayerButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(-2, 0);
                });
                multiBtnEvents.triggers.Add(pointerEnterEvent);

                EventTrigger.Entry pointerExitEvent = new EventTrigger.Entry();
                pointerExitEvent.eventID = EventTriggerType.PointerExit;
                pointerExitEvent.callback.AddListener((data) =>
                {
                    _multiButtonAnimation.SetStartPosition(_multiButtonOutlineRectTransform.localScale);
                    _multiButtonTargetSize = new Vector2(.2f, .2f);
                    _multiTextTargetSize = new Vector2(0.8f, 0.8f);
                    multiplayerButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(2, 0);
                });

                multiBtnEvents.triggers.Add(pointerExitEvent);


                #endregion

                #region graphics

                //Play and collect buttons are programmed differently... for some reasons
                GameObject collectBtnContainer = __instance.btncontainers[(int)HomeScreenButtonIndexes.Collect];
                GameThemeManager.OverwriteGameObjectSpriteAndColor(collectBtnContainer.transform.Find("FG").gameObject, "CollectButtonV2.png", Color.white);
                GameObject collectFG = collectBtnContainer.transform.Find("FG").gameObject;
                RectTransform collectFGRectTransform = collectFG.GetComponent<RectTransform>();
                collectBtnContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(900, 475.2f);
                collectBtnContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 190);
                collectFGRectTransform.sizeDelta = new Vector2(320, 190);
                GameObject collectOutline = __instance.allbtnoutlines[(int)HomeScreenButtonIndexes.Collect];
                GameThemeManager.OverwriteGameObjectSpriteAndColor(collectOutline, "CollectButtonOutline.png", Color.white);
                RectTransform collectOutlineRectTransform = collectOutline.GetComponent<RectTransform>();
                collectOutlineRectTransform.sizeDelta = new Vector2(351, 217.2f);
                GameObject textCollect = __instance.allpaneltxt.transform.Find("imgCOLLECT").gameObject;
                textCollect.GetComponent<RectTransform>().anchoredPosition = new Vector2(790, 430);
                textCollect.GetComponent<RectTransform>().sizeDelta = new Vector2(285, 48);
                textCollect.GetComponent<RectTransform>().pivot = Vector2.one / 2;

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

            [HarmonyPatch(typeof(HomeController), nameof(HomeController.Update))]
            [HarmonyPostfix]
            public static void AnimateMultiButton(HomeController __instance)
            {
                _multiButtonOutlineRectTransform.localScale = _multiButtonAnimation.GetNewPosition(_multiButtonTargetSize, Time.deltaTime);
                _multiButtonOutlineRectTransform.transform.parent.transform.Find("FG/texholder").GetComponent<CanvasGroup>().alpha = (_multiButtonOutlineRectTransform.localScale.y - 0.2f) / 1.5f;
                _multiTextRectTransform.localScale = _multiTextAnimation.GetNewPosition(_multiTextTargetSize, Time.deltaTime);
            }

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
