using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TootTally.CustomLeaderboard;
using TootTally.Graphics.Animation;
using TootTally.Replays;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public static class GameObjectFactory
    {
        private static CustomButton _buttonPrefab;
        private static Text _defaultText, _leaderboardHeaderPrefab, _leaderboardTextPrefab;
        private static Slider _verticalSliderPrefab, _sliderPrefab;
        private static PopUpNotif _popUpNotifPrefab;

        private static GameObject _settingsGraphics, _steamLeaderboardPrefab, _singleScorePrefab, _panelBodyPrefab;
        private static LeaderboardRowEntry _singleRowPrefab;

        private static GameObject _mainMultiplayerPanelPrefab;

        private static bool _isHomeControllerInitialized;
        private static bool _isLevelSelectControllerInitialized;
        private static bool _isPlaytestAnimsInitialized;



        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        static void YoinkSettingsGraphicsHomeController(HomeController __instance)
        {
            _settingsGraphics = __instance.fullsettingspanel.transform.Find("Settings").gameObject;
            OnHomeControllerInitialize();
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void YoinkGraphicsLevelSelectController()
        {
            OnLevelSelectControllerInitialize();
        }

        [HarmonyPatch(typeof(PlaytestAnims), nameof(PlaytestAnims.Start))]
        [HarmonyPostfix]
        static void YoinkGraphicsPlaytestAnims(PlaytestAnims __instance)
        {
            OnPlaytestAnimsInitialize(__instance);
        }

        public static void OnHomeControllerInitialize()
        {

            if (_isHomeControllerInitialized) return;

            SetDefaultTextPrefab();
            SetNotificationPrefab();
            SetCustomButtonPrefab();
            _isHomeControllerInitialized = true;
            UpdatePrefabTheme();
        }

        public static void OnLevelSelectControllerInitialize()
        {
            if (_isLevelSelectControllerInitialized) return;

            SetSliderPrefab();
            SetVerticalSliderPrefab();
            SetSteamLeaderboardPrefab();
            SetSingleScorePrefab();
            SetLeaderboardHeaderPrefab();
            SetLeaderboardTextPrefab();
            SetSingleRowPrefab();
            _isLevelSelectControllerInitialized = true;
            UpdatePrefabTheme();
        }

        public static void OnPlaytestAnimsInitialize(PlaytestAnims __instance)
        {
            if (_isPlaytestAnimsInitialized) return;

            SetMainMultiplayerPanelPrefab(__instance);

            _isPlaytestAnimsInitialized = true;

        }


        #region SetPrefabs

        public static void SetMainMultiplayerPanelPrefab(PlaytestAnims __instance)
        {
            GameObject factPanel = __instance.factpanel.gameObject;

            GameObject.DestroyImmediate(factPanel.transform.Find("Panelbg2").gameObject);
            factPanel.transform.Find("top").GetComponent<Image>().color = Color.green;

            GameObject border = factPanel.transform.Find("Panelbg1").gameObject;
            border.GetComponent<Image>().color = Color.green;

            GameObject topBar = factPanel.transform.Find("top").gameObject;
            topBar.AddComponent<CanvasGroup>();
            Text topTextShadow = topBar.transform.Find("Text (1)").gameObject.GetComponent<Text>();
            Text topText = topBar.transform.Find("Text (1)/Text (2)").gameObject.GetComponent<Text>();
            topTextShadow.text = topText.text = "Multiplayer";

            GameObject panelfg = __instance.factpanel.transform.Find("panelfg").gameObject;

            Text mainText = panelfg.transform.Find("FactText").GetComponent<Text>();
            mainText.text =
            "<size=36>Welcome to TootTally Multiplayer Test!</size>\n\n\n<color=\"green\">This is a beta state of the multiplayer mod and there may be a lot of glitches.\n\n" +
            "Please report any bugs found on our discord.</color>";
            mainText.gameObject.AddComponent<CanvasGroup>();

            GameObject.DestroyImmediate(panelfg.transform.Find("Button").gameObject);

            _mainMultiplayerPanelPrefab = GameObject.Instantiate(factPanel.gameObject);
            _mainMultiplayerPanelPrefab.SetActive(false);

            GameObject.DontDestroyOnLoad(_mainMultiplayerPanelPrefab);
        }

        public static void SetDefaultTextPrefab()
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas").gameObject;
            GameObject headerCreditText = mainCanvas.transform.Find("FullCreditsPanel/header-credits/Text").gameObject;

            GameObject textHolder = GameObject.Instantiate(headerCreditText);
            textHolder.name = "defaultTextPrefab";
            textHolder.AddComponent<Outline>();
            textHolder.SetActive(false);
            _defaultText = textHolder.GetComponent<Text>();
            _defaultText.fontSize = 22;
            _defaultText.alignment = TextAnchor.MiddleCenter;
            _defaultText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _defaultText.GetComponent<RectTransform>().sizeDelta = textHolder.GetComponent<RectTransform>().sizeDelta;
            _defaultText.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _defaultText.supportRichText = true;
            GameObject.DontDestroyOnLoad(_defaultText);
        }
        public static void SetNotificationPrefab()
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

            Text notifText = GameObject.Instantiate(_defaultText, _popUpNotifPrefab.transform);
            notifText.name = "NotifText";
            notifText.gameObject.GetComponent<RectTransform>().sizeDelta = popUpNorifRectTransform.sizeDelta;
            notifText.gameObject.SetActive(true);

            gameObjectHolder.SetActive(false);

            GameObject.DontDestroyOnLoad(_popUpNotifPrefab);
        }
        public static void SetCustomButtonPrefab()
        {
            GameObject settingBtn = _settingsGraphics.transform.Find("GRAPHICS/btn_opengraphicspanel").gameObject;

            GameObject gameObjectHolder = UnityEngine.Object.Instantiate(settingBtn);

            var tempBtn = gameObjectHolder.GetComponent<Button>();
            var oldBtnColors = tempBtn.colors;


            UnityEngine.Object.DestroyImmediate(tempBtn);

            var myBtn = gameObjectHolder.AddComponent<Button>();
            myBtn.colors = oldBtnColors;

            _buttonPrefab = gameObjectHolder.AddComponent<CustomButton>();
            _buttonPrefab.ConstructNewButton(gameObjectHolder.GetComponent<Button>(), gameObjectHolder.GetComponent<RectTransform>(), gameObjectHolder.GetComponentInChildren<Text>());

            gameObjectHolder.SetActive(false);

            UnityEngine.Object.DontDestroyOnLoad(gameObjectHolder);
        }

        public static void SetSteamLeaderboardPrefab()
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

        public static void SetPanelBodyInSteamLeaderboard()
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

            SetTabsInPanelBody();
            SetErrorsInPanelBody();
            SetScoreboardInPanelBody();
            SetSwirlyInPanelBody();
            AddSliderInPanelBody();
        }

        public static void SetTabsInPanelBody()
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

        public static void SetErrorsInPanelBody()
        {
            GameObject errorsHolder = _panelBodyPrefab.transform.Find("errors").gameObject;

            RectTransform errorsTransform = errorsHolder.GetComponent<RectTransform>();
            errorsTransform.anchoredPosition = new Vector2(-30, 15);
            errorsTransform.sizeDelta = new Vector2(-200, 0);

            errorsHolder.SetActive(false);

            //_errorText = _errorsHolder.transform.Find("error_noleaderboard").GetComponent<Text>();
            errorsHolder.transform.Find("error_noleaderboard").gameObject.SetActive(true);
        }

        public static void SetScoreboardInPanelBody()
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

        public static void SetSwirlyInPanelBody()
        {
            GameObject loadingSwirly = _panelBodyPrefab.transform.Find("loadingspinner_parent").gameObject; //Contains swirly, spin the container and not swirly.
            loadingSwirly.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, 5);
            loadingSwirly.SetActive(true);
        }

        public static void AddSliderInPanelBody()
        {
            CreateVerticalSliderFromPrefab(_panelBodyPrefab.transform, "LeaderboardVerticalSlider");
        }

        public static void SetSingleScorePrefab()
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

        public static void SetLeaderboardHeaderPrefab()
        {
            _leaderboardHeaderPrefab = GameObject.Instantiate(_singleScorePrefab.transform.Find("Num").GetComponent<Text>());
            _leaderboardHeaderPrefab.alignment = TextAnchor.MiddleCenter;
            _leaderboardHeaderPrefab.horizontalOverflow = HorizontalWrapMode.Overflow;
            _leaderboardHeaderPrefab.maskable = true;
            Outline outline = _leaderboardHeaderPrefab.gameObject.AddComponent<Outline>();

            GameObject.DontDestroyOnLoad(_leaderboardHeaderPrefab.gameObject);
        }

        public static void SetLeaderboardTextPrefab()
        {
            _leaderboardTextPrefab = GameObject.Instantiate(_singleScorePrefab.transform.Find("Name").GetComponent<Text>());
            _leaderboardTextPrefab.alignment = TextAnchor.MiddleCenter;
            _leaderboardTextPrefab.horizontalOverflow = HorizontalWrapMode.Overflow;
            _leaderboardTextPrefab.maskable = true;
            Outline outline = _leaderboardTextPrefab.gameObject.AddComponent<Outline>();

            DestroyNumNameScoreFromSingleScorePrefab();

            GameObject.DontDestroyOnLoad(_leaderboardTextPrefab.gameObject);
        }

        public static void SetSingleRowPrefab()
        {
            _singleRowPrefab = _singleScorePrefab.AddComponent<LeaderboardRowEntry>();
            Text rank = GameObject.Instantiate(_leaderboardHeaderPrefab, _singleScorePrefab.transform);
            rank.name = "rank";
            Text username = GameObject.Instantiate(_leaderboardTextPrefab, _singleScorePrefab.transform);
            username.name = "username";
            Text score = GameObject.Instantiate(_leaderboardTextPrefab, _singleScorePrefab.transform);
            score.name = "score";
            Text percent = GameObject.Instantiate(_leaderboardTextPrefab, _singleScorePrefab.transform);
            percent.name = "percent";
            Text grade = GameObject.Instantiate(_leaderboardTextPrefab, _singleScorePrefab.transform);
            grade.name = "grade";
            Text maxcombo = GameObject.Instantiate(_leaderboardTextPrefab, _singleScorePrefab.transform);
            maxcombo.name = "maxcombo";
            _singleRowPrefab.ConstructLeaderboardEntry(_singleScorePrefab, rank, username, score, percent, grade, maxcombo, false);
            _singleRowPrefab.singleScore.name = "singleRowPrefab";
        }

        public static void SetVerticalSliderPrefab()
        {
            Slider defaultSlider = GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "Slider").GetComponent<Slider>(); //yoink

            _verticalSliderPrefab = GameObject.Instantiate(defaultSlider);
            _verticalSliderPrefab.direction = Slider.Direction.TopToBottom;

            RectTransform sliderRect = _verticalSliderPrefab.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(25, 745);
            sliderRect.anchoredPosition = new Vector2(300, 0);

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
        public static void SetSliderPrefab()
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

        public static void DestroyNumNameScoreFromSingleScorePrefab()
        {
            DestroyFromParent(_singleScorePrefab, "Num");
            DestroyFromParent(_singleScorePrefab, "Name");
            DestroyFromParent(_singleScorePrefab, "Score");
        }

        #endregion

        #region Create Objects

        public static GameObject CreateLoginPanel(HomeController __instance)
        {
            GameObject playTesterPopup = __instance.ext_credits_go.transform.parent.gameObject;
            GameObject loginPanelPopup = GameObject.Instantiate(playTesterPopup);
            loginPanelPopup.name = "LoginPanel";
            loginPanelPopup.SetActive(true);

            GameObject fsLatencyPanel = loginPanelPopup.transform.Find("FSLatencyPanel").gameObject;
            fsLatencyPanel.SetActive(true);
            GameObject.DestroyImmediate(fsLatencyPanel.transform.Find("LatencyBG2").gameObject);
            fsLatencyPanel.transform.Find("LatencyBG").gameObject.GetComponent<Image>().color = GameTheme.themeColors.notification.border;

            GameObject latencyFGPanel = fsLatencyPanel.transform.Find("LatencyFG").gameObject; //this where most objects are located
            latencyFGPanel.GetComponent<Image>().color = GameTheme.themeColors.notification.background;

            Text title = latencyFGPanel.transform.Find("title").gameObject.GetComponent<Text>();
            Text subtitle = latencyFGPanel.transform.Find("subtitle").gameObject.GetComponent<Text>();
            title.text = "TootTally Login";
            title.color = GameTheme.themeColors.notification.defaultText;
            subtitle.text = "This is a pop up test to Login on TootTally.";
            subtitle.color = GameTheme.themeColors.notification.defaultText;


            GameObject loginPage = latencyFGPanel.transform.Find("page1").gameObject;
            GameObject.DestroyImmediate(loginPage.GetComponent<HorizontalLayoutGroup>());
            VerticalLayoutGroup vgroup = loginPage.AddComponent<VerticalLayoutGroup>();
            vgroup.childForceExpandHeight = vgroup.childScaleHeight = vgroup.childControlHeight = false;
            vgroup.childForceExpandWidth = vgroup.childScaleWidth = vgroup.childControlWidth = false;
            vgroup.padding.left = (int)(loginPage.GetComponent<RectTransform>().sizeDelta.x / 2) - 125;
            vgroup.spacing = 20;
            loginPage.name = "LoginPage";

            GameObject.DestroyImmediate(loginPage.transform.Find("col1").gameObject);
            GameObject.DestroyImmediate(loginPage.transform.Find("col3").gameObject);

            //username
            GameObject usernameTextHolder = loginPage.transform.Find("col2").gameObject;
            usernameTextHolder.name = "UsernameText";
            usernameTextHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 35);
            usernameTextHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(125, 0);
            Text usernameText = usernameTextHolder.GetComponent<Text>();
            usernameText.text = "Username:";
            usernameText.color = GameTheme.themeColors.notification.defaultText;

            GameObject usernameInputHolder = GameObject.Instantiate(usernameText.gameObject, loginPage.transform);
            GameObject usernameInputTextHolder = GameObject.Instantiate(usernameInputHolder, usernameInputHolder.transform);
            usernameInputTextHolder.name = "Text";
            GameObject.DestroyImmediate(usernameInputHolder.GetComponent<Text>());
            usernameInputHolder.name = "UsernameInput";
            InputField usernameInput = usernameInputHolder.AddComponent<InputField>();
            usernameInput.textComponent = usernameInputTextHolder.GetComponent<Text>();
            usernameInput.textComponent.alignment = TextAnchor.MiddleCenter;
            usernameInput.image = usernameInputHolder.AddComponent<Image>();
            usernameInput.image.color = GameTheme.themeColors.leaderboard.rowEntry;
            usernameInput.text = "Enter Username";

            //password
            GameObject passwordTextHolder = GameObject.Instantiate(usernameText.gameObject, loginPage.transform);
            passwordTextHolder.name = "PasswordText";
            Text passwordText = passwordTextHolder.GetComponent<Text>();
            passwordText.text = "Password:";

            GameObject passwordInputHolder = GameObject.Instantiate(usernameInputHolder, loginPage.transform);
            passwordInputHolder.name = "PasswordInput";
            InputField passwordInput = passwordInputHolder.GetComponent<InputField>();
            passwordInput.inputType = InputField.InputType.Password;
            passwordInput.textComponent = passwordInputHolder.transform.Find("Text").GetComponent<Text>();
            passwordInput.image = latencyFGPanel.GetComponent<Image>();
            passwordInput.text = "Password";

            //login button
            GameObject loginButtonHolder = latencyFGPanel.transform.Find("NEXT").gameObject;
            loginButtonHolder.name = "LoginButton";
            Button loginButton = loginButtonHolder.GetComponent<Button>();
            loginButton.onClick = new Button.ButtonClickedEvent();
            loginButton.onClick.AddListener(delegate
            {
                __instance.playSfx(4);// click button sfx
                PopUpNotifManager.DisplayNotif("Sending login info... Please wait.", GameTheme.themeColors.notification.defaultText);
                Plugin.Instance.StartCoroutine(TootTallyAPIService.GetLoginToken(usernameInput.text, passwordInput.text, (token) =>
                {
                    if (token.token == "")
                    {
                        PopUpNotifManager.DisplayNotif("Username or password wrong... Try login in again.", GameTheme.themeColors.notification.errorText);
                        return;
                    }

                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetUserFromToken(token.token, (user) =>
                    {
                        if (user == null)
                        {
                            PopUpNotifManager.DisplayNotif("Couldn't get user info... Please contact TootTally's moderator on discord.", GameTheme.themeColors.notification.errorText);
                            return;
                        }
                        PopUpNotifManager.DisplayNotif($"Login with {user.username} successful!", GameTheme.themeColors.notification.defaultText);
                        Plugin.userInfo = user;
                        Plugin.Instance.APIKey.Value = user.api_key;
                        AnimationManager.AddNewPositionAnimation(fsLatencyPanel, loginPanelPopup.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, -900), .8f, new EasingHelper.SecondOrderDynamics(0.75f, 1f, 0f));
                        AnimationManager.AddNewScaleAnimation(fsLatencyPanel, Vector2.zero, 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f), (sender) =>
                        {
                            GameObject.DestroyImmediate(sender);
                        });
                    }));
                }));
            });

            //close button
            GameObject closeButtonHolder = latencyFGPanel.transform.Find("CloseBtn").gameObject;
            Button closeButton = closeButtonHolder.GetComponent<Button>();
            closeButton.onClick = new Button.ButtonClickedEvent();
            closeButton.onClick.AddListener(delegate
            {
                __instance.playSfx(0);// click button sfx
                AnimationManager.AddNewPositionAnimation(fsLatencyPanel, loginPanelPopup.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, -900), .8f, new EasingHelper.SecondOrderDynamics(0.75f, 1f, 0f));
                AnimationManager.AddNewScaleAnimation(fsLatencyPanel, Vector2.zero, 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f), (sender) =>
                {
                    GameObject.DestroyImmediate(sender);
                });
            });

            //sign up button
            CustomButton signUpButton = CreateCustomButton(loginPage.transform, Vector2.zero, new Vector2(250, 50), "SignUp", "SignUpButton");
            signUpButton.button.onClick.AddListener(delegate
            {
                //confirm password
                GameObject confirmTextHolder = GameObject.Instantiate(usernameText.gameObject, loginPage.transform);
                confirmTextHolder.name = "ConfirmText";
                Text confirmText = confirmTextHolder.GetComponent<Text>();
                confirmText.text = "Confirm Password:";

                GameObject confirmInputHolder = GameObject.Instantiate(usernameInputHolder, loginPage.transform);
                confirmInputHolder.name = "ConfirmInput";
                InputField confirmInput = confirmInputHolder.GetComponent<InputField>();
                confirmInput.inputType = InputField.InputType.Password;
                confirmInput.textComponent = confirmInputHolder.transform.Find("Text").GetComponent<Text>();
                confirmInput.image = latencyFGPanel.GetComponent<Image>();
                confirmInput.text = "Password";
                GameObject.DestroyImmediate(signUpButton.gameObject);
                loginButtonHolder.transform.Find("Text").GetComponent<Text>().text = "SignUp";
                loginButton.onClick.RemoveAllListeners();
                loginButton.onClick.AddListener(delegate
                {
                    __instance.playSfx(4);// click button sfx
                    if (passwordInput.text != confirmInput.text)
                    {
                        passwordInput.text = "";
                        confirmInput.text = "";
                        PopUpNotifManager.DisplayNotif($"Passwords did not match! Type your password again.", GameTheme.themeColors.notification.errorText);
                        return; //skip requests
                    }
                    PopUpNotifManager.DisplayNotif($"Sending sign up request... Please wait.", GameTheme.themeColors.notification.defaultText);
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.SignUpRequest(usernameInput.text, passwordInput.text, confirmInput.text, (isValid) =>
                    {
                        if (isValid)
                        {
                            PopUpNotifManager.DisplayNotif($"Getting new user info...", GameTheme.themeColors.notification.defaultText);
                            Plugin.Instance.StartCoroutine(TootTallyAPIService.GetLoginToken(usernameInput.text, passwordInput.text, (token) =>
                            {
                                if (token.token != "")
                                {
                                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetUserFromToken(token.token, (user) =>
                                    {
                                        if (user != null)
                                        {
                                            PopUpNotifManager.DisplayNotif($"Login with {user.username} successful!", GameTheme.themeColors.notification.defaultText);
                                            Plugin.userInfo = user;
                                            Plugin.Instance.APIKey.Value = user.api_key;
                                            AnimationManager.AddNewPositionAnimation(fsLatencyPanel, loginPanelPopup.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, -900), .8f, new EasingHelper.SecondOrderDynamics(0.75f, 1f, 0f));
                                            AnimationManager.AddNewScaleAnimation(fsLatencyPanel, Vector2.zero, 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f), (sender) =>
                                            {
                                                GameObject.DestroyImmediate(sender);
                                            });
                                        }
                                    }));
                                }
                            }));
                        }
                    }));

                });
            });


            loginButtonHolder.transform.Find("Text").GetComponent<Text>().text = "Login";

            return loginPanelPopup;
        }

        public static GameObject CreateMultiplayerMainPanel(Transform canvasTransform, string name)
        {
            GameObject multiPanel = GameObject.Instantiate(_mainMultiplayerPanelPrefab, canvasTransform);
            multiPanel.name = name;
            multiPanel.SetActive(true);
            return multiPanel;
        }

        public static GameObject CreateEmptyMultiplayerPanel(Transform canvasTransform, string name, Vector2 size, Vector2 position)
        {
            GameObject panel = GameObject.Instantiate(_mainMultiplayerPanelPrefab, canvasTransform);
            GameObject panelbg = panel.transform.Find("Panelbg1").gameObject;
            GameObject panelfg = panel.transform.Find("panelfg").gameObject;
            panelbg.GetComponent<Image>().maskable = panelfg.GetComponent<Image>().maskable = true;

            RectTransform panelbgRectTransform = panelbg.GetComponent<RectTransform>();
            RectTransform panelfgRectTransform = panelfg.GetComponent<RectTransform>();
            panel.GetComponent<RectTransform>().anchoredPosition = position;
            panelbgRectTransform.anchoredPosition = Vector2.zero;
            panelbgRectTransform.sizeDelta = size;
            panelfgRectTransform.sizeDelta = size - new Vector2(4, 4);

            GameObject.DestroyImmediate(panel.transform.Find("top").gameObject);
            GameObject.DestroyImmediate(panelfg.transform.Find("FactText").gameObject);

            panel.name = name;
            panel.SetActive(true);
            return panel;
        }

        public static GameObject CreateLobbyInfoRow(Transform canvasTransform, string name, SerializableClass.MultiplayerLobbyInfo lobbyInfo, Action OnRowClick)
        {
            GameObject rowHolder = GameObject.Instantiate(_mainMultiplayerPanelPrefab.transform.Find("panelfg").gameObject, canvasTransform);
            rowHolder.name = name;
            GameObject.DestroyImmediate(rowHolder.transform.Find("FactText").gameObject);

            rowHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(730, 60);
            rowHolder.GetComponent<Image>().color = Color.gray;

            Text lobbyTitleText = CreateSingleText(rowHolder.transform, $"{name}TitleText", lobbyInfo.title, Color.white);
            lobbyTitleText.GetComponent<RectTransform>().anchoredPosition += new Vector2(5,-5);
            lobbyTitleText.alignment = TextAnchor.UpperLeft;
            lobbyTitleText.fontSize = 18;

            Text lobbyPlayerCount = CreateSingleText(rowHolder.transform, $"{name}PlayerCountText", $"{lobbyInfo.users.Count} / {lobbyInfo.maxPlayerCount} players", Color.white);
            lobbyPlayerCount.GetComponent<RectTransform>().anchoredPosition += new Vector2(-5,-5);
            lobbyPlayerCount.alignment = TextAnchor.UpperRight;
            lobbyPlayerCount.fontSize = 18;

            Text lobbyCurrentSongText = CreateSingleText(rowHolder.transform, $"{name}CurrentSongText", lobbyInfo.currentState, Color.white);
            lobbyCurrentSongText.GetComponent<RectTransform>().anchoredPosition += new Vector2(5,5);
            lobbyCurrentSongText.alignment = TextAnchor.LowerLeft;
            lobbyCurrentSongText.fontSize = 18;

            Text lobbyPingText = CreateSingleText(rowHolder.transform, $"{name}PingText", $"{lobbyInfo.ping} ms", Color.white);
            lobbyPingText.GetComponent<RectTransform>().anchoredPosition += new Vector2(-5,5);
            lobbyPingText.alignment = TextAnchor.LowerRight;
            lobbyPingText.fontSize = 18;

            Button btn = rowHolder.AddComponent<Button>();
            btn.onClick.AddListener(() => OnRowClick?.Invoke());

            return rowHolder;
        }

        public static CustomButton CreateCustomButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, string text, string name, Action onClick = null)
        {
            CustomButton newButton = UnityEngine.Object.Instantiate(_buttonPrefab, canvasTransform);
            newButton.name = name;
            newButton.gameObject.SetActive(true);
            ColorBlock btnColors = newButton.button.colors;
            btnColors.normalColor = GameTheme.themeColors.replayButton.colors.normalColor;
            btnColors.highlightedColor = GameTheme.themeColors.replayButton.colors.highlightedColor;
            btnColors.pressedColor = GameTheme.themeColors.replayButton.colors.pressedColor;
            btnColors.selectedColor = GameTheme.themeColors.replayButton.colors.normalColor;
            newButton.button.colors = btnColors;

            newButton.textHolder.text = text;
            newButton.textHolder.alignment = TextAnchor.MiddleCenter;
            newButton.textHolder.fontSize = 22;
            newButton.textHolder.horizontalOverflow = HorizontalWrapMode.Overflow;
            newButton.textHolder.verticalOverflow = VerticalWrapMode.Overflow;
            newButton.textHolder.color = GameTheme.themeColors.replayButton.text;


            newButton.GetComponent<RectTransform>().sizeDelta = size;
            newButton.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

            newButton.button.onClick.AddListener(() => onClick?.Invoke());

            return newButton;
        }

        public static void UpdatePrefabTheme()
        {
            if (_isHomeControllerInitialized)
            {
                _popUpNotifPrefab.GetComponent<Image>().color = GameTheme.themeColors.notification.border;
                _popUpNotifPrefab.transform.Find("Window Body").gameObject.GetComponent<Image>().color = GameTheme.themeColors.notification.background;
                _popUpNotifPrefab.transform.Find("NotifText").GetComponent<Text>().color = GameTheme.themeColors.notification.defaultText;
                _popUpNotifPrefab.transform.Find("NotifText").GetComponent<Outline>().effectColor = GameTheme.themeColors.notification.textOutline;
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

                Outline outline = _leaderboardTextPrefab.gameObject.GetComponent<Outline>();
                outline.effectColor = GameTheme.themeColors.leaderboard.textOutline;
                _leaderboardTextPrefab.color = GameTheme.themeColors.leaderboard.text;
                outline = _leaderboardHeaderPrefab.gameObject.GetComponent<Outline>();
                outline.effectColor = GameTheme.themeColors.leaderboard.textOutline;
                _leaderboardHeaderPrefab.color = GameTheme.themeColors.leaderboard.headerText;


                _sliderPrefab.transform.Find("Fill Area/Fill").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.fill;
                _verticalSliderPrefab.transform.Find("Handle").gameObject.GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.handle;
                _verticalSliderPrefab.transform.Find("Fill Area/Fill").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.fill;
                _verticalSliderPrefab.transform.Find("Background").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.background;
            }
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

        public static Text CreateSingleText(Transform canvasTransform, string name, string text, Color color)
        {
            Text singleText = GameObject.Instantiate(_defaultText, canvasTransform);
            singleText.name = name;

            singleText.text = text;
            singleText.color = color;
            singleText.gameObject.GetComponent<RectTransform>().sizeDelta = canvasTransform.GetComponent<RectTransform>().sizeDelta;

            singleText.gameObject.SetActive(true);

            return singleText;
        }

        public static Text CreateDoubleText(Transform canvasTransform, string name, string text, Color color)
        {
            Text doubledText = GameObject.Instantiate(_leaderboardTextPrefab, canvasTransform);
            doubledText.name = name;

            doubledText.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            doubledText.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, 15);
            doubledText.text = text;
            doubledText.color = color;

            return doubledText;
        }

        public static Text CreateTripleText(Transform canvasTransform, string name, string text, Color color)
        {
            Text tripledText = GameObject.Instantiate(_leaderboardTextPrefab, canvasTransform);
            tripledText.name = name;

            tripledText.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            tripledText.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, 15);
            tripledText.text = text;


            Text tripledTextSecondLayer = GameObject.Instantiate(_leaderboardTextPrefab, canvasTransform);
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
            rowEntry.username.text = scoreData.player.Length > 20 ? scoreData.player.Substring(0, 20) : scoreData.player;
            rowEntry.score.text = string.Format("{0:n0}", scoreData.score);
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
                rowEntry.maxcombo.text = (int)scoreData.tt + "tt";
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
            if (replayId != "NA") //if there's a uuid, add a replay button
            {
                CreateCustomButton(rowEntry.singleScore.transform, Vector2.zero, new Vector2(26, 26), "►", "ReplayButton",
                delegate
                {
                    ReplaySystemManager.ResolveLoadReplay(replayId, levelSelectControllerInstance);
                });
            }
            return rowEntry;
        }

        public static PopUpNotif CreateNotif(Transform canvasTransform, string name, string text, Color textColor)
        {
            PopUpNotif notif = GameObject.Instantiate(_popUpNotifPrefab, canvasTransform);

            notif.name = name;
            notif.SetTextColor(textColor);
            notif.SetText(text);
            notif.gameObject.SetActive(true);

            return notif;
        }

        #endregion

        public static void DestroyFromParent(GameObject parent, string objectName) => GameObject.DestroyImmediate(parent.transform.Find(objectName).gameObject);
    }
}
