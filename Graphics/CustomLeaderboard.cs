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
        private static bool _isReplayBtnInitialized;

        private const int PADDING_X = 5;
        private const int PADDING_Y = 2;
        private const int SM_PADDING_Y = 1;
        private const string FULLSCREEN_PANEL_PATH = "MainCanvas/FullScreenPanel/";

        /* Leaderboard logic:
         * The main element of a leaderboard is the container,
         * a container has rows, which represents a score. Rows are organized vertically and have a static width and height
         * a row has columns, which represents one or a pair of scoreData. columns are organized horizontally and have dynamic width and height
         * a column has a list of gameobjects which represents the scoreData. gameobjects have static width and height
         */


        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void YoinkLotOfGraphics(LevelSelectController __instance)
        {
            _levelSelectControllerInstance = __instance;

            Plugin.Instance.StartCoroutine(TootTallyAPIService.GetLeaderboardScoresFromDB(182, (scoreDataList) =>
            {

                List<List<string>> scoresMatrix = new List<List<string>>();

                int count = 1;
                foreach (SerializableSubmissionClass.ScoreDataFromDB scoreData in scoreDataList)
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

                Initialize();
                RefreshLeaderboard(scoresMatrix);
            }));

        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPostfix]
        static void UpdateLeaderboard(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {

        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static void Initialize()
        {
            _diffBar = GameObject.Find(FULLSCREEN_PANEL_PATH + "diff bar").gameObject;
            _leaderBoard = GameObject.Find(FULLSCREEN_PANEL_PATH + "Leaderboard").gameObject;

            GameObject leaderboardText = _leaderBoard.transform.Find("\"HIGH SCORES\"").gameObject;
            GameObject lbTextHolder = GameObject.Instantiate(leaderboardText, _leaderBoard.transform);

            Font yoinktextfont = lbTextHolder.GetComponent<Text>().font;
            UnityEngine.Object.DestroyImmediate(lbTextHolder.GetComponent<Text>());
            Text myText = lbTextHolder.AddComponent<Text>();
            myText.font = yoinktextfont;

            _leaderboardHeaderPrefab = lbTextHolder.AddComponent<LeaderboardText>();
            _leaderboardHeaderPrefab.ConstructLeaderboardHeaderText(myText, lbTextHolder.GetComponent<RectTransform>());

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
                    InteractableGameObjectFactory.CreateCustomButton(colContainerReplay.transform, new Vector2(92, 5), new Vector2(14, 14), "►", "ReplayButton", delegate { ReplaySystemJson.replayFileName = "TestUser - Happy Birthday - 1672164571"; _levelSelectControllerInstance.playbtn.onClick?.Invoke(); });

                colContainerReplay.AddGameObjectToList(replayButton.gameObject);


            }
            lbContainer.OrganizeRows();

            GameObject.DestroyImmediate(_leaderBoard.transform.Find(".......").gameObject);
            GameObject.DestroyImmediate(_leaderBoard.transform.Find("\"HIGH SCORES\"").gameObject);

            for (int i = 1; i < 6; i++)
            {
                _leaderBoard.transform.Find("score" + i).gameObject.SetActive(false);
                _leaderBoard.transform.Find(i.ToString()).gameObject.SetActive(false);
            }

            Slider sliderPrefab = GameObject.Find(FULLSCREEN_PANEL_PATH + "Slider").GetComponent<Slider>(); //yoink
            RectTransform sliderPrefabRect = sliderPrefab.GetComponent<RectTransform>();

            Slider mySlider = GameObject.Instantiate(sliderPrefab, lbContainer.transform);
            mySlider.direction = Slider.Direction.TopToBottom;
            RectTransform sliderRect = mySlider.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(sliderPrefabRect.sizeDelta.y, sliderPrefabRect.sizeDelta.x * 2.8f);
            sliderRect.anchoredPosition = new Vector2(230, 0);
            mySlider.handleRect = sliderRect;
            RectTransform backgroundSliderRect = mySlider.transform.Find("Background").GetComponent<RectTransform>();
            backgroundSliderRect.anchoredPosition = new Vector2(-5, backgroundSliderRect.anchoredPosition.y);
            backgroundSliderRect.sizeDelta = new Vector2(-10, backgroundSliderRect.sizeDelta.y);
            mySlider.minValue = 0;
            mySlider.maxValue = 1;
            GameObject.DestroyImmediate(mySlider.transform.Find("Handle Slide Area/Handle").gameObject);
            mySlider.onValueChanged.AddListener((float _value) =>
            {
                lbContainer.leaderboardRowList.ForEach(row =>
                {
                    RectTransform rect = row.gameObject.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, row.basePosY + (_value * lbContainer.rectTransform.sizeDelta.y));
                    row.GetComponent<CanvasGroup>().alpha = Math.Max(1 - ((rect.anchoredPosition.y + _value) / 30), 0);
                });
            });
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
                }
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
                Vector2 objPos = Vector2.zero;
                foreach (GameObject gameObject in gameObjectList)
                {
                    RectTransform rect = gameObject.GetComponent<RectTransform>();
                    rect.anchoredPosition = objPos;
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, this.rectTransform.sizeDelta.y / gameObjectList.Count);
                    objPos.y -= rect.sizeDelta.y;
                }
            }

            public float GetWidth() => rectTransform.sizeDelta.x;

            public void SetWidth(float width) => rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
            public void SetAlpha(float alpha) => gameObjectList.ForEach(gameObject => gameObject.GetComponent<CanvasRenderer>().SetAlpha(alpha));

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
            rowContainer.ConstructLeaderBoardRow(leaderboardRowHolder.gameObject, new Vector2(260, 30), Vector2.zero, leaderboardRowHolder.GetComponent<RectTransform>());
            lbContainer.AddRowToList(rowContainer);
            //leaderboardRowHolder.GetComponent<Image>().color = Color.red; //debug color

            return rowContainer;
        }

        internal static LeaderBoardColumnContainer CreateLeaderboardColumn(LeaderBoardRowContainer lbRowContainer, GameObject prefab)
        {
            GameObject leaderboardColHolder = GameObject.Instantiate(prefab, lbRowContainer.transform);
            LeaderBoardColumnContainer colContainer = leaderboardColHolder.gameObject.AddComponent<LeaderBoardColumnContainer>();
            colContainer.ConstructLeaderBoardColumn(leaderboardColHolder.gameObject, new Vector2(30, 30), Vector2.zero, leaderboardColHolder.GetComponent<RectTransform>());
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
            headerText.textHolder.fontSize = 14;
            headerText.textHolder.horizontalOverflow = HorizontalWrapMode.Overflow;
            headerText.textHolder.verticalOverflow = VerticalWrapMode.Overflow;
            headerText.textHolder.alignment = TextAnchor.MiddleRight;

            headerText.rectTransform.sizeDelta = new Vector2(headerText.textHolder.preferredWidth + PADDING_X, headerText.textHolder.preferredHeight);

            headerText.gameObject.SetActive(true);

            return headerText;
        }


    }
}
