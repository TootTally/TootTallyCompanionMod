using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SimpleJSON;
using System.Drawing;
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

        private static bool _leaderboardLoaded;

        private static LevelSelectController _levelSelectControllerInstance;
        private static List<IEnumerator<UnityWebRequestAsyncOperation>> currentLeaderboardCoroutines;

        private static GameObject _leaderBoard, _leaderboardCanvas, _panelBody, _scoreboard, _loadingSwirly;
        private static LeaderboardManager _leaderboardManager;
        private static LeaderboardRowEntry _singleRowPrefab;

        private static void QuickLog(string message) => Plugin.LogInfo(message);

        #region init
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void YoinkLotOfGraphics(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            _levelSelectControllerInstance = __instance;
            _leaderboardLoaded = false;

            Initialize();
            //UpdateLeaderboard(___alltrackslist, __instance);
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.populateScores))]
        [HarmonyPrefix]
        static bool DontPopulateBaseGameLeaderboard() => false;



        public static void Initialize()
        {
            currentLeaderboardCoroutines = new List<IEnumerator<UnityWebRequestAsyncOperation>>();

            //fuck that useless Dial
            GameObject.Find(FULLSCREEN_PANEL_PATH + "Dial").gameObject.SetActive(false);
            //move capsules to the left
            GameObject.Find(FULLSCREEN_PANEL_PATH + "capsules").GetComponent<RectTransform>().anchoredPosition = new Vector2(-275, 32);
            //move random_btn next to capsules
            GameObject.Find(FULLSCREEN_PANEL_PATH + "RANDOM_btn").GetComponent<RectTransform>().anchoredPosition = new Vector2(-123, -7);
            //move slider slightly above RANDOM_btn
            GameObject.Find(FULLSCREEN_PANEL_PATH + "Slider").GetComponent<RectTransform>().anchoredPosition = new Vector2(-115, 23);
            GameObject.Find(FULLSCREEN_PANEL_PATH + "ScrollSpeedShad").GetComponent<RectTransform>().anchoredPosition = new Vector2(-112, 36);

            _leaderBoard = GameObject.Find(FULLSCREEN_PANEL_PATH + "Leaderboard").gameObject;
            //clear original Leaderboard from its objects
            QuickLog("Clearing shit from leaderboard");
            GameObject.DestroyImmediate(_leaderBoard.transform.Find(".......").gameObject);
            GameObject.DestroyImmediate(_leaderBoard.transform.Find("\"HIGH SCORES\"").gameObject);
           for(int i = 1; i <= 5; i++)
            {
                _leaderBoard.transform.Find(i.ToString()).gameObject.SetActive(false);
                _leaderBoard.transform.Find("score" + i).gameObject.SetActive(false);
            }
            
            QuickLog("Yoink streak time!!");
            QuickLog("Yoink leaderboard canvas");
            GameObject camerapopups = GameObject.Find("Camera-Popups").gameObject; 
            GameObject newLeaderboardCanvas = camerapopups.transform.Find("LeaderboardCanvas").gameObject;

            _leaderboardCanvas = GameObject.Instantiate(newLeaderboardCanvas, _leaderBoard.transform);
            GameObject.DestroyImmediate(_leaderboardCanvas.transform.Find("BG").gameObject);
            //Don't think we need these...
            GameObject.DestroyImmediate(_leaderboardCanvas.GetComponent<CanvasScaler>());
            GameObject.DestroyImmediate(_leaderboardCanvas.GetComponent<GraphicRaycaster>());
            _leaderboardCanvas.gameObject.name = "CustomLeaderboarCanvas";
            _leaderboardCanvas.active = false;


            RectTransform lbCanvasRect = _leaderboardCanvas.GetComponent<RectTransform>();
            lbCanvasRect.anchoredPosition = new Vector2(237, -311);
            lbCanvasRect.localScale = Vector2.one * 0.5f;
                
            _leaderboardManager = _leaderboardCanvas.GetComponent<LeaderboardManager>();

            QuickLog("Yoink PanelBody");
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

            GameObject tabs = _panelBody.transform.Find("tabs").gameObject;
            GameObject.DestroyImmediate(tabs.GetComponent<HorizontalLayoutGroup>());
            tabs.AddComponent<VerticalLayoutGroup>();
            RectTransform tabsRectTransform = tabs.GetComponent<RectTransform>();
            tabsRectTransform.anchoredPosition = new Vector2(290, -24);
            tabsRectTransform.sizeDelta = new Vector2(-650, 225);

            GameObject scoresbody = _panelBody.transform.Find("scoresbody").gameObject;
            RectTransform scoresbodyRectTransform = scoresbody.GetComponent<RectTransform>();
            scoresbodyRectTransform.anchoredPosition = new Vector2(0, -10);
            scoresbodyRectTransform.sizeDelta = Vector2.one * -20;

            _scoreboard = _panelBody.transform.Find("scoreboard").gameObject; //put SingleScore in there
            RectTransform scoreboardRectTransform = _scoreboard.GetComponent<RectTransform>();
            scoreboardRectTransform.anchoredPosition = new Vector2(-64, -10);
            scoreboardRectTransform.sizeDelta = new Vector2(-150, 0);

            _loadingSwirly = _panelBody.transform.Find("loadingspinner_parent").gameObject; //Contains swirly, spin the container and not swirly.

            QuickLog("Yoink singleScore");
            GameObject singleScore = _panelBody.transform.Find("scoreboard/SingleScore").gameObject;
            GameObject mySingleScore = GameObject.Instantiate(singleScore, _leaderboardCanvas.transform);

            QuickLog("Yoink Texts");
            Text rank = mySingleScore.transform.Find("Num").GetComponent<Text>();
            Text username = mySingleScore.transform.Find("Name").GetComponent<Text>();
            Text score = mySingleScore.transform.Find("Score").GetComponent<Text>();

            QuickLog("Make row prefab");
            _singleRowPrefab = _leaderboardCanvas.AddComponent<LeaderboardRowEntry>();
            _singleRowPrefab.ConstructLeaderboardEntry(mySingleScore, rank, username, score, false);
            _singleRowPrefab.gameObject.SetActive(false);
            GameObject.DontDestroyOnLoad(_singleRowPrefab);
        }
        #endregion

        /*#region update
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPostfix]
        static void UpdateLeaderboard(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            if (_leaderboardLoaded)
            {
                _leaderboardLoaded = false;
                _loadingStarList.ForEach(star => star.gameObject.SetActive(true));
                ClearLeaderboard();
            }

            if (__instance.randomizing) return; //Do nothing if randomizing

            string trackRef = ___alltrackslist[__instance.songindex].trackref;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            string songHash;
            if (isCustom)
            {
                string songFilePath = Plugin.SongSelect.GetSongFilePath(isCustom, trackRef);
                songHash = Plugin.Instance.CalcFileHash(songFilePath);
            }
            else
                songHash = trackRef;

            if (currentLeaderboardCoroutines.Count != 0)
            {
                currentLeaderboardCoroutines.ForEach(routine => Plugin.Instance.StopCoroutine(routine));
                currentLeaderboardCoroutines.Clear();
            }

            currentLeaderboardCoroutines.Add(TootTallyAPIService.GetHashInDB(songHash, (songHashInDB) =>
            {
                if (songHashInDB == 0) return; // Skip if no song found
                currentLeaderboardCoroutines.Add(TootTallyAPIService.GetLeaderboardScoresFromDB(songHashInDB, (scoreDataList) =>
                {
                    List<List<string>> scoresMatrix = new List<List<string>>();

                    int count = 1;
                    foreach (SerializableClass.ScoreDataFromDB scoreData in scoreDataList)
                    {
                        List<string> scoreDataText = new List<string>
                        {
                                "#" + count,
                                Truncate(scoreData.player, 8),
                                string.Format("{0:n0}",scoreData.score),
                                scoreData.percentage.ToString("0.00") + "%",
                                scoreData.grade,
                                scoreData.max_combo + "x",
                                scoreData.replay_id,
                        };
                        scoresMatrix.Add(scoreDataText);
                        count++;
                    }

                    RefreshLeaderboard(scoresMatrix);
                    _leaderboardLoaded = true;
                    _loadingStarList.ForEach(star => star.gameObject.SetActive(false));
                    currentLeaderboardCoroutines.Clear();
                }));
                Plugin.Instance.StartCoroutine(currentLeaderboardCoroutines.Last());
            }));
            Plugin.Instance.StartCoroutine(currentLeaderboardCoroutines.Last());
        }

        public static void ClearLeaderboard()
        {

        }

        public static void RefreshLeaderboard(List<List<string>> scoreDataLists)
        {
            LeaderBoardContainer lbContainer = CreateContainer(_leaderBoard, _diffBar);

            foreach (List<string> dataList in scoreDataLists)
            {
                LeaderBoardRowContainer rowContainer = CreateLeaderboardRow(lbContainer, _diffBar);

                for (int i = 0; i < dataList.Count - 1; i++) //count - 1 because last data is replay uuid
                {
                    LeaderBoardColumnContainer colContainer = CreateLeaderboardColumn(rowContainer, _diffBar);

                    LeaderboardText header = CreateLeaderBoardHeader(colContainer.transform, dataList[i], "LeaderboardHeader" + dataList[i]);
                    colContainer.AddGameObjectToList(header.gameObject);

                    if (i == 1 || i == 4)
                    {
                        i++;

                        LeaderboardText header2 = CreateLeaderBoardHeader(colContainer.transform, dataList[i], "LeaderboardHeader" + dataList[i]);
                        colContainer.AddGameObjectToList(header2.gameObject);
                    }

                }
                var replayId = dataList.Last();
                if (replayId != "NA") //if there's a uuid, add a replay button
                {
                    LeaderBoardColumnContainer colContainerReplay = CreateLeaderboardColumn(rowContainer, _diffBar);
                    CustomButton replayButton =
                        GameObjectFactory.CreateCustomButton(colContainerReplay.transform, new Vector2(92, 5), new Vector2(14, 14), "►", "ReplayButton",
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

                    colContainerReplay.AddGameObjectToList(replayButton.gameObject);
                }
            }
            lbContainer.OrganizeRows();


            //Yoink slider and make it vertical
            Slider sliderPrefab = GameObject.Find(FULLSCREEN_PANEL_PATH + "Slider").GetComponent<Slider>(); //yoink
            RectTransform sliderPrefabRect = sliderPrefab.GetComponent<RectTransform>();

            Slider mySlider = GameObject.Instantiate(sliderPrefab, lbContainer.transform);
            mySlider.direction = Slider.Direction.TopToBottom;
            RectTransform sliderRect = mySlider.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(sliderPrefabRect.sizeDelta.y, sliderPrefabRect.sizeDelta.x * 3.5f);
            sliderRect.anchoredPosition = new Vector2(lbContainer.rectTransform.sizeDelta.x / 2 + (PADDING_X * 5), (lbContainer.rectTransform.sizeDelta.y / 2) - (sliderRect.sizeDelta.y / 5) - PADDING_Y);
            mySlider.handleRect = sliderRect;
            RectTransform backgroundSliderRect = mySlider.transform.Find("Background").GetComponent<RectTransform>();
            backgroundSliderRect.anchoredPosition = new Vector2(-5, backgroundSliderRect.anchoredPosition.y);
            backgroundSliderRect.sizeDelta = new Vector2(-10, backgroundSliderRect.sizeDelta.y);
            mySlider.minValue = -0.1f;
            mySlider.maxValue = 1.1f;
            mySlider.onValueChanged.RemoveAllListeners();
            mySlider.onValueChanged.AddListener((float _value) =>
            {
                if (_value < 0)
                    mySlider.value = 0;
                if (_value > 1)
                    mySlider.value = 1f;
            });
            GameObject.DestroyImmediate(mySlider.transform.Find("Handle Slide Area/Handle").gameObject);
            lbContainer.AddSliderToContainer(mySlider);
        }
        #endregion
        */
    }
}
