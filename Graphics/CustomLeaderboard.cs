using HarmonyLib;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
    public static class CustomLeaderboard
    {
        private static GameObject _leaderBoard, _diffBar;
        private static LeaderboardText _leaderboardHeaderPrefab;
        private static Slider _scrollSlider;
        private static CustomButton[] _replayBtnArray;
        private static LevelSelectController _levelSelectControllerInstance;
        private static bool _isReplayBtnInitialized, _leaderboardLoaded;
        private static List<IEnumerator<UnityWebRequestAsyncOperation>> currentLeaderboardCoroutines;
        private static List<GameObject> _loadingStarList;

        private const int PADDING_X = 10;
        private const int PADDING_Y = 2;
        private const int SM_PADDING_Y = 1;
        private const string FULLSCREEN_PANEL_PATH = "MainCanvas/FullScreenPanel/";
        private const float starSpeed = 0.5f;

        /* Leaderboard logic:
         * The main element of a leaderboard is the container,
         * a container has rows, which represents a score. Rows are organized vertically and have a static width and height
         * a row has columns, which represents one or a pair of scoreData. columns are organized horizontally and have dynamic width and height
         * a column has a list of gameobjects which represents the scoreData. gameobjects have static width and height
         */


        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void YoinkLotOfGraphics(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            _levelSelectControllerInstance = __instance;
            _leaderboardLoaded = false;

            Initialize();
            UpdateLeaderboard(___alltrackslist, __instance);
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Update))]
        [HarmonyPostfix]
        static void UpdateLoadingStarAnimation()
        {
            if (!_leaderboardLoaded && _loadingStarList != null && _loadingStarList.Count > 0)
                foreach (GameObject star in _loadingStarList)
                {
                    RectTransform starRect = star.GetComponent<RectTransform>();
                    starRect.Rotate(0, starSpeed * Time.deltaTime * 1000, 0);
                    if (starRect.eulerAngles.y <= 90)
                        star.GetComponent<CanvasGroup>().alpha = 1 - (starRect.eulerAngles.y / 90);
                    else if (starRect.eulerAngles.y >= 270)
                        star.GetComponent<CanvasGroup>().alpha = (starRect.eulerAngles.y - 270) / 90;

                }
        }

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

            string trackRef = ___alltrackslist[__instance.songindex].trackref;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            string songFilePath = Plugin.SongSelect.GetSongFilePath(isCustom, trackRef);
            string tmb = File.ReadAllText(songFilePath, Encoding.UTF8);
            string songHash = isCustom ? Plugin.Instance.CalcFileHash(songFilePath) : Plugin.Instance.CalcSHA256Hash(Encoding.UTF8.GetBytes(tmb));

            if (currentLeaderboardCoroutines.Count != 0)
            {
                currentLeaderboardCoroutines.ForEach(routine => Plugin.Instance.StopCoroutine(routine));
                currentLeaderboardCoroutines.Clear();
            }

            currentLeaderboardCoroutines.Add(TootTallyAPIService.GetHashInDB(songHash, (songHashInDB) =>
            {
                if (songHashInDB != 0)
                {
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
                }
            }));
            Plugin.Instance.StartCoroutine(currentLeaderboardCoroutines.Last());
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static void Initialize()
        {
            currentLeaderboardCoroutines = new List<IEnumerator<UnityWebRequestAsyncOperation>>();

            _diffBar = GameObject.Find(FULLSCREEN_PANEL_PATH + "diff bar").gameObject;
            _leaderBoard = GameObject.Find(FULLSCREEN_PANEL_PATH + "Leaderboard").gameObject;
            _loadingStarList = new List<GameObject>();

            for (int i = 0; i < 5; i++)
            {
                GameObject star = GameObjectFactory.CreateLoadingScreenStar(_leaderBoard.transform, new Vector2(135 + (15 * i), -225), new Vector2(12, 12), i * 72, "LoadingStar" + i);
                star.AddComponent<CanvasGroup>();
                _loadingStarList.Add(star);
            }


            GameObject leaderboardText = _leaderBoard.transform.Find("\"HIGH SCORES\"").gameObject;
            GameObject lbTextHolder = GameObject.Instantiate(leaderboardText, _leaderBoard.transform);

            Font yoinktextfont = lbTextHolder.GetComponent<Text>().font;
            UnityEngine.Object.DestroyImmediate(lbTextHolder.GetComponent<Text>());
            Text myText = lbTextHolder.AddComponent<Text>();
            myText.font = yoinktextfont;

            _leaderboardHeaderPrefab = lbTextHolder.AddComponent<LeaderboardText>();
            _leaderboardHeaderPrefab.ConstructLeaderboardHeaderText(myText, lbTextHolder.GetComponent<RectTransform>());

            GameObject.DestroyImmediate(_leaderBoard.transform.Find(".......").gameObject);
            GameObject.DestroyImmediate(_leaderBoard.transform.Find("\"HIGH SCORES\"").gameObject);

            for (int i = 1; i < 6; i++)
            {
                _leaderBoard.transform.Find("score" + i).gameObject.SetActive(false);
                _leaderBoard.transform.Find(i.ToString()).gameObject.SetActive(false);
            }

            lbTextHolder.SetActive(false);
        }

        public static void ClearLeaderboard()
        {
            GameObject.DestroyImmediate(_leaderBoard.transform.Find("LeaderboardContainer").gameObject);
        }

        public static void RefreshLeaderboard(List<List<string>> scoreDataLists)
        {
            LeaderBoardContainer lbContainer = CreateContainer(_leaderBoard, _diffBar);

            foreach (List<string> dataList in scoreDataLists)
            {
                LeaderBoardRowContainer rowContainer = CreateLeaderboardRow(lbContainer, _diffBar);

                for (int i = 0; i < dataList.Count; i++)
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

                LeaderBoardColumnContainer colContainerReplay = CreateLeaderboardColumn(rowContainer, _diffBar);
                CustomButton replayButton =
                    GameObjectFactory.CreateCustomButton(colContainerReplay.transform, new Vector2(92, 5), new Vector2(14, 14), "►", "ReplayButton", delegate { ReplaySystemJson.replayFileName = "TestUser - Happy Birthday - 1672164571"; _levelSelectControllerInstance.playbtn.onClick?.Invoke(); });

                colContainerReplay.AddGameObjectToList(replayButton.gameObject);
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
            mySlider.maxValue = 1;
            mySlider.onValueChanged.RemoveAllListeners();
            mySlider.onValueChanged.AddListener((float _value) =>
            {
                if (_value < 0)
                    mySlider.value = 0;
                if (_value > 0.9f)
                    mySlider.value = 0.9f;
                mySlider.fillRect.anchoredPosition = new Vector2(0, mySlider.value * mySlider.fillRect.sizeDelta.y);
            });
            GameObject.DestroyImmediate(mySlider.transform.Find("Handle Slide Area/Handle").gameObject);
            lbContainer.AddSliderToContainer(mySlider);
        }

        private class LeaderboardText : MonoBehaviour
        {
            public Text textHolder;
            public RectTransform rectTransform;

            public void ConstructLeaderboardHeaderText(Text text, RectTransform rectTransform)
            {
                textHolder = text;
                this.rectTransform = rectTransform;
            }
        }

        internal class LeaderBoardContainer : MonoBehaviour
        {
            public GameObject leaderboardContainer;
            public RectTransform rectTransform;
            public List<LeaderBoardRowContainer> leaderboardRowList;
            public Slider slider;

            public void ConstructLeaderBoardContainer(GameObject leaderboardContainer, float width, Vector2 anchoredPosition, RectTransform rectTransform)
            {
                leaderboardContainer.name = "LeaderboardContainer";
                this.leaderboardContainer = leaderboardContainer;
                this.rectTransform = rectTransform;
                rectTransform.sizeDelta = new Vector2(width, 0);
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.eulerAngles = Vector3.zero;
                leaderboardRowList = new List<LeaderBoardRowContainer>();

            }

            public void OrganizeRows()
            {
                Vector2 objPos = Vector2.zero;
                foreach (LeaderBoardRowContainer row in leaderboardRowList)
                {
                    row.rectTransform.anchoredPosition = objPos;
                    row.basePosY = objPos.y;
                    objPos.y -= row.rectTransform.sizeDelta.y + PADDING_Y;
                    row.OrganizeColumns();
                }
                leaderboardContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.sizeDelta.x, -objPos.y);

                List<float> columnsWidthList = new List<float>();
                foreach (LeaderBoardRowContainer row in leaderboardRowList)
                {
                    List<float> currentWidthList = row.GetColumnsWidth();

                    if (columnsWidthList.Count == 0)
                        columnsWidthList = currentWidthList;
                    else
                    {
                        for (int i = 0; i < currentWidthList.Count; i++)
                        {
                            if (columnsWidthList[i] < currentWidthList[i])
                                columnsWidthList[i] = currentWidthList[i];
                        }

                    }
                }
                foreach (LeaderBoardRowContainer row in leaderboardRowList)
                {
                    row.SetColumnsWidth(columnsWidthList);
                    row.rectTransform.sizeDelta = new Vector2(columnsWidthList.Sum() + 45, row.rectTransform.sizeDelta.y); //auto width
                }
            }

            public void AddSliderToContainer(Slider slider)
            {
                this.slider = slider;
                slider.onValueChanged.AddListener((float _value) =>
                {
                    leaderboardRowList.ForEach(row =>
                    {
                        RectTransform rect = row.gameObject.GetComponent<RectTransform>();
                        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, row.basePosY + (slider.value * rectTransform.sizeDelta.y));
                        row.GetComponent<CanvasGroup>().alpha = Math.Max(1 - ((rect.anchoredPosition.y + slider.value) / 30), 0);
                    });
                });
            }

            public void AddRowToList(LeaderBoardRowContainer rowContainer)
            {
                leaderboardRowList.Add(rowContainer);
            }

        }

        internal class LeaderBoardRowContainer : MonoBehaviour
        {
            public GameObject leaderboardRow;
            public RectTransform rectTransform;
            public List<LeaderBoardColumnContainer> leaderBoardColumnList;
            public CanvasGroup canvasGroup;
            public float basePosY;

            public void ConstructLeaderBoardRow(GameObject leaderboardRow, Vector2 sizeDelta, Vector2 anchoredPosition, RectTransform rectTransform)
            {
                leaderboardRow.name = "LeaderboardRow";
                this.leaderboardRow = leaderboardRow;
                this.rectTransform = rectTransform;
                basePosY = rectTransform.anchoredPosition.y;
                this.rectTransform.sizeDelta = sizeDelta;
                this.rectTransform.anchoredPosition = anchoredPosition;
                this.rectTransform.eulerAngles = Vector3.zero;
                leaderBoardColumnList = new List<LeaderBoardColumnContainer>();
                canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 1f;
            }

            public void OrganizeColumns()
            {
                Vector2 objPos = Vector2.zero;
                foreach (LeaderBoardColumnContainer column in leaderBoardColumnList)
                {
                    column.rectTransform.anchoredPosition = objPos;
                    objPos.x += column.rectTransform.sizeDelta.x + PADDING_X;
                    column.OrganizeGameObjects();
                }
            }

            public List<float> GetColumnsWidth()
            {
                List<float> widthList = new List<float>();
                leaderBoardColumnList.ForEach(column => widthList.Add(column.GetWidth()));
                return widthList;
            }

            public void SetColumnsWidth(List<float> columnsWidth)
            {
                for (int i = 0; i < leaderBoardColumnList.Count; i++)
                {
                    leaderBoardColumnList[i].SetWidth(columnsWidth[i]);
                }
                OrganizeColumns();
            }

            public void AddColumnToList(LeaderBoardColumnContainer leaderBoardColumnContainer)
            {
                this.leaderBoardColumnList.Add(leaderBoardColumnContainer);
            }

        }

        internal class LeaderBoardColumnContainer : MonoBehaviour
        {
            public GameObject leaderboardColumn;
            public RectTransform rectTransform;
            public List<GameObject> gameObjectList;

            public void ConstructLeaderBoardColumn(GameObject leaderboardColumn, Vector2 sizeDelta, Vector2 anchoredPosition, RectTransform rectTransform)
            {
                leaderboardColumn.name = "LeaderboardColumn";
                this.leaderboardColumn = leaderboardColumn;
                this.rectTransform = rectTransform;
                this.rectTransform.sizeDelta = sizeDelta;
                this.rectTransform.anchoredPosition = anchoredPosition;
                this.rectTransform.eulerAngles = Vector3.zero;
                gameObjectList = new List<GameObject>();
            }


            public void OrganizeGameObjects()
            {
                Vector2 objPos = new Vector2(0, -PADDING_Y);
                foreach (GameObject gameObject in gameObjectList)
                {
                    RectTransform rect = gameObject.GetComponent<RectTransform>();
                    if (gameObject.GetComponent<CustomButton>() == null) //scuffed way of getting replay buttons but whatever
                    {
                        rect.sizeDelta = new Vector2(rect.sizeDelta.x, (this.rectTransform.sizeDelta.y / gameObjectList.Count) - (PADDING_Y * 2));
                        rect.anchoredPosition = objPos;
                    }
                    else
                    {
                        rect.anchoredPosition = new Vector2(0, objPos.y - (rect.sizeDelta.y / 2) + (PADDING_Y * 3));
                    }

                    objPos.y -= rect.sizeDelta.y + PADDING_Y;
                }
            }

            public float GetWidth() => rectTransform.sizeDelta.x;

            public void SetWidth(float width) => rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);

            public void AddGameObjectToList(GameObject gameObject)
            {
                this.gameObjectList.Add(gameObject);
                RectTransform goTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(goTransform.sizeDelta.x > rectTransform.sizeDelta.x ? goTransform.sizeDelta.x : rectTransform.sizeDelta.x,
                                                      goTransform.sizeDelta.y > rectTransform.sizeDelta.y ? goTransform.sizeDelta.y : rectTransform.sizeDelta.y);
            }
        }

        internal static LeaderBoardContainer CreateContainer(GameObject leaderboard, GameObject prefab)
        {
            GameObject leaderboardHolder = GameObject.Instantiate(prefab, leaderboard.transform);
            LeaderBoardContainer lbContainer = leaderboardHolder.AddComponent<LeaderBoardContainer>();
            lbContainer.ConstructLeaderBoardContainer(leaderboardHolder, 260, new Vector2(32, -160), leaderboardHolder.GetComponent<RectTransform>());
            GameObject.DestroyImmediate(leaderboardHolder.GetComponent<Image>());
            return lbContainer;
        }

        internal static LeaderBoardRowContainer CreateLeaderboardRow(LeaderBoardContainer lbContainer, GameObject prefab)
        {

            GameObject leaderboardRowHolder = GameObject.Instantiate(prefab, lbContainer.transform);
            LeaderBoardRowContainer rowContainer = leaderboardRowHolder.gameObject.AddComponent<LeaderBoardRowContainer>();
            rowContainer.ConstructLeaderBoardRow(leaderboardRowHolder.gameObject, new Vector2(200, 22), Vector2.zero, leaderboardRowHolder.GetComponent<RectTransform>());
            lbContainer.AddRowToList(rowContainer);
            //leaderboardRowHolder.GetComponent<Image>().color = Color.red; //debug color

            return rowContainer;
        }

        internal static LeaderBoardColumnContainer CreateLeaderboardColumn(LeaderBoardRowContainer lbRowContainer, GameObject prefab)
        {
            GameObject leaderboardColHolder = GameObject.Instantiate(prefab, lbRowContainer.transform);
            LeaderBoardColumnContainer colContainer = leaderboardColHolder.gameObject.AddComponent<LeaderBoardColumnContainer>();
            colContainer.ConstructLeaderBoardColumn(leaderboardColHolder.gameObject, new Vector2(10, 24), Vector2.zero, leaderboardColHolder.GetComponent<RectTransform>());
            lbRowContainer.AddColumnToList(colContainer);
            //leaderboardColHolder.GetComponent<Image>().color = Color.blue; //debug color
            GameObject.DestroyImmediate(leaderboardColHolder.GetComponent<Image>());
            return colContainer;
        }

        private static LeaderboardText CreateLeaderBoardHeader(Transform canvasTransform, string text, string name)
        {
            LeaderboardText headerText = GameObject.Instantiate(_leaderboardHeaderPrefab, canvasTransform);

            headerText.name = name;
            headerText.textHolder.text = text;
            headerText.textHolder.fontSize = 10;
            headerText.textHolder.horizontalOverflow = HorizontalWrapMode.Overflow;
            headerText.textHolder.verticalOverflow = VerticalWrapMode.Overflow;
            headerText.textHolder.alignment = TextAnchor.MiddleRight;

            headerText.rectTransform.sizeDelta = new Vector2(headerText.textHolder.preferredWidth + PADDING_X, headerText.textHolder.preferredHeight);

            headerText.gameObject.SetActive(true);

            return headerText;
        }


    }
}
