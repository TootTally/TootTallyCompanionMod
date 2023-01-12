using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SimpleJSON;
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

namespace TootTally.CustomLeaderboard
{
    public class GlobalLeaderboard
    {
        #region constants
        private const string ERROR_NO_LEADERBOARD_FOUND_TEXT = "Could not find a leaderboard for this track.\n <size=15>Be the first one to set a score on the track!</size>"; //lol
        private const string ERROR_NO_SONGHASH_FOUND_TEXT = "Error loading this track's leaderboard...\n <size=15>If you see this error, please contact TootTally's devs on discord</size>";
        private const float SWIRLY_SPEED = 0.5f;
        private static Dictionary<string, Color> gradeToColorDict = new Dictionary<string, Color> { { "SSS", Color.yellow }, { "SS", Color.yellow }, { "S", Color.yellow }, { "A", Color.green }, { "B", new Color(0, .4f, 1f) }, { "C", Color.magenta }, { "D", Color.red }, { "F", Color.grey }, };
        private static string[] tabsImageNames = { "profile.png", "global.png", "local.png" };
        #endregion

        private List<IEnumerator<UnityWebRequestAsyncOperation>> _currentLeaderboardCoroutines;

        private LevelSelectController _levelSelectControllerInstance;

        private List<SerializableClass.ScoreDataFromDB> _scoreDataList;

        private GraphicRaycaster _globalLeaderboardGraphicRaycaster;
        private List<RaycastResult> _raycastHitList;

        private GameObject _leaderboard, _globalLeaderboard, _scoreboard, _errorsHolder, _tabs, _loadingSwirly;
        private Text _errorText;
        private List<LeaderboardRowEntry> _scoreGameObjectList;
        private Slider _slider;
        private GameObject _sliderHandle;

        private int _currentSelectedSongHash;
        public bool HasLeaderboard => _leaderboard != null;

        private float _scrollAcceleration;

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
            LoadTabsImages();

            _loadingSwirly = panelBody.transform.Find("loadingspinner_parent").gameObject;
            ShowLoadingSwirly();

            _slider = panelBody.transform.Find("LeaderboardVerticalSlider").gameObject.GetComponent<Slider>();
            _sliderHandle = _slider.transform.Find("Handle").gameObject;
            SetOnSliderValueChangeEvent();
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
                DestroyFromParent(_leaderboard, i.ToString());
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

        public void UpdateLeaderboard(List<SingleTrackData> ___alltrackslist, Action<LeaderboardState> callback)
        {
            _globalLeaderboard.SetActive(true); //for some reasons its needed to display the leaderboard

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
                    return; // Skip if no song found
                }
                else
                    _currentSelectedSongHash = songHashInDB;

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
                    CancelAndClearAllCoroutineInList();
                }));
                Plugin.Instance.StartCoroutine(_currentLeaderboardCoroutines.Last());
            }));
            Plugin.Instance.StartCoroutine(_currentLeaderboardCoroutines.Last());
        }

        public void RefreshLeaderboard()
        {
            var count = 1;
            foreach (SerializableClass.ScoreDataFromDB scoreData in _scoreDataList)
            {
                LeaderboardRowEntry rowEntry = GameObjectFactory.CreateLeaderboardRowEntryFromScore(_scoreboard.transform, "RowEntry" + scoreData.player, scoreData, count, gradeToColorDict[scoreData.grade], _levelSelectControllerInstance);
                _scoreGameObjectList.Add(rowEntry);
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

        public bool IsMouseOver() => _raycastHitList.Count > 0;

        public bool IsScrollAccelerationNotNull() => _scrollAcceleration != 0;

        public void UpdateScrolling()
        {
            _slider.value += _scrollAcceleration;
            _scrollAcceleration *= 106 * Time.deltaTime;
        }

        public void AddScrollAcceleration(float value)
        {
            if (_scoreGameObjectList.Count > 8)
                _scrollAcceleration -= value;
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

        public void OpenUserProfile() => Application.OpenURL("https://toottally.com/profile/" + ReplaySystemJson.userInfo.id);
        public void OpenSongLeaderboard() => Application.OpenURL("https://toottally.com/song/" + _currentSelectedSongHash);



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

        private void LoadTabsImages()
        {
            int count = 0;

            for (int i = 0; i < 3; i++)
            {
                GameObject currentTab = _globalLeaderboard.GetComponent<LeaderboardManager>().tabs[i];

                Button btn = currentTab.GetComponentInChildren<Button>();
                ColorBlock btnColorBlock = btn.colors;
                btnColorBlock.pressedColor = new Color(1, 1, 0, 1);
                btnColorBlock.highlightedColor = new Color(.75f, .75f, .75f, 1);
                btn.colors = btnColorBlock;

                Image icon = currentTab.GetComponent<Image>();

                Plugin.Instance.StartCoroutine(TootTallyAPIService.LoadTextureFromServer("http://cdn.toottally.com/assets/" + tabsImageNames[i], (texture) =>
                {
                    icon.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 300f);
                    btn.image.sprite = icon.sprite;
                    count++;
                    Plugin.LogInfo("CurrentTabs loaded:" + count);
                    if (count == 3) //all 3 tabs' loaded
                        _tabs.SetActive(true);


                }));
            }

        }

        private static string GetSongHash(string trackRef) => Plugin.Instance.CalcFileHash(Plugin.SongSelect.GetSongFilePath(true, trackRef));

        public enum LeaderboardState
        {
            None,
            ErrorNoSongHashFound,
            ErrorNoLeaderboardFound,
            ErrorUnexpected,
            ReadyToRefresh,
        }

        private void DestroyFromParent(GameObject parent, string objectName) => GameObject.DestroyImmediate(parent.transform.Find(objectName).gameObject);
    }
}
