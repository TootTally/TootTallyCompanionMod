﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TootTally.Graphics;
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
        private static bool _isPointerOver, _AcceptButtonClicked, _DeclineButtonClicked;
        private static EasingHelper.SecondOrderDynamics _multiButtonAnimation, _multiTextAnimation;
        private static EasingHelper.SecondOrderDynamics _panelResizeAnimation;
        private static RectTransform _multiButtonOutlineRectTransform, _multiTextRectTransform;
        private static Vector2 _multiButtonTargetSize, _multiTextTargetSize;
        private static Vector2 _panelTargetSize;
        private static bool _isSceneActive;
        private static GameObject _mainPanel, _mainPanelBorder, _acceptButton, _declineButton, _topBar;
        private static CanvasGroup _acceptButtonCanvasGroup, _topBarCanvasGroup, _mainTextCanvasGroup, _declineButtonCanvasGroup;


        [HarmonyPatch(typeof(PlaytestAnims), nameof(PlaytestAnims.Start))]
        [HarmonyPostfix]
        public static void ChangePlayTestToMultiplayerScreen(PlaytestAnims __instance)
        {
            if (_currentInstance != null) return;
            _currentInstance = __instance;
            _isSceneActive = true;
            GameObject.DestroyImmediate(__instance.factpanel.transform.Find("Panelbg2").gameObject);

            _mainPanel = __instance.factpanel.transform.Find("panelfg").gameObject;
            _mainPanelBorder = __instance.factpanel.transform.Find("Panelbg1").gameObject;
            GameObject.DestroyImmediate(_mainPanel.transform.Find("Button").gameObject);
            _topBar = __instance.factpanel.transform.Find("top").gameObject;
            _topBarCanvasGroup = _topBar.AddComponent<CanvasGroup>();
            Text topTextShadow = _topBar.transform.Find("Text (1)").gameObject.GetComponent<Text>();
            Text topText = _topBar.transform.Find("Text (1)/Text (2)").gameObject.GetComponent<Text>();
            topTextShadow.text = topText.text = "Multiplayer";
            Text mainText = _mainPanel.transform.Find("FactText").GetComponent<Text>();
            mainText.text =
            "<size=36>Welcome to TootTally Multiplayer Test!</size>\n\n\n<color=\"green\">This is a beta state of the multiplayer mod and there may be a lot of glitches.\n\n" +
            "Please report any bugs found on our discord.</color>";
            _mainTextCanvasGroup = mainText.gameObject.AddComponent<CanvasGroup>();

            _mainPanelBorder.GetComponent<Image>().color = Color.green;
            __instance.factpanel.transform.Find("top").GetComponent<Image>().color = Color.green;

            InputField inputfield = _mainPanel.AddComponent<InputField>();
            inputfield.image = _mainPanel.GetComponent<Image>();
            inputfield.textComponent = mainText;

            _acceptButton = GameObjectFactory.CreateCustomButton(__instance.factpanel.transform, new Vector2(160, -175), new Vector2(200, 50), "Accept", "AcceptButton", OnAcceptButtonClick).gameObject;
            _acceptButtonCanvasGroup = _acceptButton.AddComponent<CanvasGroup>();
            _declineButton = GameObjectFactory.CreateCustomButton(__instance.factpanel.transform, new Vector2(-75, -175), new Vector2(200, 50), "Decline", "DeclineButton", OnDeclineButtonClick).gameObject;
            _declineButtonCanvasGroup = _declineButton.AddComponent<CanvasGroup>();

            _panelResizeAnimation = new EasingHelper.SecondOrderDynamics(.75f, 1f, 0f);
            _panelResizeAnimation.SetStartVector(_mainPanel.GetComponent<RectTransform>().sizeDelta);

        }

        [HarmonyPatch(typeof(Plugin), nameof(Plugin.Update))]
        [HarmonyPostfix]
        public static void Update()
        {
            if (!_isSceneActive) return;

            if (_AcceptButtonClicked || _DeclineButtonClicked)
            {
                _acceptButtonCanvasGroup.alpha = _topBarCanvasGroup.alpha = _mainTextCanvasGroup.alpha = _declineButtonCanvasGroup.alpha -= Time.deltaTime * 4; //fade out texts and top bar
                if (_acceptButtonCanvasGroup.alpha < 0)
                {
                    _acceptButton.SetActive(false);
                    _declineButton.SetActive(false);
                }

                _mainPanel.GetComponent<RectTransform>().sizeDelta = _panelResizeAnimation.GetNewVector(_panelTargetSize, Time.deltaTime);
                _mainPanelBorder.GetComponent<RectTransform>().sizeDelta = _panelResizeAnimation.GetNewVector(_panelTargetSize, Time.deltaTime) + new Vector2(10, 10);
            }
        }

        public static void OnAcceptButtonClick()
        {
            RectTransform mainPanelRecTransform = _mainPanel.GetComponent<RectTransform>();
            _panelResizeAnimation.SetStartVector(mainPanelRecTransform.sizeDelta);
            _panelTargetSize = new Vector2(1240, 630);

            _currentInstance.sfx_ok.Play();
            _AcceptButtonClicked = true;
        }

        public static void OnDeclineButtonClick()
        {
            RectTransform mainPanelRecTransform = _mainPanel.GetComponent<RectTransform>();
            _panelResizeAnimation.SetStartVector(mainPanelRecTransform.sizeDelta);
            _panelTargetSize = new Vector2(0, 0);

            _currentInstance.clickedOK();
            _DeclineButtonClicked = true;
        }

        [HarmonyPatch(typeof(PlaytestAnims), nameof(PlaytestAnims.nextScene))]
        [HarmonyPrefix]
        public static bool OverwriteNextScene()
        {
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
                _multiButtonAnimation.SetStartVector(_multiButtonOutlineRectTransform.localScale);
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
                _multiButtonAnimation.SetStartVector(_multiButtonOutlineRectTransform.localScale);
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
            _multiButtonOutlineRectTransform.localScale = _multiButtonAnimation.GetNewVector(_multiButtonTargetSize, Time.deltaTime);
            _multiButtonOutlineRectTransform.transform.parent.transform.Find("FG/texholder").GetComponent<CanvasGroup>().alpha = (_multiButtonOutlineRectTransform.localScale.y - 0.2f) / 1.5f;
            _multiTextRectTransform.localScale = _multiTextAnimation.GetNewVector(_multiTextTargetSize, Time.deltaTime);
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