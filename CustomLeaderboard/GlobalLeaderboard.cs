﻿using System;
using System.Collections.Generic;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using TMPro;
using TootTally.Graphics;
using TootTally.Utils;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using TrombLoader.CustomTracks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace TootTally.CustomLeaderboard
{
    public class GlobalLeaderboard
    {
        #region constants
        private const string ERROR_NO_LEADERBOARD_FOUND_TEXT = "Could not find a leaderboard for this track.\n <size=15>Be the first one to set a score on the track!</size>"; //lol
        private const string ERROR_NO_SONGHASH_FOUND_TEXT = "This chart is not uploaded to TootTally...\n <size=15>Please upload the chart to TootTally.com\n or use TootBender on discord to create the leaderboard.</size>";
        private static Dictionary<string, Color> gradeToColorDict = new Dictionary<string, Color> { { "SSS", Color.yellow }, { "SS", Color.yellow }, { "S", Color.yellow }, { "A", Color.green }, { "B", new Color(0, .4f, 1f) }, { "C", Color.magenta }, { "D", Color.red }, { "F", Color.grey }, };
        private static string[] tabsImageNames = { "profile64.png", "global64.png", "local64.png" };
        private static float[] _starSizeDeltaPositions = { 0, 20, 58, 96, 134, 172, 210, 248, 285, 324, 361 };
        #endregion

        private List<IEnumerator<UnityWebRequestAsyncOperation>> _currentLeaderboardCoroutines;

        private LevelSelectController _levelSelectControllerInstance;

        private List<SerializableClass.ScoreDataFromDB> _scoreDataList;

        private GameObject _leaderboard, _globalLeaderboard, _scoreboard, _errorsHolder, _tabs, _profilePopup;
        private GameObject _ratedIcon;
        private LoadingIcon _loadingSwirly, _profilePopupLoadingSwirly;
        private Text _errorText;
        private TMP_Text _diffRating;
        private Vector2 _starRatingMaskSizeTarget;
        private RectTransform _diffRatingMaskRectangle;
        private List<LeaderboardRowEntry> _scoreGameObjectList;
        private SerializableClass.SongDataFromDB _songData;
        //private Chart _localSongData;
        private Slider _slider, _gameSpeedSlider;
        private ScrollableSliderHandler _scrollableSliderHandler;
        private GameObject _sliderHandle;

        private Dictionary<int, float> _speedToDiffDict;

        private int _currentSelectedSongHash, _localScoreId;
        public bool HasLeaderboard => _leaderboard != null;
        public bool IsMouseOver;

        private EasingHelper.SecondOrderDynamics _starMaskAnimation;

        public void Initialize(LevelSelectController __instance)
        {
            _levelSelectControllerInstance = __instance;
            _currentLeaderboardCoroutines = new List<IEnumerator<UnityWebRequestAsyncOperation>>();
            _scoreGameObjectList = new List<LeaderboardRowEntry>();
            ClearBaseLeaderboard();
            CustomizeGameMenuUI();

            _leaderboard.transform.SetAsLastSibling();
            _globalLeaderboard = GameObjectFactory.CreateSteamLeaderboardFromPrefab(_leaderboard.transform, "GlobalLeaderboard");
            _globalLeaderboard.SetActive(true);

            EventTrigger leaderboardHitboxEvent = _globalLeaderboard.AddComponent<EventTrigger>();
            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) => OnPointerEnter());
            leaderboardHitboxEvent.triggers.Add(pointerEnterEvent);

            EventTrigger.Entry pointerExitEvent = new EventTrigger.Entry();
            pointerExitEvent.eventID = EventTriggerType.PointerExit;
            pointerExitEvent.callback.AddListener((data) => OnPointerExit());
            leaderboardHitboxEvent.triggers.Add(pointerExitEvent);


            GameObject panelBody = _globalLeaderboard.transform.Find("PanelBody").gameObject;
            panelBody.SetActive(true);
            _scoreboard = panelBody.transform.Find("scoreboard").gameObject;
            _scoreboard.SetActive(true);

            _errorsHolder = panelBody.transform.Find("errors").gameObject;

            _errorText = _errorsHolder.transform.Find("error_noleaderboard").GetComponent<Text>();
            _errorText.gameObject.SetActive(true);

            _tabs = panelBody.transform.Find("tabs").gameObject; //Hidden until icons are loaded
            SetTabsImages();

            _loadingSwirly = GameObjectFactory.CreateLoadingIcon(panelBody.transform, new Vector2(-20, 0), new Vector2(128, 128), AssetManager.GetSprite("icon.png"), true, "LeaderboardLoadingSwirly");
            _loadingSwirly.StartRecursiveAnimation();
            _loadingSwirly.Show();

            _slider = panelBody.transform.Find("LeaderboardVerticalSlider").gameObject.GetComponent<Slider>();
            _slider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.fill;
            _slider.transform.Find("Background").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.background;

            _scrollableSliderHandler = _slider.gameObject.AddComponent<ScrollableSliderHandler>();

            _sliderHandle = _slider.transform.Find("Handle").gameObject;
            _sliderHandle.GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.handle;

            SetOnSliderValueChangeEvent();

            GameObject diffBar = GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "diff bar");
            GameObject.DestroyImmediate(GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "difficulty text").gameObject);
            var t = GameObjectFactory.CreateSingleText(diffBar.transform, "Difficulty Text", "Difficulty:", Color.white, GameObjectFactory.TextFont.Multicolore);
            t.alignment = TextAlignmentOptions.Left;
            t.margin = new Vector2(80, 4);
            t.fontSize = 16;

            GameObject diffStarsHolder = GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "difficulty stars");
            _diffRatingMaskRectangle = diffStarsHolder.GetComponent<RectTransform>();
            _diffRatingMaskRectangle.anchoredPosition = new Vector2(-284, -48);
            _diffRatingMaskRectangle.sizeDelta = new Vector2(0, 30);
            var mask = diffStarsHolder.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            diffStarsHolder.AddComponent<Image>();
            //imageMask.color = new Color(0, 0, 0, 0.01f); //if set at 0 stars wont display ?__?
            diffBar.GetComponent<RectTransform>().sizeDelta += new Vector2(41.5f, 0);
            _diffRating = GameObjectFactory.CreateSingleText(diffBar.transform, "diffRating", "", GameTheme.themeColors.leaderboard.text, GameObjectFactory.TextFont.Multicolore);
            _diffRating.outlineColor = GameTheme.themeColors.leaderboard.textOutline;
            _diffRating.outlineWidth = 0.2f;
            _diffRating.fontSize = 20;
            _diffRating.alignment = TextAlignmentOptions.MidlineRight;
            _diffRating.rectTransform.sizeDelta = new Vector2(450, 30);
            _diffRating.rectTransform.anchorMin = _diffRating.rectTransform.anchorMax = new Vector2(0, .5f);
            _diffRating.rectTransform.offsetMin = Vector2.zero;

            _starMaskAnimation = new EasingHelper.SecondOrderDynamics(1.23f, 1f, 1.2f);

            _ratedIcon = GameObjectFactory.CreateImageHolder(_globalLeaderboard.transform, new Vector2(350, 180), Vector2.one * 42f, AssetManager.GetSprite("rated64.png"), "RatedChartIcon");
            var bubble = _ratedIcon.AddComponent<BubblePopupHandler>();
            bubble.Initialize(GameObjectFactory.CreateBubble(new Vector2(300, 40), "RatedIconBubble", "This chart is rated.", 6, true, 12));

            _levelSelectControllerInstance.sortdrop.transform.SetAsLastSibling();
        }

        public void OnPointerEnter()
        {
            _scrollableSliderHandler.enabled = IsMouseOver = true;
        }

        public void OnPointerExit()
        {
            _scrollableSliderHandler.enabled = IsMouseOver = false;
        }

        public void ClearBaseLeaderboard()
        {
            _leaderboard = GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "Leaderboard").gameObject;

            //clear original Leaderboard from its objects
            foreach (Transform gameObjectTransform in _leaderboard.transform)
                gameObjectTransform.gameObject.SetActive(false);

            DestroyFromParent(_leaderboard, ".......");
            DestroyFromParent(_leaderboard, "\"HIGH SCORES\"");
            for (int i = 1; i <= 5; i++)
                _leaderboard.transform.Find(i.ToString()).gameObject.SetActive(false);
        }

        public void CustomizeGameMenuUI()
        {
            try
            {
                //fuck that useless Dial
                GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "Dial").gameObject.SetActive(false);

                //move capsules to the left
                GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "capsules").GetComponent<RectTransform>().anchoredPosition = new Vector2(-275, 32);

                //move btn_random next to capsules
                GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "btn_RANDOM").GetComponent<RectTransform>().anchoredPosition = new Vector2(-123, -7);

                //move btn_turbo somewhere
                GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "btn_TURBO").GetComponent<RectTransform>().anchoredPosition = new Vector2(-110, 65);

                //Patch current slider and move it slightly above RANDOM_btn
                BetterScrollSpeedSliderPatcher.PatchScrollSpeedSlider();
                GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "Slider").GetComponent<RectTransform>().anchoredPosition = new Vector2(-115, 23);
                GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "ScrollSpeedShad").GetComponent<RectTransform>().anchoredPosition = new Vector2(-112, 36);

                //Remove btn_TURBO + btn_PRACTICE and add GameSpeed slider
                GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "btn_TURBO").SetActive(false);
                GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "btn_PRACTICE").SetActive(false);
                _gameSpeedSlider = GameObject.Instantiate(GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "Slider").GetComponent<Slider>(), GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH).transform);
                _gameSpeedSlider.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-110, 65);
                _gameSpeedSlider.wholeNumbers = true;
                _gameSpeedSlider.minValue = 0;
                _gameSpeedSlider.maxValue = 30;
                _gameSpeedSlider.value = (Replays.ReplaySystemManager.gameSpeedMultiplier - .5f) / .05f;

                GameObject gameSpeedText = GameObject.Instantiate(GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "ScrollSpeedShad"), GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH).transform);
                gameSpeedText.name = "GameSpeedShad";
                gameSpeedText.GetComponent<Text>().text = "Game Speed";
                gameSpeedText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-108, 78);
                GameObject gameSpeedTextFG = gameSpeedText.transform.Find("ScrollSpeed").gameObject;
                gameSpeedTextFG.name = "GameSpeed";
                gameSpeedTextFG.GetComponent<Text>().text = "Game Speed";

                Text scrollSpeedSliderText = _gameSpeedSlider.transform.Find("Handle Slide Area/Handle/ScrollSpeed-lbl(Clone)").GetComponent<Text>(); //💀
                scrollSpeedSliderText.text = (_gameSpeedSlider.value * .05f + .5f).ToString("0.00");
                _gameSpeedSlider.onValueChanged = new Slider.SliderEvent();
                _gameSpeedSlider.onValueChanged.AddListener((float _value) =>
                {
                    _gameSpeedSlider.value = Mathf.Round(_value * 20) / 20f;
                    Replays.ReplaySystemManager.gameSpeedMultiplier = _gameSpeedSlider.value * .05f + .5f;
                    scrollSpeedSliderText.text = Replays.ReplaySystemManager.gameSpeedMultiplier.ToString("0.00");
                    UpdateStarRating();
                });

                GameObject titlebarPrefab = GameObject.Instantiate(_levelSelectControllerInstance.songtitlebar);
                titlebarPrefab.name = "titlebarPrefab";
                titlebarPrefab.GetComponent<RectTransform>().eulerAngles = Vector3.zero;
                titlebarPrefab.GetComponent<RectTransform>().localScale = Vector3.one;
                titlebarPrefab.GetComponent<Image>().color = new Color(0, 0, 0, 0.001f);

                GameObject fullScreenPanelCanvas = GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH);

                GameObject ttHitbox = GameObjectFactory.CreateDefaultPanel(fullScreenPanelCanvas.transform, new Vector2(381, -207), new Vector2(72, 72), "ProfilePopupHitbox");
                GameObjectFactory.CreateSingleText(ttHitbox.transform, "ProfilePopupHitboxText", "P", GameTheme.themeColors.leaderboard.text, GameObjectFactory.TextFont.Multicolore);

                _profilePopup = GameObjectFactory.CreateDefaultPanel(fullScreenPanelCanvas.transform, new Vector2(525, -300), new Vector2(450, 270), "TootTallyScorePanel");
                _profilePopupLoadingSwirly = GameObjectFactory.CreateLoadingIcon(_profilePopup.transform, Vector2.zero, new Vector2(96, 96), AssetManager.GetSprite("icon.png"), true, "ProfilePopupLoadingSwirly");
                _profilePopupLoadingSwirly.Show();
                _profilePopupLoadingSwirly.StartRecursiveAnimation();

                var scoresbody = _profilePopup.transform.Find("scoresbody").gameObject;

                HorizontalLayoutGroup horizontalLayoutGroup = scoresbody.AddComponent<HorizontalLayoutGroup>();
                horizontalLayoutGroup.padding = new RectOffset(2, 2, 2, 2);
                horizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
                horizontalLayoutGroup.childForceExpandHeight = horizontalLayoutGroup.childForceExpandWidth = true;

                GameObject mainPanel = GameObject.Instantiate(titlebarPrefab, scoresbody.transform);
                VerticalLayoutGroup verticalLayoutGroup = mainPanel.AddComponent<VerticalLayoutGroup>();
                verticalLayoutGroup.padding = new RectOffset(2, 2, 2, 2);
                verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
                verticalLayoutGroup.childForceExpandHeight = verticalLayoutGroup.childForceExpandWidth = true;

                Plugin.Instance.StartCoroutine(TootTallyAPIService.GetUserFromID(Plugin.userInfo.id, user =>
                {
                    AssetManager.GetProfilePictureByID(user.id, sprite =>
                    {
                        var i = GameObjectFactory.CreateCustomButton(scoresbody.transform, Vector2.zero, new Vector2(sprite.rect.width, sprite.rect.height), sprite, false, "Pfp", OpenUserProfile);
                        i.transform.SetSiblingIndex(0);
                    });
                    if (Plugin.userInfo.tt == 0)
                        Plugin.userInfo.tt = user.tt;

                    var t = GameObjectFactory.CreateSingleText(mainPanel.transform, "NameLabel", $"{user.username} #{user.rank}", GameTheme.themeColors.leaderboard.text);
                    var t2 = GameObjectFactory.CreateSingleText(mainPanel.transform, "TTLabel", $"{user.tt}tt (<color=\"green\">{(user.tt - Plugin.userInfo.tt > 0?"+":"")}{user.tt - Plugin.userInfo.tt:0.00}tt</color>)", GameTheme.themeColors.leaderboard.text);
                    _profilePopupLoadingSwirly.Dispose();
                }));

                new SlideTooltip(ttHitbox, _profilePopup, new Vector2(525, -300), new Vector2(282, -155));
            }
            catch (Exception e)
            {
                TootTallyLogger.LogError(e.Message);
            }
        }

        private void UpdateStarRating()
        {

            if (_songData != null && _speedToDiffDict != null)
            {
                float diff = _songData.is_rated ? _speedToDiffDict[(int)_gameSpeedSlider.value] : _speedToDiffDict[1];
                _diffRating.text = diff.ToString("0.0");

                int roundedUpStar = (int)Mathf.Clamp(diff + 1, 1, 10);
                int roundedDownStar = (int)Mathf.Clamp(diff, 0, 9);
                _starMaskAnimation.SetStartVector(_diffRatingMaskRectangle.sizeDelta);
                _starRatingMaskSizeTarget = new Vector2(EasingHelper.Lerp(_starSizeDeltaPositions[roundedUpStar], _starSizeDeltaPositions[roundedDownStar], roundedUpStar - diff), 30);

            }
            else
            {
                _diffRating.text = "NA";
                _starMaskAnimation.SetStartVector(_diffRatingMaskRectangle.sizeDelta);
                _starRatingMaskSizeTarget = new Vector2(_starSizeDeltaPositions[0], 30);
            }
        }

        public void UpdateLeaderboard(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist, Action<LeaderboardState> callback)
        {
            _globalLeaderboard.SetActive(true); //for some reasons its needed to display the leaderboard
            _ratedIcon.SetActive(false);
            _scrollableSliderHandler.ResetAcceleration();

            var trackRef = ___alltrackslist[_levelSelectControllerInstance.songindex].trackref;
            var track = TrackLookup.lookup(trackRef);
            var songHash = SongDataHelper.GetSongHash(track);


            if (_currentLeaderboardCoroutines.Count != 0) CancelAndClearAllCoroutineInList();

            //TootTallyAPIService.GetLocalChartRatings(SongDataHelper.GetSongFilePath(track), chart => _localSongData = chart);

            _currentLeaderboardCoroutines.Add(TootTallyAPIService.GetHashInDB(songHash, track is CustomTrack, songHashInDB =>
            {
                if (songHashInDB == 0)
                {
                    _errorText.text = ERROR_NO_SONGHASH_FOUND_TEXT;
                    callback(LeaderboardState.ErrorNoSongHashFound);
                    _diffRating.text = "NA";
                    _starMaskAnimation.SetStartVector(_diffRatingMaskRectangle.sizeDelta);
                    _starRatingMaskSizeTarget = new Vector2(_starSizeDeltaPositions[0], 30);
                    return; // Skip if no song found
                }
                else
                    _currentSelectedSongHash = songHashInDB;
                //_localSongData = null;
                _songData = null;
                _scoreDataList = null;
                _speedToDiffDict = null;
                _currentLeaderboardCoroutines.Add(TootTallyAPIService.GetSongDataFromDB(songHashInDB, songData =>
                {
                    if (songData != null)
                    {
                        _songData = songData;
                        _speedToDiffDict = new Dictionary<int, float>();
                        if (songData.is_rated)
                        {
                            _ratedIcon.SetActive(true);
                            for (int i = 0; i <= 29; i++)
                            {
                                float diffIndex = (int)(i / 5f);
                                float diffMin = diffIndex * .25f + .5f;
                                float diffMax = (diffIndex + 1f) * .25f + .5f;
                                float currentGameSpeed = i * .05f + .5f;

                                float by = (currentGameSpeed - diffMin) / (diffMax - diffMin);

                                float diff = EasingHelper.Lerp(_songData.speed_diffs[(int)diffIndex], _songData.speed_diffs[(int)diffIndex + 1], by);

                                _speedToDiffDict.Add(i, diff);
                            }
                            _speedToDiffDict.Add(30, _songData.speed_diffs.Last());
                        }
                        else
                            _speedToDiffDict.Add(1, _songData.difficulty);
                    }
                    else
                        _speedToDiffDict = null;
                    UpdateStarRating();

                    if (_scoreDataList != null)
                        CancelAndClearAllCoroutineInList();
                }));
                Plugin.Instance.StartCoroutine(_currentLeaderboardCoroutines.Last());
                _currentLeaderboardCoroutines.Add(TootTallyAPIService.GetLeaderboardScoresFromDB(songHashInDB, scoreDataList =>
                {
                    if (scoreDataList != null)
                    {
                        _scoreDataList = scoreDataList;
                        _scrollableSliderHandler.accelerationMult = 8f / _scoreDataList.Count;
                        callback(LeaderboardState.ReadyToRefresh);
                    }
                    else
                    {
                        _errorText.text = ERROR_NO_LEADERBOARD_FOUND_TEXT;
                        callback(LeaderboardState.ErrorNoLeaderboardFound);
                    }

                    if (_songData != null)
                        CancelAndClearAllCoroutineInList();
                }));
                Plugin.Instance.StartCoroutine(_currentLeaderboardCoroutines.Last());
            }));
            Plugin.Instance.StartCoroutine(_currentLeaderboardCoroutines.Last());
        }

        public void ShowLoadingSwirly() => _loadingSwirly.Show();
        public void HideLoadingSwirly() => _loadingSwirly.Hide();

        public void RefreshLeaderboard()
        {
            var count = 1;
            _localScoreId = -1;
            foreach (SerializableClass.ScoreDataFromDB scoreData in _scoreDataList)
            {
                LeaderboardRowEntry rowEntry = GameObjectFactory.CreateLeaderboardRowEntryFromScore(_scoreboard.transform, $"RowEntry{scoreData.player}", scoreData, count, gradeToColorDict[scoreData.grade], _levelSelectControllerInstance);
                _scoreGameObjectList.Add(rowEntry);
                if (scoreData.player == Plugin.userInfo.username)
                {
                    rowEntry.imageStrip.color = GameTheme.themeColors.leaderboard.yourRowEntry;
                    rowEntry.imageStrip.gameObject.SetActive(true);
                    _localScoreId = count - 1;
                }
                count++;
            }
            if (_scoreGameObjectList.Count > 8)
            {
                _slider.value = 0f;
                _sliderHandle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-12, 522);
                ShowSlider();
            }
            else
                HideSlider();
        }

        public void SetOnSliderValueChangeEvent()
        {
            _slider.onValueChanged.AddListener((float _value) =>
            {
                _slider.value = Mathf.Clamp(_value, 0, 1);

                foreach (LeaderboardRowEntry row in _scoreGameObjectList)
                {
                    _sliderHandle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-_slider.GetComponent<RectTransform>().sizeDelta.x / 2, (_slider.GetComponent<RectTransform>().sizeDelta.y / 1.38f) - _slider.fillRect.rect.height); //Dont even ask why divided by 1.38... I dont understand either
                    RectTransform rect = row.singleScore.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, ((row.rowId - 1) * -35) + (_slider.value * 35 * (_scoreGameObjectList.Count - 8)) - 17);
                    if (rect.anchoredPosition.y >= -15)
                        row.GetComponent<CanvasGroup>().alpha = Math.Max(1 - ((rect.anchoredPosition.y + 15) / 35), 0);
                    else if (rect.anchoredPosition.y - 35 <= 35 * 8 + 15)
                        row.GetComponent<CanvasGroup>().alpha = Math.Max((rect.anchoredPosition.y + (35 * 8) + 15) / 35, 0);
                    else
                        row.GetComponent<CanvasGroup>().alpha = 1;
                }
            });
        }

        public void UpdateStarRatingAnimation()
        {
            _diffRatingMaskRectangle.sizeDelta = _starMaskAnimation.GetNewVector(_starRatingMaskSizeTarget, Time.deltaTime);
        }

        public bool IsScrollingEnabled() => _scrollableSliderHandler.enabled;

        public void ClearLeaderboard()
        {
            _scoreGameObjectList.ForEach(score => GameObject.DestroyImmediate(score.singleScore));
            _scoreGameObjectList.Clear();
        }

        public void CancelAndClearAllCoroutineInList()
        {
            _currentLeaderboardCoroutines.ForEach(routine => Plugin.Instance.StopCoroutine(routine));
            _currentLeaderboardCoroutines.Clear();
        }

        public void ShowSlider() => _slider.gameObject.SetActive(true); public void HideSlider() => _slider.gameObject.SetActive(false);
        public void ShowErrorText() => _errorsHolder.SetActive(true); public void HideErrorText() => _errorsHolder.SetActive(false);

        public void OpenUserProfile() => Application.OpenURL("https://toottally.com/profile/" + Plugin.userInfo.id);
        public void OpenLoginPage() => Application.OpenURL("https://toottally.com/login");
        public void OpenSongLeaderboard() => Application.OpenURL("https://toottally.com/song/" + _currentSelectedSongHash);

        public void ScrollToLocalScore()
        {
            if (_localScoreId == -1)
                PopUpNotifManager.DisplayNotif("You don't have a score on that leaderboard yet", GameTheme.themeColors.notification.defaultText);
            else if (_scoreGameObjectList.Count > 8)
            {
                _slider.value = _localScoreId / (_scoreGameObjectList.Count - 8f);
                _slider.onValueChanged.Invoke(_slider.value);
            }
        }

        private void SetTabsImages()
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject currentTab = _globalLeaderboard.GetComponent<LeaderboardManager>().tabs[i];

                Button btn = currentTab.GetComponentInChildren<Button>();
                btn.image.sprite = AssetManager.GetSprite(tabsImageNames[i]);

            }
            _tabs.SetActive(true);
        }

        public enum LeaderboardState
        {
            None,
            ErrorNoSongHashFound,
            ErrorNoLeaderboardFound,
            ErrorUnexpected,
            ReadyToRefresh,
            SongDataLoaded,
            SongDataMissing
        }

        private void DestroyFromParent(GameObject parent, string objectName) => GameObject.DestroyImmediate(parent.transform.Find(objectName).gameObject);
    }
}
