using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TootTally.Multiplayer
{
    public static class MultiplayerManager
    {
        private static PlaytestAnims _currentInstance;
        private static RectTransform _multiButtonOutlineRectTransform;
        private static bool _isSceneActive;
        private static CustomAnimation _multiBtnAnimation, _multiTextAnimation;
        private static MultiplayerController.MultiplayerState _state, _previousState;
        private static MultiplayerController _multiController;


        [HarmonyPatch(typeof(PlaytestAnims), nameof(PlaytestAnims.Start))]
        [HarmonyPostfix]
        public static void ChangePlayTestToMultiplayerScreen(PlaytestAnims __instance)
        {
            if (_multiController != null)
            {
                if (_state == MultiplayerController.MultiplayerState.SelectSong)
                    UpdateMultiplayerState(MultiplayerController.MultiplayerState.Hosting);
                return;
            }


            _currentInstance = __instance;
            _multiController = new MultiplayerController(__instance);

            _state = _previousState = MultiplayerController.MultiplayerState.None;
            _isSceneActive = true;

            if (Plugin.userInfo.username != "emmett" || false) //temporary
                UpdateMultiplayerState(MultiplayerController.MultiplayerState.FirstTimePopUp);
            else
                UpdateMultiplayerState(MultiplayerController.MultiplayerState.LoadHome);
        }

        [HarmonyPatch(typeof(Plugin), nameof(Plugin.Update))]
        [HarmonyPostfix]
        public static void Update()
        {
            if (!_isSceneActive) return;

            if (Input.GetKeyDown(KeyCode.Escape) && _state != MultiplayerController.MultiplayerState.ExitScene)
                UpdateMultiplayerState(MultiplayerController.MultiplayerState.ExitScene);

        }

        [HarmonyPatch(typeof(PlaytestAnims), nameof(PlaytestAnims.nextScene))]
        [HarmonyPrefix]
        public static bool OverwriteNextScene()
        {
            Plugin.LogInfo("exiting multi");
            _isSceneActive = false;
            SceneManager.LoadScene("saveslot");
            return false;
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
            GameObject multiplayerText = GameObject.Instantiate(__instance.paneltxts[(int)HomeScreenButtonIndexes.Collect], mainMenu.transform);
            multiplayerButton.name = "MULTIContainer";
            multiplayerHitbox.name = "MULTIButton";
            multiplayerText.name = "MULTIText";
            GameThemeManager.OverwriteGameObjectSpriteAndColor(multiplayerButton.transform.Find("FG").gameObject, "MultiplayerButtonV2.png", Color.white);
            GameThemeManager.OverwriteGameObjectSpriteAndColor(multiplayerText, "MultiText.png", Color.white);
            multiplayerButton.transform.SetSiblingIndex(0);
            RectTransform multiTextRectTransform = multiplayerText.GetComponent<RectTransform>();
            multiTextRectTransform.anchoredPosition = new Vector2(100, 100);
            multiTextRectTransform.sizeDelta = new Vector2(334, 87);

            _multiButtonOutlineRectTransform = multiplayerButton.transform.Find("outline").GetComponent<RectTransform>();

            multiplayerHitbox.GetComponent<Button>().onClick.AddListener(() =>
            {
                __instance.addWaitForClick();
                __instance.playSfx(3);
                if (Plugin.userInfo == null || Plugin.userInfo.id == 0)
                {
                    PopUpNotifManager.DisplayNotif("Please login on TootTally to play online.", GameTheme.themeColors.notification.errorText);
                    return;
                }

                //Yoinked from DNSpy KEKW

                __instance.musobj.Stop();
                __instance.quickFlash(2);
                __instance.fadeAndLoadScene(16);
                //SceneManager.MoveGameObjectToScene(GameObject.Instantiate(multiplayerButton), scene);
                //1 is HomeScreen
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

            EventTrigger multiBtnEvents = multiplayerHitbox.GetComponent<EventTrigger>();
            multiBtnEvents.triggers.Clear();

            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) =>
            {
                if (_multiBtnAnimation != null)
                    _multiBtnAnimation.Dispose();
                _multiBtnAnimation = AnimationManager.AddNewScaleAnimation(multiplayerButton.transform.Find("outline").gameObject, new Vector2(1.01f, 1.01f), 0.5f, new EasingHelper.SecondOrderDynamics(3.75f, 0.80f, 1.05f));
                _multiBtnAnimation.SetStartVector(_multiButtonOutlineRectTransform.localScale);

                if (_multiTextAnimation != null)
                    _multiTextAnimation.Dispose();
                _multiTextAnimation = AnimationManager.AddNewScaleAnimation(multiplayerText, new Vector2(1f, 1f), 0.5f, new EasingHelper.SecondOrderDynamics(3.5f, 0.65f, 1.15f));
                _multiTextAnimation.SetStartVector(multiplayerText.GetComponent<RectTransform>().localScale);

                __instance.playSfx(2); // btn sound effect KEKW
                multiplayerButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(-2, 0);
            });
            multiBtnEvents.triggers.Add(pointerEnterEvent);

            EventTrigger.Entry pointerExitEvent = new EventTrigger.Entry();
            pointerExitEvent.eventID = EventTriggerType.PointerExit;
            pointerExitEvent.callback.AddListener((data) =>
            {
                if (_multiBtnAnimation != null)
                    _multiBtnAnimation.Dispose();
                _multiBtnAnimation = AnimationManager.AddNewScaleAnimation(multiplayerButton.transform.Find("outline").gameObject, new Vector2(.4f, .4f), 0.5f, new EasingHelper.SecondOrderDynamics(1.50f, 0.80f, 1.00f));
                _multiBtnAnimation.SetStartVector(_multiButtonOutlineRectTransform.localScale);

                if (_multiTextAnimation != null)
                    _multiTextAnimation.Dispose();
                _multiTextAnimation = AnimationManager.AddNewScaleAnimation(multiplayerText, new Vector2(.8f, .8f), 0.5f, new EasingHelper.SecondOrderDynamics(3.5f, 0.65f, 1.15f));
                _multiTextAnimation.SetStartVector(multiplayerText.GetComponent<RectTransform>().localScale);

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
            _multiButtonOutlineRectTransform.transform.parent.transform.Find("FG/texholder").GetComponent<CanvasGroup>().alpha = (_multiButtonOutlineRectTransform.localScale.y - 0.4f) / 1.5f;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickBack))]
        [HarmonyPrefix]
        public static bool ClickBackButtonMultiplayerSelectSong(LevelSelectController __instance)
        {
            GlobalVariables.levelselect_index = __instance.songindex;
            __instance.back_clicked = true;
            __instance.bgmus.Stop();
            __instance.doSfx(__instance.sfx_slidedown);
            __instance.fadeOut("playtest", 0.35f);
            return false;
        }

        private static void ResolveMultiplayerState()
        {
            Plugin.LogInfo($"Multiplayer state changed from {_previousState} to {_state}");
            switch (_state)
            {
                case MultiplayerController.MultiplayerState.FirstTimePopUp:
                    _multiController.EnterMainPanelAnimation();
                    _multiController.AddAcceptDeclineButtonsToPanelFG();
                    break;
                case MultiplayerController.MultiplayerState.LoadHome:
                    if (_previousState == MultiplayerController.MultiplayerState.None)
                        _multiController.EnterMainPanelAnimation();
                    _multiController.OnMultiplayerHomeScreenEnter();
                    break;
                case MultiplayerController.MultiplayerState.Home:
                    break;
                case MultiplayerController.MultiplayerState.Lobby:
                    break;
                case MultiplayerController.MultiplayerState.Hosting:
                    break;
                case MultiplayerController.MultiplayerState.SelectSong:
                    SceneManager.LoadScene("levelselect");
                    break;
                case MultiplayerController.MultiplayerState.ExitScene:
                    _currentInstance.clickedOK();
                    _multiController.OnExitAnimation();
                    _multiController = null;
                    break;
            }
        }

        public static void UpdateMultiplayerState(MultiplayerController.MultiplayerState newState)
        {
            _previousState = _state;
            _state = newState;
            ResolveMultiplayerState();
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
