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

namespace TootTally.Graphics
{
    public static class NewCustomLeaderboard
    {
        private const string FULLSCREEN_PANEL_PATH = "MainCanvas/FullScreenPanel/";
        private const string LEADERBOARD_CANVAS_PATH = "Camera-Popups/LeaderboardCanvas";
        private const string NO_SCORE_ERROR_TEXT = "Could not find a leaderboard for this track.\n <size=15>Be the first one to set a score on the track!</size>"; //lol
        private const string CANT_LOAD_SONG_ERROR_TEXT = "Error loading this track's leaderboard...\n <size=15>If you see this error, please contact TootTally's devs on discord</size>";
        private static Dictionary<string, Color> gradeToColorDict = new Dictionary<string, Color> { { "S", Color.yellow }, { "A", Color.green }, { "B", new Color(0, .4f, 1f) }, { "C", Color.magenta }, { "D", Color.red }, { "F", Color.grey }, };

        private const float SWIRLY_SPEED = 0.5f;

        private static bool _leaderboardLoaded;

        private static LevelSelectController _levelSelectControllerInstance;
        private static List<IEnumerator<UnityWebRequestAsyncOperation>> currentLeaderboardCoroutines;

        private static GameObject _leaderboard, _leaderboardCanvas, _panelBody, _scoreboard, _errorsHolder, _loadingSwirly;
        private static Text _leaderboardHeaderPrefab, _leaderboardTextPrefab, _errorText;
        private static LeaderboardManager _leaderboardManager;
        private static LeaderboardRowEntry _singleRowPrefab;
        private static List<LeaderboardRowEntry> _scoreGameObjectList;
        private static Slider _slider;

        private static void QuickLog(string message) => Plugin.LogInfo(message);

        #region init
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void YoinkLotOfGraphics(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            _levelSelectControllerInstance = __instance;
            _leaderboardLoaded = false;

            Initialize();
            UpdateLeaderboard(___alltrackslist, __instance);
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.populateScores))]
        [HarmonyPrefix]
        static bool DontPopulateBaseGameLeaderboard() => false;

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Update))]
        [HarmonyPostfix]
        static void UpdateLoadingStarAnimation()
        {
            if (!_leaderboardLoaded && _loadingSwirly != null)
            {
                _loadingSwirly.GetComponent<RectTransform>().Rotate(0, 0, 1000 * Time.deltaTime * SWIRLY_SPEED);
            }
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.sortTracks))]
        [HarmonyPostfix]
        static void OnTrackSortReloadLeaderboard(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            if (_leaderboard != null)
                UpdateLeaderboard(___alltrackslist, __instance);
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.showButtonsAfterRandomizing))]
        [HarmonyPostfix]
        static void OnDoneRandomizingReloadLeaderboard(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance) =>
            UpdateLeaderboard(___alltrackslist, __instance);


        public static void Initialize()
        {
            currentLeaderboardCoroutines = new List<IEnumerator<UnityWebRequestAsyncOperation>>();
            _scoreGameObjectList = new List<LeaderboardRowEntry>();

            //fuck that useless Dial
            GameObject.Find(FULLSCREEN_PANEL_PATH + "Dial").gameObject.SetActive(false);
            //move capsules to the left
            GameObject.Find(FULLSCREEN_PANEL_PATH + "capsules").GetComponent<RectTransform>().anchoredPosition = new Vector2(-275, 32);
            //move random_btn next to capsules
            GameObject.Find(FULLSCREEN_PANEL_PATH + "RANDOM_btn").GetComponent<RectTransform>().anchoredPosition = new Vector2(-123, -7);
            //move slider slightly above RANDOM_btn and make it better B)
            BetterScrollSpeedSliderPatcher.PatchScrollSpeedSlider();
            GameObject.Find(FULLSCREEN_PANEL_PATH + "Slider").GetComponent<RectTransform>().anchoredPosition = new Vector2(-115, 23);
            GameObject.Find(FULLSCREEN_PANEL_PATH + "ScrollSpeedShad").GetComponent<RectTransform>().anchoredPosition = new Vector2(-112, 36);

            _leaderboard = GameObject.Find(FULLSCREEN_PANEL_PATH + "Leaderboard").gameObject;

            //clear original Leaderboard from its objects
            GameObject.DestroyImmediate(_leaderboard.transform.Find(".......").gameObject);
            GameObject.DestroyImmediate(_leaderboard.transform.Find("\"HIGH SCORES\"").gameObject);
            for (int i = 1; i <= 5; i++)
            {
                _leaderboard.transform.Find(i.ToString()).gameObject.SetActive(false);
                _leaderboard.transform.Find("score" + i).gameObject.SetActive(false);
            }

            GameObject camerapopups = GameObject.Find("Camera-Popups").gameObject;
            GameObject newLeaderboardCanvas = camerapopups.transform.Find("LeaderboardCanvas").gameObject;

            _leaderboardCanvas = GameObject.Instantiate(newLeaderboardCanvas, _leaderboard.transform);
            _leaderboardManager = _leaderboardCanvas.GetComponent<LeaderboardManager>();
            //Don't think we need these...
            GameObject.DestroyImmediate(_leaderboardCanvas.transform.Find("BG").gameObject);
            GameObject.DestroyImmediate(_leaderboardCanvas.GetComponent<CanvasScaler>());
            _leaderboardCanvas.name = "CustomLeaderboarCanvas";
            _leaderboardCanvas.SetActive(true); //Has to be set to true else it crashes when yoinking other objects?? #UnityStuff

            RectTransform lbCanvasRect = _leaderboardCanvas.GetComponent<RectTransform>();
            lbCanvasRect.anchoredPosition = new Vector2(237, -311);
            lbCanvasRect.localScale = Vector2.one * 0.5f;


            _panelBody = _leaderboardCanvas.transform.Find("PanelBody").gameObject;
            _panelBody.SetActive(true);
            RectTransform panelRectTransform = _panelBody.GetComponent<RectTransform>();
            panelRectTransform.anchoredPosition = Vector2.zero;
            panelRectTransform.sizeDelta = new Vector2(750, 300);
            //We dont need these right?
            GameObject.DestroyImmediate(_panelBody.transform.Find("CloseButton").gameObject);
            GameObject.DestroyImmediate(_panelBody.transform.Find("txt_legal").gameObject);
            GameObject.DestroyImmediate(_panelBody.transform.Find("txt_leaderboards").gameObject);
            GameObject.DestroyImmediate(_panelBody.transform.Find("txt_songname").gameObject);
            GameObject.DestroyImmediate(_panelBody.transform.Find("rule").gameObject);

            //Hidding it for now, gonna use later
            GameObject tabs = _panelBody.transform.Find("tabs").gameObject;
            GameObject.DestroyImmediate(tabs.GetComponent<HorizontalLayoutGroup>());
            tabs.AddComponent<VerticalLayoutGroup>();
            RectTransform tabsRectTransform = tabs.GetComponent<RectTransform>();
            tabsRectTransform.anchoredPosition = new Vector2(290, -24);
            tabsRectTransform.sizeDelta = new Vector2(-650, 225);
            tabs.SetActive(false);

            _errorsHolder = _panelBody.transform.Find("errors").gameObject;
            RectTransform errorsTransform = _errorsHolder.GetComponent<RectTransform>();
            errorsTransform.anchoredPosition = new Vector2(-30, -120);
            errorsTransform.sizeDelta = new Vector2(-200, -190);
            _errorsHolder.SetActive(false);
            _errorText = _errorsHolder.transform.Find("error_noleaderboard").GetComponent<Text>();

            GameObject scoresbody = _panelBody.transform.Find("scoresbody").gameObject;
            RectTransform scoresbodyRectTransform = scoresbody.GetComponent<RectTransform>();
            scoresbodyRectTransform.anchoredPosition = new Vector2(0, -10);
            scoresbodyRectTransform.sizeDelta = Vector2.one * -20;

            _scoreboard = _panelBody.transform.Find("scoreboard").gameObject; //put SingleScore in there
            _scoreboard.AddComponent<RectMask2D>();
            RectTransform scoreboardRectTransform = _scoreboard.GetComponent<RectTransform>();
            scoreboardRectTransform.anchoredPosition = new Vector2(-22, -10);
            scoreboardRectTransform.sizeDelta = new Vector2(-80, -20);

            _loadingSwirly = _panelBody.transform.Find("loadingspinner_parent").gameObject; //Contains swirly, spin the container and not swirly.
            _loadingSwirly.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, 5);
            _loadingSwirly.SetActive(true);

            GameObject singleScore = _panelBody.transform.Find("scoreboard/SingleScore").gameObject;
            GameObject mySingleScore = GameObject.Instantiate(singleScore, _leaderboardCanvas.transform);
            mySingleScore.name = "singleScorePrefab";
            mySingleScore.GetComponent<RectTransform>().sizeDelta = new Vector2(mySingleScore.GetComponent<RectTransform>().sizeDelta.x, 35);
            mySingleScore.gameObject.SetActive(false);
            _leaderboardManager.scores.ToList().ForEach(score => GameObject.DestroyImmediate(score.gameObject));

            _leaderboardHeaderPrefab = GameObject.Instantiate(mySingleScore.transform.Find("Num").GetComponent<Text>(), _leaderboardCanvas.transform);
            _leaderboardHeaderPrefab.alignment = TextAnchor.MiddleCenter;
            _leaderboardHeaderPrefab.horizontalOverflow = HorizontalWrapMode.Overflow;
            _leaderboardHeaderPrefab.maskable = true;
            _leaderboardHeaderPrefab.gameObject.AddComponent<Outline>();
            _leaderboardTextPrefab = GameObject.Instantiate(mySingleScore.transform.Find("Name").GetComponent<Text>(), _leaderboardCanvas.transform);
            _leaderboardTextPrefab.alignment = TextAnchor.MiddleCenter;
            _leaderboardTextPrefab.horizontalOverflow = HorizontalWrapMode.Overflow;
            _leaderboardTextPrefab.maskable = true;
            _leaderboardTextPrefab.gameObject.AddComponent<Outline>();
            GameObject.DestroyImmediate(mySingleScore.transform.Find("Num").gameObject);
            GameObject.DestroyImmediate(mySingleScore.transform.Find("Name").gameObject);
            GameObject.DestroyImmediate(mySingleScore.transform.Find("Score").gameObject);

            _singleRowPrefab = mySingleScore.AddComponent<LeaderboardRowEntry>();
            Text rank = GameObject.Instantiate(_leaderboardHeaderPrefab, mySingleScore.transform);
            rank.name = "rank";
            Text username = GameObject.Instantiate(_leaderboardTextPrefab, mySingleScore.transform);
            username.name = "username";
            Text score = GameObject.Instantiate(_leaderboardTextPrefab, mySingleScore.transform);
            score.name = "score";
            Text percent = GameObject.Instantiate(_leaderboardTextPrefab, mySingleScore.transform);
            percent.name = "percent";
            Text grade = GameObject.Instantiate(_leaderboardTextPrefab, mySingleScore.transform);
            grade.name = "grade";
            Text maxcombo = GameObject.Instantiate(_leaderboardTextPrefab, mySingleScore.transform);
            maxcombo.name = "maxcombo";
            _singleRowPrefab.ConstructLeaderboardEntry(mySingleScore, rank, username, score, percent, grade, maxcombo, false);
            _singleRowPrefab.singleScore.name = "singleRowPrefab";
            GameObject.DontDestroyOnLoad(_singleRowPrefab);

            //Yoink slider and make it vertical
            Slider sliderPrefab = GameObject.Find(FULLSCREEN_PANEL_PATH + "Slider").GetComponent<Slider>(); //yoink
            RectTransform sliderPrefabRect = sliderPrefab.GetComponent<RectTransform>();

            _slider = GameObject.Instantiate(sliderPrefab, _panelBody.transform);
            _slider.direction = Slider.Direction.TopToBottom;
            RectTransform sliderRect = _slider.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(25, 745);
            sliderRect.anchoredPosition = new Vector2(300, 0);
            _slider.handleRect = sliderRect;
            RectTransform backgroundSliderRect = _slider.transform.Find("Background").GetComponent<RectTransform>();
            backgroundSliderRect.anchoredPosition = new Vector2(-5, backgroundSliderRect.anchoredPosition.y);
            backgroundSliderRect.sizeDelta = new Vector2(-10, backgroundSliderRect.sizeDelta.y);
            _slider.value = 0f;
            _slider.minValue = 0f;
            _slider.maxValue = 1f;
            _slider.gameObject.SetActive(false);
            GameObject.DestroyImmediate(_slider.transform.Find("Handle Slide Area/Handle").gameObject);

        }
        #endregion

        #region update
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPostfix]
        static void UpdateLeaderboard(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            _leaderboardCanvas.SetActive(true); //for some reasons its needed to display the leaderboard
            if (_leaderboardLoaded)
            {
                _leaderboardLoaded = false;
                _loadingSwirly.SetActive(true);
                _slider.gameObject.SetActive(false);
                _errorsHolder.SetActive(false);
                ClearLeaderboard();
            }

            if (__instance.randomizing) return; //Do nothing if randomizing

            string trackRef = ___alltrackslist[__instance.songindex].trackref;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            string songHash = GetChoosenSongHash(trackRef);

            if (currentLeaderboardCoroutines.Count != 0)
            {
                currentLeaderboardCoroutines.ForEach(routine => Plugin.Instance.StopCoroutine(routine));
                currentLeaderboardCoroutines.Clear();
            }

            currentLeaderboardCoroutines.Add(TootTallyAPIService.GetHashInDB(songHash, isCustom, (songHashInDB) =>
             {
                 if (songHashInDB == 0)
                 {
                     _errorsHolder.SetActive(true);
                     _errorText.text = CANT_LOAD_SONG_ERROR_TEXT;
                     _loadingSwirly.SetActive(false);
                     _leaderboardLoaded = true;
                     return; // Skip if no song found
                 }
                 currentLeaderboardCoroutines.Add(TootTallyAPIService.GetLeaderboardScoresFromDB(songHashInDB, (scoreDataList) =>
                  {
                      if (scoreDataList != null)
                      {
                          RefreshLeaderboard(scoreDataList);
                          _leaderboardLoaded = true;
                          _loadingSwirly.SetActive(false);
                          _slider.gameObject.SetActive(_scoreGameObjectList.Count > 8);
                      }
                      else
                      {
                          _errorsHolder.SetActive(true);
                          _errorText.text = NO_SCORE_ERROR_TEXT;
                      }


                      _leaderboardLoaded = true;
                      _loadingSwirly.SetActive(false);
                      currentLeaderboardCoroutines.Clear();

                  }));
                 Plugin.Instance.StartCoroutine(currentLeaderboardCoroutines.Last());
             }));
            Plugin.Instance.StartCoroutine(currentLeaderboardCoroutines.Last());
        }

        public static void ClearLeaderboard()
        {
            _scoreGameObjectList.ForEach(score => GameObject.DestroyImmediate(score.singleScore));
            _scoreGameObjectList.Clear();
        }

        public static void RefreshLeaderboard(List<SerializableClass.ScoreDataFromDB> scoreDataLists)
        {
            var count = 1;
            foreach (SerializableClass.ScoreDataFromDB scoreData in scoreDataLists)
            {
                LeaderboardRowEntry rowEntry = GameObject.Instantiate(_singleRowPrefab, _scoreboard.transform);
                rowEntry.username.text = scoreData.player;
                rowEntry.score.text = string.Format("{0:n0}", scoreData.score);
                rowEntry.rank.text = "#" + count;
                rowEntry.percent.text = scoreData.percentage.ToString("0.00") + "%";
                rowEntry.grade.text = scoreData.grade;
                if (scoreData.grade == "SS")
                    MakeDoubleSText(rowEntry);
                else if (scoreData.grade == "SSS")
                    MakeTripleSText(rowEntry);
                else
                    rowEntry.grade.color = gradeToColorDict[rowEntry.grade.text];
                rowEntry.maxcombo.text = scoreData.max_combo + "x";
                rowEntry.replayId = scoreData.replay_id;
                rowEntry.rowId = count;
                count++;
                rowEntry.singleScore.AddComponent<CanvasGroup>();
                HorizontalLayoutGroup layoutGroup = rowEntry.singleScore.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.childAlignment = TextAnchor.MiddleLeft;
                layoutGroup.childForceExpandWidth = layoutGroup.childForceExpandHeight = false;
                layoutGroup.childScaleWidth = layoutGroup.childScaleHeight = false;
                layoutGroup.childControlWidth = layoutGroup.childControlHeight = false;
                layoutGroup.spacing = 8;
                rowEntry.singleScore.SetActive(true);
                _scoreGameObjectList.Add(rowEntry);

                var replayId = rowEntry.replayId;
                if (replayId != "NA") //if there's a uuid, add a replay button
                {
                    GameObjectFactory.CreateCustomButton(rowEntry.singleScore.transform, Vector2.zero, new Vector2(26, 26), "►", "ReplayButton",
                    delegate
                    {
                        if (ReplaySystemJson.LoadReplay(replayId)) //Try loading replay locally
                            _levelSelectControllerInstance.playbtn.onClick?.Invoke();
                        else //Download it first, then try loading again
                        {
                            //add some loading indicator here to let user know replay is being downloaded
                            Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadReplay(replayId, (uuid) =>
                               {
                                   if (ReplaySystemJson.LoadReplay(uuid)) //Replay Successfully downloaded... trying to load again
                                       _levelSelectControllerInstance.playbtn.onClick?.Invoke();
                               }));
                        }
                    });
                }
            }

            _slider.onValueChanged.RemoveAllListeners();
            _slider.onValueChanged.AddListener((float _value) =>
            {
                foreach (LeaderboardRowEntry row in _scoreGameObjectList)
                {
                    RectTransform rect = row.singleScore.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, ((row.rowId - 1) * -35) + (_slider.value * 35 * (_scoreGameObjectList.Count - 8)) - 17);
                    if (rect.anchoredPosition.y >= -15)
                        row.GetComponent<CanvasGroup>().alpha = Math.Max(1 - ((rect.anchoredPosition.y + 15) / 35), 0);
                    else if (rect.anchoredPosition.y - 35 <= 35 * 8 + 15)
                        row.GetComponent<CanvasGroup>().alpha = Math.Max((rect.anchoredPosition.y + (35 * 8) + 15) / 35, 0);
                    else
                        row.GetComponent<CanvasGroup>().alpha = 1;
                }

                if (_value < 0f)
                    _slider.value = 0f;
                if (_value > 1f)
                    _slider.value = 1f;
            });
            #endregion
        }

        //not working yet
        public static void MakeDoubleSText(LeaderboardRowEntry rowEntry)
        {
            Text text = GameObject.Instantiate(_leaderboardTextPrefab, rowEntry.grade.transform);
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            text.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, 15);
            text.text = rowEntry.grade.text;
            text.color = Color.yellow;
        }
        public static void MakeTripleSText(LeaderboardRowEntry rowEntry)
        {
            Text text = GameObject.Instantiate(_leaderboardTextPrefab, rowEntry.grade.transform);
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            text.GetComponent<RectTransform>().anchoredPosition = new Vector2(3, 15);
            text.text = rowEntry.grade.text;


            Text text2 = GameObject.Instantiate(_leaderboardTextPrefab, rowEntry.grade.transform);
            text2.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);
            text2.GetComponent<RectTransform>().anchoredPosition = new Vector2(6, 12);
            text2.text = rowEntry.grade.text;
            text2.color = Color.yellow;
        }


        private static string GetChoosenSongHash(string trackRef)
        {
            bool isCustom = Globals.IsCustomTrack(trackRef);
            return isCustom ? GetSongHash(trackRef) : trackRef;
        }
        public static string GetSongHash(string trackRef) => Plugin.Instance.CalcFileHash(Plugin.SongSelect.GetSongFilePath(true, trackRef));
    }
}
