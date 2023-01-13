using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TootTally.CustomLeaderboard;
using TootTally.Replays;
using TootTally.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public static class GameObjectFactory
    {
        private static CustomButton _buttonPrefab;
        private static Text _defaultText, _leaderboardHeaderPrefab, _leaderboardTextPrefab;
        private static Slider _verticalSliderPrefab;
        private static GameObject _starPrefab;
        private static PopUpNotif _popUpNotifPrefab;

        private static GameObject _settingsGraphics, _steamLeaderboardPrefab, _singleScorePrefab, _panelBodyPrefab;
        private static LeaderboardRowEntry _singleRowPrefab;
        private static bool _isHomeControllerInitialized = false;
        private static bool _isLevelSelectControllerInitialized = false;



        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        static void YoinkSettingsGraphicsHomeController(HomeController __instance)
        {
            _settingsGraphics = __instance.fullsettingspanel.transform.Find("Settings").gameObject;
            OnHomeControllerInitialize();
        }
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void YoinkSettingsGraphicsLevelSelectController()
        {
            OnLevelSelectControllerInitialize();
        }

        public static void OnHomeControllerInitialize()
        {

            if (_isHomeControllerInitialized) return;

            SetDefaultTextPrefab();
            SetNotificationPrefab();
            SetCustomButtonPrefab();
            SetStarPrefab();
        }

        public static void OnLevelSelectControllerInitialize()
        {
            if (_isLevelSelectControllerInitialized) return;

            SetVerticalSliderPrefab();
            SetSteamLeaderboardPrefab();
            SetSingleScorePrefab();
            SetLeaderboardHeaderPrefab();
            SetLeaderboardTextPrefab();
            SetSingleRowPrefab();

        }

        #region SetPrefabs
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
            _popUpNotifPrefab.GetComponent<Image>().color = new Color(1, .3f, .5f, .75f);
            _popUpNotifPrefab.transform.Find("Window Body").gameObject.GetComponent<Image>().color = new Color(0, 0, 0, .95f);

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

        public static void SetStarPrefab()
        {
            GameObject star = _settingsGraphics.transform.Find("ALLEGIANCE/star (back)").gameObject;

            _starPrefab = UnityEngine.Object.Instantiate(star);

            _starPrefab.SetActive(false);

            UnityEngine.Object.DontDestroyOnLoad(_starPrefab);
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

                currentTab.AddComponent<Image>();
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
            scoreboardRectTransform.anchoredPosition = new Vector2(-29, -10);
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
            CreateSliderFromPrefab(_panelBodyPrefab.transform, "LeaderboardVerticalSlider");
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
            _leaderboardHeaderPrefab.gameObject.AddComponent<Outline>();

            GameObject.DontDestroyOnLoad(_leaderboardHeaderPrefab.gameObject);
        }

        public static void SetLeaderboardTextPrefab()
        {
            _leaderboardTextPrefab = GameObject.Instantiate(_singleScorePrefab.transform.Find("Name").GetComponent<Text>());
            _leaderboardTextPrefab.alignment = TextAnchor.MiddleCenter;
            _leaderboardTextPrefab.horizontalOverflow = HorizontalWrapMode.Overflow;
            _leaderboardTextPrefab.maskable = true;
            _leaderboardTextPrefab.gameObject.AddComponent<Outline>();

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
            _verticalSliderPrefab.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Color.white;

            RectTransform sliderRect = _verticalSliderPrefab.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(25, 745);
            sliderRect.anchoredPosition = new Vector2(300, 0);

            RectTransform handleSlideAreaRect = _verticalSliderPrefab.transform.Find("Handle Slide Area").GetComponent<RectTransform>();
            RectTransform handleRect = handleSlideAreaRect.gameObject.transform.Find("Handle").GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(40, 40);
            handleRect.pivot = Vector2.zero;
            handleRect.anchorMax = Vector2.zero;
            handleRect.gameObject.GetComponent<Image>().color = Color.white;
            GameObject handle = GameObject.Instantiate(handleRect.gameObject, _verticalSliderPrefab.transform);
            handle.name = "Handle";
            RectTransform backgroundSliderRect = _verticalSliderPrefab.transform.Find("Background").GetComponent<RectTransform>();
            backgroundSliderRect.anchoredPosition = new Vector2(-5, backgroundSliderRect.anchoredPosition.y);
            backgroundSliderRect.sizeDelta = new Vector2(-10, backgroundSliderRect.sizeDelta.y);

            _verticalSliderPrefab.value = 0f;
            _verticalSliderPrefab.minValue = -0.05f;
            _verticalSliderPrefab.maxValue = 1.04f;
            _verticalSliderPrefab.gameObject.SetActive(false);

            DestroyFromParent(_verticalSliderPrefab.gameObject, "Handle Slide Area/Handle");

            GameObject.DontDestroyOnLoad(_verticalSliderPrefab);
        }

        public static void DestroyNumNameScoreFromSingleScorePrefab()
        {
            DestroyFromParent(_singleScorePrefab, "Num");
            DestroyFromParent(_singleScorePrefab, "Name");
            DestroyFromParent(_singleScorePrefab, "Score");
        }

        #endregion

        #region Create Objects
        public static CustomButton CreateCustomButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, string text, string name, Action onClick = null)
        {
            CustomButton newButton = UnityEngine.Object.Instantiate(_buttonPrefab, canvasTransform);
            newButton.name = name;
            newButton.gameObject.SetActive(true);

            newButton.textHolder.text = text;
            newButton.textHolder.alignment = TextAnchor.MiddleCenter;
            newButton.textHolder.fontSize = 22;
            newButton.textHolder.horizontalOverflow = HorizontalWrapMode.Overflow;
            newButton.textHolder.verticalOverflow = VerticalWrapMode.Overflow;
            newButton.textHolder.color = Color.black;


            newButton.GetComponent<RectTransform>().sizeDelta = size;
            newButton.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

            newButton.button.onClick.AddListener(() => onClick?.Invoke());

            return newButton;
        }

        //Keeping this just in case I wanna use it somewhere
        public static GameObject CreateLoadingScreenStar(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, float eulerRotationY, string name)
        {
            GameObject myStar = UnityEngine.Object.Instantiate(_starPrefab, canvasTransform);
            myStar.name = name;
            myStar.gameObject.SetActive(true);

            RectTransform rectBack = myStar.GetComponent<RectTransform>();
            rectBack.anchoredPosition = anchoredPosition;
            rectBack.eulerAngles = new Vector3(0, eulerRotationY, 0);
            rectBack.sizeDelta = size;

            RectTransform rectFront = myStar.transform.Find("star (front)").GetComponent<RectTransform>();
            rectFront.anchoredPosition = new Vector2(-1, 1);
            rectFront.sizeDelta = size;

            return myStar;
        }

        public static GameObject CreateSteamLeaderboardFromPrefab(Transform canvasTransform, string name)
        {
            GameObject steamLeaderboard = GameObject.Instantiate(_steamLeaderboardPrefab, canvasTransform);
            steamLeaderboard.name = name;
            return steamLeaderboard;
        }

        public static Slider CreateSliderFromPrefab(Transform canvasTransform, string name)
        {
            Slider slider = GameObject.Instantiate(_verticalSliderPrefab, canvasTransform);
            slider.name = name;
            return slider;
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
            rowEntry.username.text = scoreData.player;
            rowEntry.score.text = string.Format("{0:n0}", scoreData.score);
            rowEntry.rank.text = "#" + count;
            rowEntry.percent.text = scoreData.percentage.ToString("0.00") + "%";
            rowEntry.grade.text = scoreData.grade;
            if (scoreData.grade == "SS")
            {
                rowEntry.grade.text = "S";
                GameObjectFactory.CreateDoubleText(rowEntry.grade.transform, "DoubleS" + scoreData.player + "Text", "S", Color.yellow); ;

            }
            else if (scoreData.grade == "SSS")
            {
                rowEntry.grade.text = "S";
                GameObjectFactory.CreateTripleText(rowEntry.grade.transform, "TripleS" + scoreData.player + "Text", "S", Color.yellow);
            }
            else
                rowEntry.grade.color = gradeColor;
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
                    ResolveLoadReplay(replayId, levelSelectControllerInstance);
                });
            }
            return rowEntry;
        }

        private static ReplaySystem.ReplayState ResolveLoadReplay(string replayId, LevelSelectController levelSelectControllerInstance)
        {
            ReplaySystem.ReplayState replayState = ReplaySystem.LoadReplay(replayId);
            switch (replayState)
            {
                case ReplaySystem.ReplayState.ReplayLoadSuccess:
                    levelSelectControllerInstance.playbtn.onClick?.Invoke();
                    break;

                case ReplaySystem.ReplayState.ReplayLoadNotFound:
                    PopUpNotifManager.DisplayNotif("Downloading replay...");
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadReplay(replayId, (uuid) =>
                    {
                        ResolveLoadReplay(uuid, levelSelectControllerInstance);
                    }));
                    break;

                case ReplaySystem.ReplayState.ReplayLoadErrorIncompatible:
                    break;
                case ReplaySystem.ReplayState.ReplayLoadError:
                    break;

            }
            return replayState;
        }

        public static PopUpNotif CreateNotif(Transform canvasTransform, string name, string text)
        {
            PopUpNotif notif = GameObject.Instantiate(_popUpNotifPrefab, canvasTransform);

            notif.name = name;
            notif.SetText(text);
            notif.gameObject.SetActive(true);

            return notif;
        }

        #endregion

        public static void DestroyFromParent(GameObject parent, string objectName) => GameObject.DestroyImmediate(parent.transform.Find(objectName).gameObject);
    }
}
