using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TootTally.Replays;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Playables;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using TootTally.Graphics;
using TootTally.Utils;
using UnityEngine.EventSystems;
using TootTally.Utils.Helpers;

namespace TootTally.CustomLeaderboard
{
    public class GlobalLeaderboard
    {
        #region constants
        private const string ERROR_NO_LEADERBOARD_FOUND_TEXT = "Could not find a leaderboard for this track.\n <size=15>Be the first one to set a score on the track!</size>"; //lol
        private const string ERROR_NO_SONGHASH_FOUND_TEXT = "This chart is not uploaded to TootTally...\n <size=15>Clicking Play should automatically upload the chart\n if you have it turned on in your TootTally.cfg file</size>";
        private const float SWIRLY_SPEED = 0.5f;
        private static Dictionary<string, Color> gradeToColorDict = new Dictionary<string, Color> { { "SSS", Color.yellow }, { "SS", Color.yellow }, { "S", Color.yellow }, { "A", Color.green }, { "B", new Color(0, .4f, 1f) }, { "C", Color.magenta }, { "D", Color.red }, { "F", Color.grey }, };
        private static string[] tabsImageNames = { "profile64.png", "global64.png", "local64.png" };
        private static float[] _starSizeDeltaPositions = { 0, 20, 58, 96, 134, 172, 210, 248, 285, 324, 361 };
        #endregion

        private List<IEnumerator<UnityWebRequestAsyncOperation>> _currentLeaderboardCoroutines;

        private LevelSelectController _levelSelectControllerInstance;

        private List<SerializableClass.ScoreDataFromDB> _scoreDataList;

        private GraphicRaycaster _globalLeaderboardGraphicRaycaster;
        private List<RaycastResult> _raycastHitList;

        private GameObject _leaderboard, _globalLeaderboard, _scoreboard, _errorsHolder, _tabs, _loadingSwirly;
        private Text _errorText, _diffRating;
        private Vector2 _starRatingMaskSizeTarget;
        private RectTransform _diffRatingMaskRectangle;
        private List<LeaderboardRowEntry> _scoreGameObjectList;
        private SerializableClass.SongDataFromDB _songData;
        private Slider _slider;
        private GameObject _sliderHandle;

        private int _currentSelectedSongHash, _localScoreId;
        public bool HasLeaderboard => _leaderboard != null;

        private float _scrollAcceleration;

        private EasingHelper.SecondOrderDynamics _starMaskAnimation;

        public void Initialize(LevelSelectController __instance)
        {
            _levelSelectControllerInstance = __instance;
            _currentLeaderboardCoroutines = new List<IEnumerator<UnityWebRequestAsyncOperation>>();
            _scoreGameObjectList = new List<LeaderboardRowEntry>();

            ClearBaseLeaderboard();
            CustomizeGameMenuUI();

            _globalLeaderboard = GameObjectFactory.CreateSteamLeaderboardFromPrefab(_leaderboard.transform, "GlobalLeaderboard");
            _globalLeaderboard.SetActive(true);
            _globalLeaderboardGraphicRaycaster = _globalLeaderboard.GetComponent<GraphicRaycaster>();
            _raycastHitList = new List<RaycastResult>();


            GameObject panelBody = _globalLeaderboard.transform.Find("PanelBody").gameObject;
            panelBody.SetActive(true);
            _scoreboard = panelBody.transform.Find("scoreboard").gameObject;
            _scoreboard.SetActive(true);

            _errorsHolder = panelBody.transform.Find("errors").gameObject;

            _errorText = _errorsHolder.transform.Find("error_noleaderboard").GetComponent<Text>();
            _errorText.gameObject.SetActive(true);

            _tabs = panelBody.transform.Find("tabs").gameObject; //Hidden until icons are loaded
            SetTabsImages();

            _loadingSwirly = panelBody.transform.Find("loadingspinner_parent").gameObject;
            ShowLoadingSwirly();

            _slider = panelBody.transform.Find("LeaderboardVerticalSlider").gameObject.GetComponent<Slider>();
            _slider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.fill;
            _slider.transform.Find("Background").GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.background;
            _sliderHandle = _slider.transform.Find("Handle").gameObject;
            _sliderHandle.GetComponent<Image>().color = GameTheme.themeColors.leaderboard.slider.handle;

            SetOnSliderValueChangeEvent();

            GameObject diffBar = GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "diff bar");
            GameObject diffStarsHolder = GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "difficulty stars");
            _diffRatingMaskRectangle = diffStarsHolder.GetComponent<RectTransform>();
            _diffRatingMaskRectangle.anchoredPosition = new Vector2(-284, -48);
            _diffRatingMaskRectangle.sizeDelta = new Vector2(0, 30);
            diffStarsHolder.AddComponent<Mask>();
            Image imageMask = diffStarsHolder.AddComponent<Image>();
            imageMask.color = new Color(0, 0, 0, 0.01f); //if set at 0 stars wont display ?__? 
            diffBar.GetComponent<RectTransform>().sizeDelta += new Vector2(41.5f, 0);
            _diffRating = GameObjectFactory.CreateSingleText(diffBar.transform, "diffRating", "", GameTheme.themeColors.leaderboard.text);
            _diffRating.gameObject.GetComponent<Outline>().effectColor = GameTheme.themeColors.leaderboard.textOutline;
            _diffRating.fontSize = 20;
            _diffRating.alignment = TextAnchor.MiddleRight;
            _diffRating.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 30);

            _starMaskAnimation = new EasingHelper.SecondOrderDynamics(1.23f, 1f, 1.2f);
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
            //fuck that useless Dial
            GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "Dial").gameObject.SetActive(false);

            //move capsules to the left
            GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "capsules").GetComponent<RectTransform>().anchoredPosition = new Vector2(-275, 32);

            //move random_btn next to capsules
            GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "RANDOM_btn").GetComponent<RectTransform>().anchoredPosition = new Vector2(-123, -7);

            //Patch current slider and move it slightly above RANDOM_btn
            BetterScrollSpeedSliderPatcher.PatchScrollSpeedSlider();
            GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "Slider").GetComponent<RectTransform>().anchoredPosition = new Vector2(-115, 23);
            GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "ScrollSpeedShad").GetComponent<RectTransform>().anchoredPosition = new Vector2(-112, 36);
        }

        public void UpdateLeaderboard(LevelSelectController __instance, List<SingleTrackData> ___alltrackslist, Action<LeaderboardState> callback)
        {
            _globalLeaderboard.SetActive(true); //for some reasons its needed to display the leaderboard
            _scrollAcceleration = 0;

            string trackRef = ___alltrackslist[_levelSelectControllerInstance.songindex].trackref;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            string songHash = GetChoosenSongHash(trackRef);

            if (_currentLeaderboardCoroutines.Count != 0) CancelAndClearAllCoroutineInList();

            _currentLeaderboardCoroutines.Add(TootTallyAPIService.GetHashInDB(songHash, isCustom, (songHashInDB) =>
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
                _songData = null;
                _scoreDataList = null;
                _currentLeaderboardCoroutines.Add(TootTallyAPIService.GetSongDataFromDB(songHashInDB, (songData) =>
                {
                    if (songData != null)
                    {
                        _songData = songData;
                        _diffRating.text = _songData.difficulty.ToString("0.0");

                        int roundedUpStar = (int)Mathf.Clamp(_songData.difficulty + 1, 1, 10);
                        int roundedDownStar = (int)Mathf.Clamp(_songData.difficulty, 0, 9);
                        _starMaskAnimation.SetStartVector(_diffRatingMaskRectangle.sizeDelta);
                        _starRatingMaskSizeTarget = new Vector2(EasingHelper.Lerp(_starSizeDeltaPositions[roundedUpStar], _starSizeDeltaPositions[roundedDownStar], roundedUpStar - _songData.difficulty), 30);
                    }
                    else
                    {
                        _diffRating.text = "NA";
                        _starMaskAnimation.SetStartVector(_diffRatingMaskRectangle.sizeDelta);
                        _starRatingMaskSizeTarget = new Vector2(_starSizeDeltaPositions[0], 30);
                    }


                    if (_scoreDataList != null)
                        CancelAndClearAllCoroutineInList();
                }));
                Plugin.Instance.StartCoroutine(_currentLeaderboardCoroutines.Last());
                _currentLeaderboardCoroutines.Add(TootTallyAPIService.GetLeaderboardScoresFromDB(songHashInDB, (scoreDataList) =>
                {
                    if (scoreDataList != null)
                    {
                        _scoreDataList = scoreDataList;
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
                if (_value == 0f || _value == 1f)
                    _scrollAcceleration = 0;


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

        public void UpdateRaycastHitList()
        {
            PointerEventData pointerData = new PointerEventData(null);
            pointerData.position = Input.mousePosition;
            _raycastHitList.Clear();
            _globalLeaderboardGraphicRaycaster.Raycast(pointerData, _raycastHitList);
        }

        public void UpdateStarRatingAnimation()
        {
            _diffRatingMaskRectangle.sizeDelta = _starMaskAnimation.GetNewVector(_starRatingMaskSizeTarget, Time.deltaTime);
        }

        public bool IsMouseOver() => _raycastHitList.Count > 0;

        public bool IsScrollAccelerationNotNull() => _scrollAcceleration != 0;

        public void UpdateScrolling()
        {
            _slider.value += _scrollAcceleration * Time.deltaTime;
            _scrollAcceleration *= 130f * Time.deltaTime; //Abitrary value just so it looks nice / feel nice
        }

        public void AddScrollAcceleration(float value)
        {
            if (_scoreGameObjectList.Count > 8)
                _scrollAcceleration -= (value * 0.01f) / Time.deltaTime;
        }

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
        public void ShowLoadingSwirly() => _loadingSwirly.SetActive(true); public void HideLoadingSwirly() => _loadingSwirly.SetActive(false);
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


        public void UpdateLoadingSwirlyAnimation()
        {
            if (_loadingSwirly != null)
                _loadingSwirly.GetComponent<RectTransform>().Rotate(0, 0, 1000 * Time.deltaTime * SWIRLY_SPEED);
        }

        private static string GetChoosenSongHash(string trackRef)
        {
            bool isCustom = Globals.IsCustomTrack(trackRef);
            return isCustom ? GetSongHash(trackRef) : trackRef;
        }

        private void SetTabsImages()
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject currentTab = _globalLeaderboard.GetComponent<LeaderboardManager>().tabs[i];

                Button btn = currentTab.GetComponentInChildren<Button>();
                Texture2D texture = AssetManager.GetTexture(tabsImageNames[i]);
                if (texture != null)
                    btn.image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 300f);

            }
            _tabs.SetActive(true);
        }

        private static string GetSongHash(string trackRef) => SongDataHelper.CalcFileHash(GetSongFilePath(true, trackRef));
        private static string GetSongFilePath(bool isCustom, string trackRef)
        {
            return isCustom ?
                Path.Combine(Globals.ChartFolders[trackRef], "song.tmb") :
                $"{Application.streamingAssetsPath}/leveldata/{trackRef}.tmb";
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
