using System;
using System.Linq;
using System.Security.Policy;
using HarmonyLib;
using TMPro;
using TootTally.CustomLeaderboard;
using TootTally.Graphics.Animation;
using TootTally.Replays;
using TootTally.Spectating;
using TootTally.TootTallyOverlay;
using TootTally.Utils;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public static class GameObjectFactory
    {
        private static CustomButton _buttonPrefab;
        private static TextMeshProUGUI _multicoloreTextPrefab, _comfortaaTextPrefab, _leaderboardHeaderPrefab, _leaderboardTextPrefab;
        private static GameObject _bubblePrefab;
        private static Slider _verticalSliderPrefab, _sliderPrefab;
        private static Slider _settingsPanelVolumeSlider;
        private static PopUpNotif _popUpNotifPrefab;

        private static GameObject _settingsGraphics, _creditPanel;
        private static GameObject _steamLeaderboardPrefab, _singleScorePrefab, _panelBodyPrefab;
        private static LeaderboardRowEntry _singleRowPrefab;
        private static TMP_InputField _inputFieldPrefab;

        private static GameObject _overlayPanelPrefab, _userCardPrefab;

        private static bool _isHomeControllerInitialized;
        private static bool _isLevelSelectControllerInitialized;


        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        static void YoinkSettingsGraphicsHomeController(HomeController __instance)
        {
            _settingsGraphics = __instance.fullsettingspanel.transform.Find("Settings").gameObject;
            _settingsPanelVolumeSlider = GameObject.Instantiate(__instance.set_sld_volume_tromb);
            _settingsPanelVolumeSlider.gameObject.SetActive(false);
            GameObject.DontDestroyOnLoad(_settingsPanelVolumeSlider);
            _creditPanel = __instance.ext_credits_go.transform.parent.gameObject;
            OnHomeControllerInitialize();
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void YoinkGraphicsLevelSelectController()
        {
            OnLevelSelectControllerInitialize();
        }

        public static void OnHomeControllerInitialize()
        {

            if (_isHomeControllerInitialized) return;

            SetMulticoloreTextPrefab();
            SetComfortaaTextPrefab();
            SetInputFieldPrefab();
            SetNotificationPrefab();
            SetBubblePrefab();
            SetCustomButtonPrefab();
            SetOverlayPanelPrefab();
            SetUserCardPrefab();
            _isHomeControllerInitialized = true;
            UpdatePrefabTheme();
        }

        public static void OnLevelSelectControllerInitialize()
        {
            if (_isLevelSelectControllerInitialized) return;

            TootTallyLogger.DebugModeLog("Generating Slider prefab...");
            SetSliderPrefab();
            TootTallyLogger.DebugModeLog("Generating VerticalSlider prefab...");
            SetVerticalSliderPrefab();
            TootTallyLogger.DebugModeLog("Generating GlobalLeaderboard prefab...");
            SetSteamLeaderboardPrefab();
            TootTallyLogger.DebugModeLog("Generating SingleScore prefab...");
            SetSingleScorePrefab();
            TootTallyLogger.DebugModeLog("Generating Leaderboard Header prefab...");
            SetLeaderboardHeaderPrefab();
            TootTallyLogger.DebugModeLog("Generating Leaderboard prefab...");
            SetLeaderboardTextPrefab();
            TootTallyLogger.DebugModeLog("Generating Single Leaderboard Row prefab...");
            SetSingleRowPrefab();
            _isLevelSelectControllerInitialized = true;
            TootTallyLogger.DebugModeLog("Applying theme...");
            UpdatePrefabTheme();
        }

        #region SetPrefabs

        private static void SetMulticoloreTextPrefab()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
            GameObject headerCreditText = mainCanvas.transform.Find("FullCreditsPanel/header-credits/Text").gameObject;

            GameObject textHolder = GameObject.Instantiate(headerCreditText);
            textHolder.name = "defaultTextPrefab";
            textHolder.SetActive(true);
            GameObject.DestroyImmediate(textHolder.GetComponent<Text>());
            _multicoloreTextPrefab = textHolder.AddComponent<TextMeshProUGUI>();
            _multicoloreTextPrefab.fontSize = 22;
            _multicoloreTextPrefab.text = "defaultText";
            _multicoloreTextPrefab.font = TMP_FontAsset.CreateFontAsset(headerCreditText.GetComponent<Text>().font);

            _multicoloreTextPrefab.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, .25f);
            _multicoloreTextPrefab.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
            _multicoloreTextPrefab.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, .25f);
            _multicoloreTextPrefab.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, GameTheme.themeColors.leaderboard.textOutline);

            _multicoloreTextPrefab.alignment = TextAlignmentOptions.Center;
            _multicoloreTextPrefab.GetComponent<RectTransform>().sizeDelta = textHolder.GetComponent<RectTransform>().sizeDelta;
            _multicoloreTextPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _multicoloreTextPrefab.richText = true;
            GameObject.DontDestroyOnLoad(_multicoloreTextPrefab);
        }

        private static void SetComfortaaTextPrefab()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
            GameObject advancePanelText = mainCanvas.transform.Find("AdvancedInfoPanel/primary-content/intro/copy").gameObject;

            GameObject textHolder = GameObject.Instantiate(advancePanelText);
            textHolder.name = "ComfortaaTextPrefab";
            textHolder.SetActive(true);
            GameObject.DestroyImmediate(textHolder.GetComponent<Text>());
            _comfortaaTextPrefab = textHolder.AddComponent<TextMeshProUGUI>();
            _comfortaaTextPrefab.fontSize = 22;
            _comfortaaTextPrefab.text = "DefaultText";
            _comfortaaTextPrefab.font = TMP_FontAsset.CreateFontAsset(advancePanelText.GetComponent<Text>().font);

            _comfortaaTextPrefab.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, .25f);
            _comfortaaTextPrefab.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
            _comfortaaTextPrefab.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, .25f);
            _comfortaaTextPrefab.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, GameTheme.themeColors.leaderboard.textOutline);

            _comfortaaTextPrefab.alignment = TextAlignmentOptions.Center;
            _comfortaaTextPrefab.GetComponent<RectTransform>().sizeDelta = textHolder.GetComponent<RectTransform>().sizeDelta;
            _comfortaaTextPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _comfortaaTextPrefab.richText = true;
            _comfortaaTextPrefab.enableWordWrapping = false;
            GameObject.DontDestroyOnLoad(_comfortaaTextPrefab);
        }

        private static void SetInputFieldPrefab()
        {
            var inputHolder = new GameObject("InputFieldHolder");
            var rectHolder = inputHolder.AddComponent<RectTransform>();
            rectHolder.anchoredPosition = Vector2.zero;
            rectHolder.sizeDelta = new Vector2(350, 50);
            var inputImageHolder = GameObject.Instantiate(inputHolder, inputHolder.transform);
            var inputTextHolder = GameObject.Instantiate(inputImageHolder, inputHolder.transform);
            inputImageHolder.name = "Image";
            inputTextHolder.name = "Text";

            _inputFieldPrefab = inputHolder.AddComponent<TMP_InputField>();

            rectHolder.anchorMax = rectHolder.anchorMin = Vector2.zero;

            //pain... @_@
            _inputFieldPrefab.image = inputImageHolder.AddComponent<Image>();
            RectTransform rectImage = inputImageHolder.GetComponent<RectTransform>();

            rectImage.anchorMin = rectImage.anchorMax = rectImage.pivot = Vector2.zero;
            rectImage.anchoredPosition = new Vector2(0, 4);
            rectImage.sizeDelta = new Vector2(350, 2);

            RectTransform rectText = inputTextHolder.GetComponent<RectTransform>();
            rectText.anchoredPosition = rectText.anchorMin = rectText.anchorMax = rectText.pivot = Vector2.zero;
            rectText.sizeDelta = new Vector2(350, 50);

            _inputFieldPrefab.textComponent = GameObjectFactory.CreateSingleText(inputTextHolder.transform, $"TextLabel", "", GameTheme.themeColors.leaderboard.text);
            _inputFieldPrefab.textComponent.rectTransform.pivot = new Vector2(0, 0.5f);
            _inputFieldPrefab.textComponent.alignment = TextAlignmentOptions.Left;
            _inputFieldPrefab.textComponent.margin = new Vector4(5, 0, 0, 0);
            _inputFieldPrefab.textComponent.enableWordWrapping = true;
            _inputFieldPrefab.textViewport = _inputFieldPrefab.textComponent.rectTransform;

            GameObject.DontDestroyOnLoad(_inputFieldPrefab);
        }

        private static void SetNotificationPrefab()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
            GameObject bufferPanel = mainCanvas.transform.Find("SettingsPanel/buffer_panel/window border").gameObject;

            GameObject gameObjectHolder = GameObject.Instantiate(bufferPanel);
            gameObjectHolder.name = "NotificationPrefab";
            GameObject.DestroyImmediate(gameObjectHolder.transform.Find("Window Body/all_settiings").gameObject);

            _popUpNotifPrefab = gameObjectHolder.AddComponent<PopUpNotif>();
            RectTransform popUpNorifRectTransform = _popUpNotifPrefab.GetComponent<RectTransform>();
            popUpNorifRectTransform.anchoredPosition = new Vector2(695, -700);
            popUpNorifRectTransform.sizeDelta = new Vector2(450, 200);

            TMP_Text notifText = GameObject.Instantiate(_multicoloreTextPrefab, _popUpNotifPrefab.transform);
            notifText.name = "NotifText";
            notifText.gameObject.GetComponent<RectTransform>().sizeDelta = popUpNorifRectTransform.sizeDelta;
            notifText.gameObject.SetActive(true);

            gameObjectHolder.SetActive(false);

            GameObject.DontDestroyOnLoad(_popUpNotifPrefab);
        }

        private static void SetBubblePrefab()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
            GameObject bufferPanel = mainCanvas.transform.Find("SettingsPanel/buffer_panel/window border").gameObject;

            _bubblePrefab = GameObject.Instantiate(bufferPanel);
            _bubblePrefab.name = "BubblePrefab";
            GameObject.DestroyImmediate(_bubblePrefab.transform.Find("Window Body/all_settiings").gameObject);

            CanvasGroup canvasGroup = _bubblePrefab.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;

            RectTransform bubblePrefabRect = _bubblePrefab.GetComponent<RectTransform>();
            bubblePrefabRect.sizeDelta = new Vector2(250, 100);
            bubblePrefabRect.pivot = new Vector2(1, 0);



            var text = GameObject.Instantiate(_multicoloreTextPrefab, _bubblePrefab.transform.Find("Window Body"));
            text.name = "BubbleText";
            text.maskable = false;
            text.enableWordWrapping = true;
            text.rectTransform.sizeDelta = bubblePrefabRect.sizeDelta;
            text.gameObject.SetActive(true);

            _bubblePrefab.SetActive(false);

            GameObject.DontDestroyOnLoad(_bubblePrefab);
        }


        private static void SetCustomButtonPrefab()
        {
            GameObject settingBtn = _settingsGraphics.transform.Find("GRAPHICS/btn_opengraphicspanel").gameObject;

            GameObject gameObjectHolder = UnityEngine.Object.Instantiate(settingBtn);

            var tempBtn = gameObjectHolder.GetComponent<Button>();
            var oldBtnColors = tempBtn.colors;

            UnityEngine.Object.DestroyImmediate(tempBtn);

            var myBtn = gameObjectHolder.AddComponent<Button>();
            myBtn.colors = oldBtnColors;


            _buttonPrefab = gameObjectHolder.AddComponent<CustomButton>();
            _buttonPrefab.ConstructNewButton(gameObjectHolder.GetComponent<Button>(), gameObjectHolder.GetComponentInChildren<Text>());

            gameObjectHolder.SetActive(false);

            UnityEngine.Object.DontDestroyOnLoad(gameObjectHolder);
        }

        private static void SetSteamLeaderboardPrefab()
        {
            GameObject camerapopups = GameObject.Find("Camera-Popups").gameObject;
            GameObject steamLeaderboardCanvas = camerapopups.transform.Find("LeaderboardCanvas").gameObject;

            _steamLeaderboardPrefab = GameObject.Instantiate(steamLeaderboardCanvas);
            _steamLeaderboardPrefab.name = "CustomLeaderboardCanvas";
            _steamLeaderboardPrefab.SetActive(true); //Has to be set to true else it crashes when yoinking other objects?? #UnityStuff

            //Don't think we need these...
            DestroyFromParent(_steamLeaderboardPrefab, "BG");
            GameObject.DestroyImmediate(_steamLeaderboardPrefab.GetComponent<CanvasScaler>());

            RectTransform lbCanvasRect = _steamLeaderboardPrefab.GetComponent<RectTransform>();
            lbCanvasRect.anchoredPosition = new Vector2(237, -311);
            lbCanvasRect.localScale = Vector2.one * 0.5f;

            SetPanelBodyInSteamLeaderboard();

            GameObject.DontDestroyOnLoad(_steamLeaderboardPrefab);
        }

        private static void SetPanelBodyInSteamLeaderboard()
        {
            _panelBodyPrefab = _steamLeaderboardPrefab.transform.Find("PanelBody").gameObject;
            _panelBodyPrefab.SetActive(true);

            RectTransform panelRectTransform = _panelBodyPrefab.GetComponent<RectTransform>();
            panelRectTransform.anchoredPosition = Vector2.zero;
            panelRectTransform.sizeDelta = new Vector2(750, 300);

            //We dont need these right?
            DestroyFromParent(_panelBodyPrefab, "CloseButton");
            DestroyFromParent(_panelBodyPrefab, "txt_legal");
            DestroyFromParent(_panelBodyPrefab, "txt_leaderboards");
            DestroyFromParent(_panelBodyPrefab, "txt_songname");
            DestroyFromParent(_panelBodyPrefab, "rule");
            DestroyFromParent(_panelBodyPrefab, "HelpBtn");
            DestroyFromParent(_panelBodyPrefab, "loadingspinner_parent");

            SetTabsInPanelBody();
            SetErrorsInPanelBody();
            SetScoreboardInPanelBody();
            AddSliderInPanelBody();
        }

        private static void SetTabsInPanelBody()
        {
            GameObject tabs = _panelBodyPrefab.transform.Find("tabs").gameObject;
            tabs.SetActive(false); //Hide until icons are loaded
            GameObject.DestroyImmediate(tabs.GetComponent<HorizontalLayoutGroup>());
            for (int i = 0; i < 3; i++)
            {
                GameObject currentTab = _steamLeaderboardPrefab.GetComponent<LeaderboardManager>().tabs[i];
                DestroyFromParent(currentTab, "label");
                DestroyFromParent(currentTab, "rule");

                RectTransform tabRect = currentTab.GetComponent<RectTransform>();
                tabRect.anchoredPosition = new Vector2(15, -40);
                tabRect.sizeDelta = new Vector2(40, 40);
            }
            VerticalLayoutGroup verticalLayout = tabs.AddComponent<VerticalLayoutGroup>();
            verticalLayout.childForceExpandWidth = false;
            verticalLayout.childScaleWidth = verticalLayout.childScaleHeight = false;
            verticalLayout.childControlWidth = verticalLayout.childControlHeight = false;
            verticalLayout.padding.left = 20;
            verticalLayout.padding.top = 36;

            RectTransform tabsRectTransform = tabs.GetComponent<RectTransform>();
            tabsRectTransform.anchoredPosition = new Vector2(328, -10);
            tabsRectTransform.sizeDelta = new Vector2(-676, 280);
        }

        private static void SetErrorsInPanelBody()
        {
            GameObject errorsHolder = _panelBodyPrefab.transform.Find("errors").gameObject;

            RectTransform errorsTransform = errorsHolder.GetComponent<RectTransform>();
            errorsTransform.anchoredPosition = new Vector2(-30, 15);
            errorsTransform.sizeDelta = new Vector2(-200, 0);

            errorsHolder.SetActive(false);

            errorsHolder.transform.Find("error_noleaderboard").gameObject.SetActive(true);
        }

        private static void SetScoreboardInPanelBody()
        {
            GameObject scoresbody = _panelBodyPrefab.transform.Find("scoresbody").gameObject;

            RectTransform scoresbodyRectTransform = scoresbody.GetComponent<RectTransform>();
            scoresbodyRectTransform.anchoredPosition = new Vector2(0, -10);
            scoresbodyRectTransform.sizeDelta = Vector2.one * -20;

            GameObject scoreboard = _panelBodyPrefab.transform.Find("scoreboard").gameObject; //Single scores goes in there

            scoreboard.AddComponent<RectMask2D>();
            RectTransform scoreboardRectTransform = scoreboard.GetComponent<RectTransform>();
            scoreboardRectTransform.anchoredPosition = new Vector2(-30, -10);
            scoreboardRectTransform.sizeDelta = new Vector2(-80, -20);
        }

        private static void AddSliderInPanelBody()
        {
            CreateVerticalSliderFromPrefab(_panelBodyPrefab.transform, "LeaderboardVerticalSlider");
        }

        private static void SetSingleScorePrefab()
        {
            GameObject singleScore = _panelBodyPrefab.transform.Find("scoreboard/SingleScore").gameObject;
            _singleScorePrefab = GameObject.Instantiate(singleScore);
            _singleScorePrefab.name = "singleScorePrefab";
            _singleScorePrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(_singleScorePrefab.GetComponent<RectTransform>().sizeDelta.x, 35);

            //find image. set the size and position and always enable the image
            GameObject imageGameObject = _singleScorePrefab.transform.Find("Image").gameObject;
            LayoutElement layoutElement = imageGameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            RectTransform imageRectTransform = imageGameObject.GetComponent<RectTransform>();
            imageRectTransform.sizeDelta = new Vector2(-5, 0);
            imageRectTransform.anchoredPosition = new Vector2(-10, 0);

            Image image = imageGameObject.GetComponent<Image>();
            image.enabled = true;
            image.maskable = true;

            _singleScorePrefab.gameObject.SetActive(false);

            GameObject.DontDestroyOnLoad(_singleScorePrefab.gameObject);
        }

        private static void SetLeaderboardHeaderPrefab()
        {
            Text tempHeaderTxt = GameObject.Instantiate(_singleScorePrefab.transform.Find("Num").GetComponent<Text>());
            _leaderboardHeaderPrefab = GameObject.Instantiate(_comfortaaTextPrefab);
            _leaderboardHeaderPrefab.name = "LeaderboardHeaderPrefab";
            _leaderboardHeaderPrefab.alignment = TextAlignmentOptions.Center;
            _leaderboardHeaderPrefab.maskable = true;
            _leaderboardHeaderPrefab.enableWordWrapping = false;
            _leaderboardHeaderPrefab.gameObject.SetActive(true);
            _leaderboardHeaderPrefab.enableAutoSizing = true;
            _leaderboardHeaderPrefab.fontSizeMax = _leaderboardHeaderPrefab.fontSize;

            GameObject.DestroyImmediate(tempHeaderTxt.gameObject);
            GameObject.DontDestroyOnLoad(_leaderboardHeaderPrefab.gameObject);
        }

        private static void SetLeaderboardTextPrefab()
        {
            Text tempTxt = GameObject.Instantiate(_singleScorePrefab.transform.Find("Name").GetComponent<Text>());
            _leaderboardTextPrefab = GameObject.Instantiate(_comfortaaTextPrefab);
            _leaderboardTextPrefab.name = "LeaderboardTextPrefab";
            _leaderboardTextPrefab.alignment = TextAlignmentOptions.Center;
            _leaderboardTextPrefab.maskable = true;
            _leaderboardTextPrefab.enableWordWrapping = false;
            _leaderboardTextPrefab.gameObject.SetActive(true);
            _leaderboardTextPrefab.color = Color.white;
            _leaderboardTextPrefab.enableAutoSizing = true;
            _leaderboardTextPrefab.fontSizeMax = _leaderboardTextPrefab.fontSize;


            DestroyNumNameScoreFromSingleScorePrefab();

            GameObject.DestroyImmediate(tempTxt.gameObject);
            GameObject.DontDestroyOnLoad(_leaderboardTextPrefab.gameObject);
        }

        private static void SetSingleRowPrefab()
        {
            _singleRowPrefab = _singleScorePrefab.AddComponent<LeaderboardRowEntry>();
            TMP_Text rank = GameObject.Instantiate(_leaderboardHeaderPrefab, _singleScorePrefab.transform);
            rank.name = "rank";
            TMP_Text username = GameObject.Instantiate(_leaderboardTextPrefab, _singleScorePrefab.transform);
            username.name = "username";
            TMP_Text score = GameObject.Instantiate(_leaderboardTextPrefab, _singleScorePrefab.transform);
            score.name = "score";
            TMP_Text percent = GameObject.Instantiate(_leaderboardTextPrefab, _singleScorePrefab.transform);
            percent.name = "percent";
            TMP_Text grade = GameObject.Instantiate(_leaderboardTextPrefab, _singleScorePrefab.transform);
            grade.name = "grade";
            TMP_Text maxcombo = GameObject.Instantiate(_leaderboardTextPrefab, _singleScorePrefab.transform);
            maxcombo.name = "maxcombo";
            _singleRowPrefab.ConstructLeaderboardEntry(_singleScorePrefab, rank, username, score, percent, grade, maxcombo, false);
            _singleRowPrefab.singleScore.name = "singleRowPrefab";
        }

        private static void SetVerticalSliderPrefab()
        {
            Slider defaultSlider = GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "Slider").GetComponent<Slider>(); //yoink

            _verticalSliderPrefab = GameObject.Instantiate(defaultSlider);
            _verticalSliderPrefab.direction = Slider.Direction.TopToBottom;

            RectTransform sliderRect = _verticalSliderPrefab.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(25, 745);
            sliderRect.anchoredPosition = new Vector2(300, 0);

            RectTransform fillAreaRect = _verticalSliderPrefab.transform.Find("Fill Area").GetComponent<RectTransform>();
            fillAreaRect.sizeDelta = new Vector2(-19, -2);
            fillAreaRect.anchoredPosition = new Vector2(-5, 0);

            RectTransform handleSlideAreaRect = _verticalSliderPrefab.transform.Find("Handle Slide Area").GetComponent<RectTransform>();
            RectTransform handleRect = handleSlideAreaRect.gameObject.transform.Find("Handle").GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(40, 40);
            handleRect.pivot = Vector2.zero;
            handleRect.anchorMax = Vector2.zero;
            GameObject handle = GameObject.Instantiate(handleRect.gameObject, _verticalSliderPrefab.transform);
            handle.name = "Handle";
            RectTransform backgroundSliderRect = _verticalSliderPrefab.transform.Find("Background").GetComponent<RectTransform>();
            backgroundSliderRect.anchoredPosition = new Vector2(-5, backgroundSliderRect.anchoredPosition.y);
            backgroundSliderRect.sizeDelta = new Vector2(-10, backgroundSliderRect.sizeDelta.y);

            _verticalSliderPrefab.value = 0f;
            _verticalSliderPrefab.minValue = -0.05f;
            _verticalSliderPrefab.maxValue = 1.04f;
            _verticalSliderPrefab.onValueChanged = new Slider.SliderEvent();
            _verticalSliderPrefab.gameObject.SetActive(false);

            DestroyFromParent(_verticalSliderPrefab.gameObject, "Handle Slide Area/Handle");

            GameObject.DontDestroyOnLoad(_verticalSliderPrefab);
        }
        private static void SetSliderPrefab()
        {
            Slider defaultSlider = GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "Slider").GetComponent<Slider>(); //yoink

            _sliderPrefab = GameObject.Instantiate(defaultSlider);
            //_sliderPrefab.transform.Find("Fill Area/Fill").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.fill;

            RectTransform sliderRect = _sliderPrefab.GetComponent<RectTransform>();
            sliderRect.anchoredPosition = new Vector2(-200, 0);

            RectTransform backgroundSliderRect = _sliderPrefab.transform.Find("Background").GetComponent<RectTransform>();
            backgroundSliderRect.anchoredPosition = new Vector2(-5, backgroundSliderRect.anchoredPosition.y);
            backgroundSliderRect.sizeDelta = new Vector2(-10, backgroundSliderRect.sizeDelta.y);

            _sliderPrefab.value = 1f;
            _sliderPrefab.minValue = 0f;
            _sliderPrefab.maxValue = 2f;
            _sliderPrefab.onValueChanged = new Slider.SliderEvent();
            _sliderPrefab.gameObject.SetActive(false);


            GameObject.DontDestroyOnLoad(_sliderPrefab);
        }

        private static void DestroyNumNameScoreFromSingleScorePrefab()
        {
            DestroyFromParent(_singleScorePrefab, "Num");
            DestroyFromParent(_singleScorePrefab, "Name");
            DestroyFromParent(_singleScorePrefab, "Score");
        }

        private static void SetOverlayPanelPrefab()
        {
            _overlayPanelPrefab = GameObject.Instantiate(_creditPanel);
            _overlayPanelPrefab.name = "OverlayPanelPrefab";
            _overlayPanelPrefab.transform.localScale = Vector3.one;
            _overlayPanelPrefab.SetActive(false);
            _overlayPanelPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);

            GameObject fsLatencyPanel = _overlayPanelPrefab.transform.Find("FSLatencyPanel").gameObject;
            fsLatencyPanel.SetActive(true);
            GameObject.DestroyImmediate(fsLatencyPanel.transform.Find("LatencyBG2").gameObject);
            fsLatencyPanel.transform.Find("LatencyBG").gameObject.GetComponent<Image>().color = GameTheme.themeColors.notification.border;

            GameObject latencyFGPanel = fsLatencyPanel.transform.Find("LatencyFG").gameObject; //this where most objects are located
            latencyFGPanel.GetComponent<Image>().color = GameTheme.themeColors.notification.background;
            DestroyFromParent(latencyFGPanel, "page2");
            DestroyFromParent(latencyFGPanel, "page3");
            DestroyFromParent(latencyFGPanel, "page4");
            DestroyFromParent(latencyFGPanel, "page5");
            DestroyFromParent(latencyFGPanel, "PREV");

            Text title = latencyFGPanel.transform.Find("title").gameObject.GetComponent<Text>();
            Text subtitle = latencyFGPanel.transform.Find("subtitle").gameObject.GetComponent<Text>();
            title.text = "TootTally Panel";
            title.color = GameTheme.themeColors.notification.defaultText;
            subtitle.text = "TootTally Panel Early Version Description";
            subtitle.color = GameTheme.themeColors.notification.defaultText;


            GameObject mainPage = latencyFGPanel.transform.Find("page1").gameObject;
            GameObject.DestroyImmediate(mainPage.GetComponent<HorizontalLayoutGroup>());
            VerticalLayoutGroup vgroup = mainPage.AddComponent<VerticalLayoutGroup>();
            vgroup.childForceExpandHeight = vgroup.childScaleHeight = vgroup.childControlHeight = false;
            vgroup.childForceExpandWidth = vgroup.childScaleWidth = vgroup.childControlWidth = false;
            vgroup.padding.left = (int)(mainPage.GetComponent<RectTransform>().sizeDelta.x / 2) - 125;
            vgroup.spacing = 20;
            mainPage.name = "MainPage";

            DestroyFromParent(mainPage, "col1");
            DestroyFromParent(mainPage, "col2");
            DestroyFromParent(mainPage, "col3");
            DestroyFromParent(latencyFGPanel, "CloseBtn");
            DestroyFromParent(latencyFGPanel, "NEXT");

            GameObject.DontDestroyOnLoad(_overlayPanelPrefab);
        }


        private static void SetUserCardPrefab()
        {
            _userCardPrefab = GameObject.Instantiate(_overlayPanelPrefab.transform.Find("FSLatencyPanel").gameObject);
            _userCardPrefab.name = "UserCardPrefab";
            _userCardPrefab.GetComponent<Image>().color = new Color(0, 0, 0, 0);


            var fgRect = _userCardPrefab.transform.Find("LatencyFG").GetComponent<RectTransform>();
            var bgRect = _userCardPrefab.transform.Find("LatencyBG").GetComponent<RectTransform>();
            fgRect.GetComponent<Image>().maskable = bgRect.GetComponent<Image>().maskable = true;
            var size = new Vector2(360, 100);
            fgRect.sizeDelta = size;
            fgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = size + (Vector2.one * 10f);
            bgRect.anchoredPosition = Vector2.zero;
            _userCardPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            var horizontalContentHolder = fgRect.gameObject;
            DestroyFromParent(horizontalContentHolder, "title");
            DestroyFromParent(horizontalContentHolder, "subtitle");

            var horizontalLayoutGroup = horizontalContentHolder.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.padding = new RectOffset(5, 5, 5, 5);
            horizontalLayoutGroup.spacing = 20f;
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            horizontalLayoutGroup.childControlHeight = horizontalLayoutGroup.childControlWidth = true;
            horizontalLayoutGroup.childForceExpandHeight = horizontalLayoutGroup.childForceExpandWidth = false;



            var contentHolderLeft = horizontalContentHolder.transform.Find("MainPage").gameObject;
            contentHolderLeft.name = "LeftContent";

            var verticalLayoutGroup = contentHolderLeft.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.padding = new RectOffset(5, 5, 5, 5);
            verticalLayoutGroup.spacing = 4f;
            verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            verticalLayoutGroup.childControlHeight = verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childForceExpandHeight = verticalLayoutGroup.childForceExpandWidth = true;

            var contentHolderRight = GameObject.Instantiate(contentHolderLeft, horizontalContentHolder.transform);
            contentHolderRight.name = "RightContent";
            var verticalLayoutGroupRight = contentHolderRight.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroupRight.childControlHeight = verticalLayoutGroupRight.childControlWidth = false;
            verticalLayoutGroupRight.childForceExpandHeight = verticalLayoutGroupRight.childForceExpandWidth = false;

            var outlineTemp = new GameObject("PFPPrefab", typeof(Image));
            var outlineImage = outlineTemp.GetComponent<Image>();
            outlineImage.maskable = true;
            outlineImage.preserveAspect = true;

            var maskTemp = GameObject.Instantiate(outlineTemp, outlineTemp.transform);
            maskTemp.name = "ImageMask";
            var pfpTemp = GameObject.Instantiate(maskTemp, maskTemp.transform);
            pfpTemp.name = "Image";

            var mask = maskTemp.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var maskImage = maskTemp.GetComponent<Image>();
            maskImage.sprite = AssetManager.GetSprite("PfpMask.png");
            maskTemp.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 90);

            var pfpImage = pfpTemp.GetComponent<Image>();
            outlineTemp.transform.SetSiblingIndex(0);
            outlineImage.enabled = false;
            pfpImage.sprite = AssetManager.GetSprite("icon.png");
            pfpImage.preserveAspect = false;

            var layoutElement = outlineTemp.AddComponent<LayoutElement>();
            layoutElement.minHeight = layoutElement.minWidth = 96;
            var pfp = GameObject.Instantiate(outlineTemp, horizontalContentHolder.transform);
            pfp.transform.SetSiblingIndex(0);
            pfp.name = "PFP";
            GameObject.DestroyImmediate(outlineTemp);

            GameObject.DontDestroyOnLoad(_userCardPrefab);
            _userCardPrefab.SetActive(false);
        }

        #endregion

        #region Create Objects

        public static SpectatingViewerIcon CreateDefaultViewerIcon(Transform canvasTransform, string name) => new SpectatingViewerIcon(canvasTransform, Vector2.zero, Vector2.one * 80, name);

        public static LoadingIcon CreateLoadingIcon(Transform canvasTransform, Vector2 position, Vector2 size, Sprite sprite, bool isActive, string name) =>
            new LoadingIcon(CreateImageHolder(canvasTransform, position, size, sprite, name), isActive);

        public static GameObject CreateImageHolder(Transform canvasTransform, Vector2 position, Vector2 size, Sprite sprite, string name)
        {
            GameObject imageHolder = new GameObject(name, typeof(Image));
            imageHolder.transform.SetParent(canvasTransform);

            var rect = imageHolder.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;

            Image image = imageHolder.GetComponent<Image>();
            image.preserveAspect = true;
            image.color = Color.white;
            image.sprite = sprite;

            image.gameObject.SetActive(true);

            return imageHolder;
        }

        public static GameObject CreateClickableImageHolder(Transform canvasTransform, Vector2 position, Vector2 size, Sprite sprite, string name, Action onClick)
        {
            var imageHolder = CreateImageHolder(canvasTransform, position, size, sprite, name);
            var eventTrigger = imageHolder.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerClickEvent = new EventTrigger.Entry();
            pointerClickEvent.eventID = EventTriggerType.PointerClick;
            pointerClickEvent.callback.AddListener((data) => onClick?.Invoke());
            eventTrigger.triggers.Add(pointerClickEvent);

            return imageHolder;
        }

        public static ProgressBar CreateProgressBar(Transform canvasTransform, Vector2 position, Vector2 size, bool active, string name)
        {
            Slider slider = GameObject.Instantiate(_settingsPanelVolumeSlider, canvasTransform);
            DestroyFromParent(slider.gameObject, "Handle Slide Area");
            slider.name = name;
            slider.onValueChanged = new Slider.SliderEvent();
            RectTransform rect = slider.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.2f); //line up centered and slightly higher than the lowest point
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.pivot = new Vector2(0.5f, 1f);
            slider.minValue = slider.value = 0f;
            slider.maxValue = 1f;
            slider.interactable = false;
            slider.wholeNumbers = false;
            ProgressBar bar = new ProgressBar(slider, active);
            return bar;
        }

        public static GameObject CreateUserCard(Transform canvasTransform, SerializableClass.User user, string status)
        {
            GameObject card = GameObject.Instantiate(_userCardPrefab, canvasTransform);
            card.name = $"{user.username}UserCard";
            card.SetActive(true);

            var leftContent = card.transform.Find("LatencyFG/LeftContent").gameObject;

            var pfp = leftContent.transform.parent.Find("PFP/ImageMask/Image").GetComponent<Image>();
            if (user.picture != null)
                AssetManager.GetProfilePictureByID(user.id, (sprite) => pfp.sprite = sprite);


            var t1 = CreateSingleText(leftContent.transform, "Name", $"{user.username}", GameTheme.themeColors.leaderboard.text);
            var t2 = CreateSingleText(leftContent.transform, "Status", $"{status}", GameTheme.themeColors.leaderboard.text);
            t1.enableWordWrapping = t2.enableWordWrapping = false;
            t1.overflowMode = t2.overflowMode = TextOverflowModes.Ellipsis;

            var rightContent = card.transform.Find("LatencyFG/RightContent").gameObject;

            if (user.id != Plugin.userInfo.id)
            {
                var bgColor = card.transform.Find("LatencyBG").GetComponent<Image>().color = UserFriendStatusToColor(user.friend_status);
                TintImage(card.transform.Find("LatencyFG").GetComponent<Image>(), bgColor, .1f);
                if (user.friend_status == "Friend" || user.friend_status == "Mutuals")
                    CreateCustomButton(rightContent.transform, Vector2.zero, new Vector2(30, 30), "-", "RemoveFriendButton", delegate { TootTallyOverlayManager.OnRemoveButtonPress(user); });
                else
                    CreateCustomButton(rightContent.transform, Vector2.zero, new Vector2(30, 30), "+", "AddFriendButton", delegate { TootTallyOverlayManager.OnAddButtonPress(user); });
                CreateCustomButton(rightContent.transform, Vector2.zero, new Vector2(30, 30), "P", "OpenProfileButton", delegate { TootTallyOverlayManager.OpenUserProfile(user.id); });
                if (SpectatingManager.currentSpectatorIDList.Contains(user.id))
                    CreateCustomButton(rightContent.transform, Vector2.zero, new Vector2(30, 30), "S", "SpectateUserButton", delegate { TootTallyOverlayManager.OnSpectateButtonPress(user.id, user.username); });
            }
            else
            {
                card.transform.Find("LatencyBG").GetComponent<Image>().color = Color.cyan;
                TintImage(card.transform.Find("LatencyFG").GetComponent<Image>(), Color.cyan, .1f);
                if (!SpectatingManager.IsHosting)
                    CreateCustomButton(rightContent.transform, Vector2.zero, new Vector2(30, 30), "H", "SpectateUserButton", delegate { TootTallyOverlayManager.OnSpectateButtonPress(user.id, user.username); });
            }

            return card;
        }

        private static void TintImage(Image image, Color tint, float percent) =>
            image.color = new Color(image.color.r * (1f - percent) + tint.r * percent, image.color.g * (1f - percent) + tint.g * percent, image.color.b * (1f - percent) + tint.b * percent);

        private static Color UserFriendStatusToColor(string status) =>
            status switch
            {
                "Friend" => new Color(0, .8f, 0, 1),
                "Mutuals" => new Color(1, 0, 1, 1),
                _ => new Color(0, 0, 0, 1),
            };

        public static TootTallyLoginPanel CreateLoginPanelV2()
        {
            TootTallyLoginPanel panel = new TootTallyLoginPanel();
            return panel;
        }


        public static GameObject CreateOverlayPanel(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, float borderThiccness, string name)
        {
            GameObject overlayPanel = GameObject.Instantiate(_overlayPanelPrefab, canvasTransform);
            overlayPanel.name = name;
            var fgRect = overlayPanel.transform.Find("FSLatencyPanel/LatencyFG").GetComponent<RectTransform>();
            var bgRect = overlayPanel.transform.Find("FSLatencyPanel/LatencyBG").GetComponent<RectTransform>();
            fgRect.sizeDelta = size;
            fgRect.anchoredPosition = anchoredPosition;
            bgRect.sizeDelta = size + (Vector2.one * borderThiccness);
            bgRect.anchoredPosition = anchoredPosition;
            overlayPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            overlayPanel.SetActive(true);

            return overlayPanel;
        }

        public static CustomButton CreateCustomButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, string text, string name, Action onClick = null)
        {
            CustomButton newButton = UnityEngine.Object.Instantiate(_buttonPrefab, canvasTransform);
            newButton.name = name;
            ColorBlock btnColors = newButton.button.colors;
            btnColors.normalColor = GameTheme.themeColors.replayButton.colors.normalColor;
            btnColors.highlightedColor = GameTheme.themeColors.replayButton.colors.highlightedColor;
            btnColors.pressedColor = GameTheme.themeColors.replayButton.colors.pressedColor;
            btnColors.selectedColor = GameTheme.themeColors.replayButton.colors.normalColor;
            newButton.button.colors = btnColors;

            newButton.textHolder.text = text;
            newButton.textHolder.alignment = TextAnchor.MiddleCenter;
            newButton.textHolder.fontSize = 22;
            newButton.textHolder.horizontalOverflow = HorizontalWrapMode.Wrap;
            newButton.textHolder.verticalOverflow = VerticalWrapMode.Overflow;
            newButton.textHolder.color = GameTheme.themeColors.replayButton.text;


            newButton.GetComponent<RectTransform>().sizeDelta = size;
            newButton.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

            newButton.button.onClick.AddListener(() => onClick?.Invoke());

            newButton.gameObject.SetActive(true);
            return newButton;
        }

        public static CustomButton CreateDefaultButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, string text, int fontsize, string name, Action onClick = null)
        {
            CustomButton newButton = UnityEngine.Object.Instantiate(_buttonPrefab, canvasTransform);
            newButton.name = name;
            newButton.textHolder.text = text;
            newButton.textHolder.alignment = TextAnchor.MiddleCenter;
            newButton.textHolder.fontSize = fontsize;
            newButton.textHolder.horizontalOverflow = HorizontalWrapMode.Wrap;
            newButton.textHolder.verticalOverflow = VerticalWrapMode.Overflow;
            newButton.GetComponent<RectTransform>().sizeDelta = size;
            newButton.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

            newButton.button.onClick.AddListener(() => onClick?.Invoke());

            newButton.gameObject.SetActive(true);
            return newButton;
        }

        //Backward Compatibility
        public static CustomButton CreateCustomButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, Sprite sprite, string name, Action onClick = null)
        => CreateCustomButton(canvasTransform, anchoredPosition, size, sprite, true, name, onClick);

        public static CustomButton CreateCustomButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, Sprite sprite, bool isImageThemable, string name, Action onClick = null)
        {
            CustomButton newButton = UnityEngine.Object.Instantiate(_buttonPrefab, canvasTransform);
            newButton.name = name;
            //newButton.button.GetComponent<Image>().sprite = sprite;

            ColorBlock btnColors = newButton.button.colors;
            btnColors.normalColor = GameTheme.themeColors.replayButton.colors.normalColor;
            btnColors.highlightedColor = GameTheme.themeColors.replayButton.colors.highlightedColor;
            btnColors.pressedColor = GameTheme.themeColors.replayButton.colors.pressedColor;
            btnColors.selectedColor = GameTheme.themeColors.replayButton.colors.normalColor;
            newButton.button.colors = btnColors;

            GameObject imageHolder = newButton.textHolder.gameObject;
            imageHolder.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            imageHolder.transform.localScale = new Vector3(.81f, .81f);
            GameObject.DestroyImmediate(imageHolder.GetComponent<Text>());
            Image image = imageHolder.AddComponent<Image>();
            image.color = isImageThemable ? GameTheme.themeColors.replayButton.text : Color.white;
            image.preserveAspect = true;
            image.maskable = true;
            image.sprite = sprite;

            newButton.GetComponent<RectTransform>().sizeDelta = size;
            newButton.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

            newButton.button.onClick.AddListener(() => onClick?.Invoke());

            newButton.gameObject.SetActive(true);
            return newButton;
        }

        public static GameObject CreateModifierButton(Transform canvasTransform, Sprite sprite, string name, string description, bool active, Action onClick = null)
        {
            var btn = CreateCustomButton(canvasTransform, new Vector2(350, -200), new Vector2(32, 32), sprite, name, onClick).gameObject;
            if (description != "")
                btn.AddComponent<BubblePopupHandler>().Initialize(CreateBubble(Vector2.zero, $"{name}Bubble", description, 6, true, 16));
            var glow = new GameObject("glow", typeof(Image));
            var image = glow.GetComponent<Image>();
            image.maskable = true;
            image.sprite = AssetManager.GetSprite("glow.png");
            glow.transform.SetParent(btn.transform);
            glow.transform.localScale = Vector3.one / 1.2f;
            glow.SetActive(active);
            image.color = GameTheme.themeColors.replayButton.text;
            var rect = btn.GetComponent<RectTransform>();
            rect.pivot = Vector2.one / 2f;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.localScale = Vector2.zero;
            rect.eulerAngles = active ? new Vector3(0, 0, 8) : Vector3.zero;
            return btn;
        }

        public static void UpdatePrefabTheme()
        {
            if (_isHomeControllerInitialized)
            {
                _popUpNotifPrefab.GetComponent<Image>().color =
                    _bubblePrefab.GetComponent<Image>().color = GameTheme.themeColors.notification.border;

                _popUpNotifPrefab.transform.Find("Window Body").gameObject.GetComponent<Image>().color =
                    _bubblePrefab.transform.Find("Window Body").gameObject.GetComponent<Image>().color = GameTheme.themeColors.notification.background;

                _popUpNotifPrefab.transform.Find("NotifText").GetComponent<TMP_Text>().color =
                   _bubblePrefab.transform.Find("Window Body/BubbleText").GetComponent<TMP_Text>().color = GameTheme.themeColors.notification.defaultText;

                _popUpNotifPrefab.transform.Find("NotifText").GetComponent<TMP_Text>().outlineColor =
                   _bubblePrefab.transform.Find("Window Body/BubbleText").GetComponent<TMP_Text>().outlineColor = GameTheme.themeColors.notification.textOutline;

                _overlayPanelPrefab.transform.Find("FSLatencyPanel/LatencyBG").gameObject.GetComponent<Image>().color = GameTheme.themeColors.notification.border;
                _overlayPanelPrefab.transform.Find("FSLatencyPanel/LatencyFG").gameObject.GetComponent<Image>().color = GameTheme.themeColors.notification.background;
                _userCardPrefab.transform.Find("LatencyBG").gameObject.GetComponent<Image>().color = GameTheme.themeColors.notification.border;
                _userCardPrefab.transform.Find("LatencyFG").gameObject.GetComponent<Image>().color = GameTheme.themeColors.notification.background;


                TootTallyOverlayManager.UpdateTheme();
            }

            if (_isLevelSelectControllerInitialized)
            {
                _panelBodyPrefab.GetComponent<Image>().color = GameTheme.themeColors.leaderboard.panelBody;
                for (int i = 0; i < 3; i++)
                {
                    GameObject currentTab = _steamLeaderboardPrefab.GetComponent<LeaderboardManager>().tabs[i];
                    ColorBlock colors = currentTab.transform.Find("Button").gameObject.GetComponent<Button>().colors;
                    colors.normalColor = GameTheme.themeColors.leaderboard.tabs.normalColor;
                    colors.pressedColor = GameTheme.themeColors.leaderboard.tabs.pressedColor;
                    colors.highlightedColor = GameTheme.themeColors.leaderboard.tabs.highlightedColor;
                    currentTab.transform.Find("Button").gameObject.GetComponent<Button>().colors = colors;
                }
                _panelBodyPrefab.transform.Find("scoresbody").gameObject.GetComponent<Image>().color = GameTheme.themeColors.leaderboard.scoresBody;
                _singleRowPrefab.UpdateTheme();

                _leaderboardTextPrefab.color = GameTheme.themeColors.leaderboard.text;
                _leaderboardTextPrefab.outlineColor = GameTheme.themeColors.leaderboard.textOutline;
                _leaderboardHeaderPrefab.color = GameTheme.themeColors.leaderboard.headerText;
                _leaderboardHeaderPrefab.outlineColor = GameTheme.themeColors.leaderboard.textOutline;

                _sliderPrefab.transform.Find("Fill Area/Fill").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.fill;
                _verticalSliderPrefab.transform.Find("Handle").gameObject.GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.handle;
                _verticalSliderPrefab.transform.Find("Fill Area/Fill").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.fill;
                _verticalSliderPrefab.transform.Find("Background").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.background;
            }
        }

        public static GameObject CreateDefaultPanel(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, string name)
        {
            GameObject panel = GameObject.Instantiate(_panelBodyPrefab, canvasTransform);
            panel.name = name;
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
            rectTransform.localScale = Vector2.one * .5f;

            DestroyFromParent(panel, "scoreboard");
            DestroyFromParent(panel, "tabs");
            DestroyFromParent(panel, "errors");
            DestroyFromParent(panel, "LeaderboardVerticalSlider");

            return panel;
        }

        public static TMP_InputField CreateInputField(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, string name, bool isPassword)
        {
            TMP_InputField inputField = GameObject.Instantiate(_inputFieldPrefab, canvasTransform);
            inputField.name = name;
            inputField.inputType = isPassword ? TMP_InputField.InputType.Password : TMP_InputField.InputType.Standard;

            //Have to do this so the input text actually updates after we're done typing :derp:
            inputField.onEndEdit.AddListener((text) => inputField.text = text);

            inputField.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
            inputField.GetComponent<RectTransform>().sizeDelta = size;

            return inputField;
        }

        public static GameObject CreateSteamLeaderboardFromPrefab(Transform canvasTransform, string name)
        {
            GameObject steamLeaderboard = GameObject.Instantiate(_steamLeaderboardPrefab, canvasTransform);
            steamLeaderboard.name = name;
            return steamLeaderboard;
        }

        public static Slider CreateVerticalSliderFromPrefab(Transform canvasTransform, string name)
        {
            Slider slider = GameObject.Instantiate(_verticalSliderPrefab, canvasTransform);
            slider.name = name;
            return slider;
        }

        public static Slider CreateSliderFromPrefab(Transform canvasTransform, string name)
        {
            Slider slider = GameObject.Instantiate(_sliderPrefab, canvasTransform);
            slider.name = name;
            return slider;
        }

        public static TMP_Text CreateSingleText(Transform canvasTransform, string name, string text, Color color, TextFont textFont = TextFont.Comfortaa) => CreateSingleText(canvasTransform, name, text, new Vector2(0,1), canvasTransform.GetComponent<RectTransform>().sizeDelta, color, textFont);
        public static TMP_Text CreateSingleText(Transform canvasTransform, string name, string text, Vector2 pivot, Vector2 size, Color color, TextFont textFont = TextFont.Comfortaa)
        {
            TMP_Text singleText;
            switch (textFont)
            {
                case TextFont.Multicolore:
                    singleText = GameObject.Instantiate(_multicoloreTextPrefab, canvasTransform);
                    break;
                default:
                    singleText = GameObject.Instantiate(_comfortaaTextPrefab, canvasTransform);
                    break;
            }
            singleText.name = name;

            singleText.text = text;
            singleText.color = color;
            singleText.gameObject.GetComponent<RectTransform>().pivot = pivot;
            singleText.gameObject.GetComponent<RectTransform>().sizeDelta = size;
            singleText.enableWordWrapping = true;

            singleText.gameObject.SetActive(true);

            return singleText;
        }

        public static TMP_Text CreateDoubleText(Transform canvasTransform, string name, string text, Color color)
        {
            TMP_Text doubledText = GameObject.Instantiate(_leaderboardTextPrefab, canvasTransform);
            doubledText.name = name;

            doubledText.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            doubledText.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, 15);
            doubledText.text = text;
            doubledText.color = color;

            return doubledText;
        }

        public static TMP_Text CreateTripleText(Transform canvasTransform, string name, string text, Color color)
        {
            TMP_Text tripledText = GameObject.Instantiate(_leaderboardTextPrefab, canvasTransform);
            tripledText.name = name;

            tripledText.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            tripledText.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, 15);
            tripledText.text = text;


            TMP_Text tripledTextSecondLayer = GameObject.Instantiate(_leaderboardTextPrefab, canvasTransform);
            tripledTextSecondLayer.name = name + "SecondLayer";

            tripledTextSecondLayer.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            tripledTextSecondLayer.GetComponent<RectTransform>().anchoredPosition = new Vector2(6, 12);
            tripledTextSecondLayer.text = text;
            tripledTextSecondLayer.color = color;

            return tripledText;
        }

        public static LeaderboardRowEntry CreateLeaderboardRowEntryFromScore(Transform canvasTransform, string name, SerializableClass.ScoreDataFromDB scoreData, int count, Color gradeColor, LevelSelectController levelSelectControllerInstance)
        {
            LeaderboardRowEntry rowEntry = GameObject.Instantiate(_singleRowPrefab, canvasTransform);
            rowEntry.name = name;
            rowEntry.username.text = scoreData.player;
            rowEntry.score.text = string.Format("{0:n0}", scoreData.score) + $" ({scoreData.replay_speed:0.00}x)";
            rowEntry.score.gameObject.AddComponent<BubblePopupHandler>().Initialize(CreateBubble(new Vector2(175, 200), $"{rowEntry.name}ScoreBubble", GetTallyBubbleText(scoreData.GetTally), 10, false));
            rowEntry.rank.text = "#" + count;
            rowEntry.percent.text = scoreData.percentage.ToString("0.00") + "%";
            rowEntry.grade.text = scoreData.grade;
            if (scoreData.grade == "SS")
            {
                rowEntry.grade.text = "S";
                GameObjectFactory.CreateDoubleText(rowEntry.grade.transform, "DoubleS" + scoreData.player + "Text", "S", Color.yellow);

            }
            else if (scoreData.grade == "SSS")
            {
                rowEntry.grade.text = "S";
                GameObjectFactory.CreateTripleText(rowEntry.grade.transform, "TripleS" + scoreData.player + "Text", "S", Color.yellow);
            }
            else
                rowEntry.grade.color = gradeColor;
            if (scoreData.is_rated)
            {

                rowEntry.maxcombo.text = (int)scoreData.tt + "tt";
                rowEntry.maxcombo.gameObject.AddComponent<BubblePopupHandler>().Initialize(CreateBubble(new Vector2(150, 75), $"{rowEntry.name}ComboBubble", $"{scoreData.max_combo} combo", 10, true));
            }
            else
                rowEntry.maxcombo.text = scoreData.max_combo + "x";
            rowEntry.replayId = scoreData.replay_id;
            rowEntry.rowId = count;
            rowEntry.singleScore.AddComponent<CanvasGroup>();
            HorizontalLayoutGroup layoutGroup = rowEntry.singleScore.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childForceExpandWidth = layoutGroup.childForceExpandHeight = false;
            layoutGroup.childScaleWidth = layoutGroup.childScaleHeight = false;
            layoutGroup.childControlWidth = layoutGroup.childControlHeight = false;
            layoutGroup.spacing = 8;
            layoutGroup.padding.left = 8;
            layoutGroup.padding.top = 2;
            rowEntry.singleScore.SetActive(true);
            rowEntry.singleScore.transform.Find("Image").gameObject.SetActive(count % 2 == 0);

            var replayId = rowEntry.replayId;
            if (replayId != "" && replayId != null) //if there's a uuid, add a replay button
            {
                CreateCustomButton(rowEntry.singleScore.transform, Vector2.zero, new Vector2(26, 26), "►", "ReplayButton",
                delegate
                {
                    TootTallyLogger.LogInfo("ID:" + replayId);
                    ReplaySystemManager.ResolveLoadReplay(replayId, levelSelectControllerInstance);
                });
            }
            return rowEntry;
        }

        private static string GetTallyBubbleText(int[] tally) =>
            tally != null ? $"Perfect: {tally[4]}\n" +
                            $"Nice: {tally[3]}\n" +
                            $"Okay: {tally[2]}\n" +
                            $"Meh: {tally[1]}\n" +
                            $"Nasty: {tally[0]}\n" : "No Tally";

        public static GameObject CreateBubble(Vector2 size, string name, string text) => CreateBubble(size, name, text, new Vector2(1, 0), 6, false);
        public static GameObject CreateBubble(Vector2 size, string name, string text, int borderThiccness, bool autoResize, int fontSize = 22) => CreateBubble(size, name, text, new Vector2(1, 0), 6, autoResize, fontSize);

        public static GameObject CreateBubble(Vector2 size, string name, string text, Vector2 alignement, int borderThiccness, bool autoResize, int fontSize = 22)
        {
            var bubble = GameObject.Instantiate(_bubblePrefab);
            bubble.name = name;
            bubble.GetComponent<RectTransform>().sizeDelta = size;
            bubble.GetComponent<RectTransform>().pivot = alignement;
            var windowbody = bubble.transform.Find("Window Body");
            windowbody.GetComponent<RectTransform>().sizeDelta = -(Vector2.one * borderThiccness);

            var textObj = windowbody.Find("BubbleText").GetComponent<TMP_Text>();
            textObj.text = text;
            textObj.fontSize = fontSize;
            textObj.fontSizeMax = fontSize;
            textObj.rectTransform.sizeDelta = size;
            textObj.margin = Vector3.one * borderThiccness;

            if (autoResize)
            {
                AddAutoSizeToObject(windowbody.gameObject, borderThiccness);
                AddAutoSizeToObject(bubble, borderThiccness);
            }
            else
                textObj.enableAutoSizing = true;

            return bubble;
        }

        private static void AddAutoSizeToObject(GameObject obj, int padding)
        {
            var contentFitter = obj.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vertical = obj.AddComponent<VerticalLayoutGroup>();
            vertical.childForceExpandHeight = vertical.childForceExpandWidth = false;
            vertical.childScaleHeight = vertical.childScaleWidth = false;
            vertical.childControlHeight = vertical.childControlWidth = true;
            vertical.padding = new RectOffset(padding, padding, padding, padding);
        }

        public static GameObject CreateBubble(Vector2 size, string name, Sprite sprite)
        {
            var imageObj = CreateImageHolder(null, Vector2.zero, size, sprite, name);
            imageObj.GetComponent<Image>().maskable = false;
            imageObj.AddComponent<CanvasGroup>().blocksRaycasts = false;
            return imageObj;
        }


        public static PopUpNotif CreateNotif(Transform canvasTransform, string name, string text, Color textColor)
        {
            PopUpNotif notif = GameObject.Instantiate(_popUpNotifPrefab, canvasTransform);

            notif.name = name;
            notif.SetTextColor(textColor);
            notif.SetText(text);

            return notif;
        }


        #endregion

        public static void DestroyFromParent(GameObject parent, string objectName)
        {
            try
            {
                GameObject.DestroyImmediate(parent.transform.Find(objectName).gameObject);
            }
            catch (Exception)
            {
                TootTallyLogger.LogError($"Object {objectName} couldn't be deleted from {parent.name} parent");
            }
        }

        public enum TextFont
        {
            Comfortaa,
            Multicolore
        }
    }
}
