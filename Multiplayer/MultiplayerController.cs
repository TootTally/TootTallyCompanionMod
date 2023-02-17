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
    public class MultiplayerController : MonoBehaviour
    {
        private static PlaytestAnims _currentInstance;
        private static bool _isPointerOver, _acceptButtonClicked, _declineButtonClicked;
        private static RectTransform _mainPanelRectTransform, _multiButtonOutlineRectTransform;
        private static bool _isSceneActive;
        private static GameObject _mainPanel, _mainPanelFg, _mainPanelBorder, _acceptButton, _declineButton, _topBar;
        private static CanvasGroup _acceptButtonCanvasGroup, _topBarCanvasGroup, _mainTextCanvasGroup, _declineButtonCanvasGroup;
        private static CustomAnimation _multiBtnAnimation, _multiTextAnimation;


        [HarmonyPatch(typeof(PlaytestAnims), nameof(PlaytestAnims.Start))]
        [HarmonyPostfix]
        public static void ChangePlayTestToMultiplayerScreen(PlaytestAnims __instance)
        {
            if (_currentInstance == null)
                _currentInstance = __instance;
            __instance.factpanel.gameObject.SetActive(false);
            Plugin.LogInfo(SceneManager.GetActiveScene().name);

            GameObject canvasWindow = GameObject.Find("Canvas-Window").gameObject;
            Transform panelTransform = canvasWindow.transform.Find("Panel");

            _mainPanel = GameObjectFactory.CreateMultiplayerMainPanel(panelTransform, "MultiPanel");
            _mainPanelRectTransform = _mainPanel.GetComponent<RectTransform>();


            _mainPanelFg = _mainPanel.transform.Find("panelfg").gameObject;

            _mainPanelBorder = _mainPanel.transform.Find("Panelbg1").gameObject;

            _topBar = _mainPanel.transform.Find("top").gameObject;
            _topBarCanvasGroup = _topBar.GetComponent<CanvasGroup>();
            _mainTextCanvasGroup = _mainPanelFg.transform.Find("FactText").GetComponent<CanvasGroup>();

            _acceptButtonClicked = _declineButtonClicked = false;

            if (Plugin.userInfo.username == "emmett" || true)
            {
                _mainTextCanvasGroup.alpha = 0;
                _topBarCanvasGroup.alpha = 0;
                AnimatePanels();
            }
            else
            {
                _acceptButton = GameObjectFactory.CreateCustomButton(_mainPanelFg.transform, new Vector2(-80, -340), new Vector2(200, 50), "Accept", "AcceptButton", OnAcceptButtonClick).gameObject;
                _acceptButtonCanvasGroup = _acceptButton.AddComponent<CanvasGroup>();
                _declineButton = GameObjectFactory.CreateCustomButton(_mainPanelFg.transform, new Vector2(-320, -340), new Vector2(200, 50), "Decline", "DeclineButton", OnDeclineButtonClick).gameObject;
                _declineButtonCanvasGroup = _declineButton.AddComponent<CanvasGroup>();
            }

            AnimationManager.AddNewPositionAnimation(_mainPanel, new Vector2(0, -20), 2f, new EasingHelper.SecondOrderDynamics(1.25f, 1f, 0f));

            _isSceneActive = true;
        }

        [HarmonyPatch(typeof(Plugin), nameof(Plugin.Update))]
        [HarmonyPostfix]
        public static void Update()
        {
            if (!_isSceneActive) return;

            if (Input.GetKeyDown(KeyCode.Escape))
                OnDeclineButtonClick();

            if (_acceptButtonClicked || _declineButtonClicked)
            {
                if (_acceptButton != null && _declineButton != null)
                {
                    _acceptButtonCanvasGroup.alpha = _topBarCanvasGroup.alpha = _mainTextCanvasGroup.alpha = _declineButtonCanvasGroup.alpha -= Time.deltaTime * 4; //fade out texts and top bar
                    if (_acceptButtonCanvasGroup.alpha <= 0)
                    {
                        GameObject.DestroyImmediate(_acceptButton);
                        GameObject.DestroyImmediate(_declineButton);
                        _acceptButton = null;
                        _declineButton = null;
                    }
                }
            }
        }

        public static void OnAcceptButtonClick()
        {
            AnimatePanels();
            AnimationManager.AddNewSizeDeltaAnimation(_acceptButton, Vector2.zero, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            AnimationManager.AddNewSizeDeltaAnimation(_declineButton, Vector2.zero, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            _currentInstance.sfx_ok.Play();
            _acceptButtonClicked = true;
        }

        public static void AnimatePanels()
        {
            GameObject topPanel = GameObjectFactory.CreateMultiplayerPanel(_mainPanel.transform, "TopPanel", new Vector2(1230, 50), new Vector2(0, 284));
            topPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            GameObject leftPanel = GameObjectFactory.CreateMultiplayerPanel(_mainPanel.transform, "LeftPanel", new Vector2(750, 564), new Vector2(-240, -28));
            leftPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            GameObject topRightPanel = GameObjectFactory.CreateMultiplayerPanel(_mainPanel.transform, "TopRightPanel", new Vector2(426, 280), new Vector2(402, 114));
            topRightPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            GameObject bottomRightPanel = GameObjectFactory.CreateMultiplayerPanel(_mainPanel.transform, "BottomRightPanel", new Vector2(426, 280), new Vector2(402, -170));
            bottomRightPanel.GetComponent<RectTransform>().localScale = Vector2.zero;


            AnimationManager.AddNewSizeDeltaAnimation(_mainPanelFg, new Vector2(1240, 630), 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            AnimationManager.AddNewSizeDeltaAnimation(_mainPanelBorder, new Vector2(1250, 640), 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f), (sender) =>
            {
                AnimationManager.AddNewScaleAnimation(topPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                AnimationManager.AddNewScaleAnimation(leftPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                CustomButton selectSongButton = GameObjectFactory.CreateCustomButton(leftPanel.transform, Vector2.zero, new Vector2(200, 200), "SelectSong", "SelectSongButton", delegate
                {
                    SceneManager.LoadScene("levelselect");
                });
                selectSongButton.GetComponent<RectTransform>().localScale = Vector2.zero;
                AnimationManager.AddNewScaleAnimation(selectSongButton.gameObject, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                AnimationManager.AddNewScaleAnimation(topRightPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                AnimationManager.AddNewScaleAnimation(bottomRightPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            });
        }

        public static void OnDeclineButtonClick()
        {
            AnimationManager.AddNewScaleAnimation(_mainPanel, Vector2.zero, 2f, new EasingHelper.SecondOrderDynamics(.75f, 1f, 0f));

            _currentInstance.clickedOK();
            _declineButtonClicked = true;
        }

        [HarmonyPatch(typeof(PlaytestAnims), nameof(PlaytestAnims.nextScene))]
        [HarmonyPrefix]
        public static bool OverwriteNextScene()
        {
            _isSceneActive = false;
            GameObject.Destroy(_mainPanel);
            GameObject.Destroy(_acceptButton);
            GameObject.Destroy(_declineButton);

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
                if (Plugin.userInfo.id == 0)
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
