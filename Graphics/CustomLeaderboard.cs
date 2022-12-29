using HarmonyLib;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private static List<ScoreData> _scoreDataList;
        private static GameObject _leaderBoard;
        private static List<List<string>> fakeDataMatrix, realDataMatrix;
        private static LeaderboardHeaderText _leaderboardHeaderPrefab;
        private static Slider _scrollSlider;
        private const int PADDING_X = 5;
        private const int PADDING_Y = 2;
        private const int SM_PADDING_Y = 1;
        private const string FULLSCREEN_PANEL_PATH = "";



        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void YoinkLotOfGraphics(LevelSelectController __instance)
        {

            Plugin.Instance.StartCoroutine(GetScoresFromDB(182));

        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPostfix]
        static void UpdateLeaderboard(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {

        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetScoresFromDB(int songID)
        {
            string apiLink = $"{Plugin.APIURL}/api/songs/{songID}/leaderboard/";

            UnityWebRequest webRequest = UnityWebRequest.Get(apiLink);

            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
                Plugin.LogError($"ERROR IN LOADING LEADERBOARD: {webRequest.error}");
            else if (webRequest.isHttpError)
                Plugin.LogError($"HTTP ERROR {webRequest.error}");
            else
                Plugin.LogInfo($"LEADERBOARD SCORES LOADED!");

            List<List<string>> scoresMatrix = new List<List<string>>();

            var leaderboardJson = JSONObject.Parse(webRequest.downloadHandler.GetText());
            int count = 1;
            foreach (JSONObject scoreJson in leaderboardJson["results"])
            {
                List<string> scoreData = new List<string>
                {
                    "#" + count,
                    Truncate(scoreJson["player"], 8),
                    string.Format("{0:n0}",int.Parse(scoreJson["score"])),
                    float.Parse(scoreJson["percentage"]).ToString("0.00") + "%",
                    scoreJson["grade"],
                    scoreJson["max_combo"] + "x",
                };
                scoresMatrix.Add(scoreData);
                count++;
            }

            fakeDataMatrix = scoresMatrix;
            Initialize();
        }
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }


        public class LeaderboardVerticalSlider
        {
            public Slider slider;
            public Vector2 sizeDelta;
            public RectTransform rectTransform;

            public void ConstructVerticalSlider(Slider slider, Vector2 sizeDelta, Vector2 anchoredPosition, RectTransform rectTransform)
            {
                slider.name = "LeaderboardVerticalSlider";
                this.slider = slider;
                this.rectTransform = rectTransform;
                rectTransform.sizeDelta = sizeDelta;
                rectTransform.anchoredPosition = anchoredPosition;
            }
        }

        /*public LeaderboardVerticalSlider CreateLeaderboardVerticalSlider(Transform canvasTransform, Slider prefab)
        {
            Slider mySlider = GameObject.Instantiate(prefab, canvasTransform);
            mySlider.direction = Slider.Direction.TopToBottom;
            RectTransform sliderRect = mySlider.GetComponent<RectTransform>();
            RectTransform sliderPrefabRect = prefab.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(sliderPrefabRect.sizeDelta.y, sliderPrefabRect.sizeDelta.x * 2.8f);
            sliderRect.anchoredPosition = new Vector2(156, -190);
            mySlider.handleRect = sliderRect;
            RectTransform backgroundSliderRect = mySlider.transform.Find("Background").GetComponent<RectTransform>();
            backgroundSliderRect.anchoredPosition = new Vector2(-5, backgroundSliderRect.anchoredPosition.y);
            backgroundSliderRect.sizeDelta = new Vector2(-10, backgroundSliderRect.sizeDelta.y);
            mySlider.minValue = 0;
            mySlider.maxValue = 1;
            //mySlider.onValueChanged.AddListener((float _value) => { mySlider.fillRect.sizeDelta = new Vector2(mySlider.fillRect.sizeDelta.x, 5); }); //not working for some reasons
            GameObject.DestroyImmediate(mySlider.transform.Find("Handle Slide Area/Handle").gameObject);
            return mySlider;
        }*/

        public static void Initialize()
        {
            _scoreDataList = new List<ScoreData>();

            GameObject diffBar = GameObject.Find(FULLSCREEN_PANEL_PATH + "diff bar").gameObject;

            _leaderBoard = GameObject.Find(FULLSCREEN_PANEL_PATH + "Leaderboard").gameObject;
            LeaderBoardContainer lbContainer = CreateContainer(_leaderBoard, diffBar);

            GameObject leaderboardHeader = _leaderBoard.transform.Find("\"HIGH SCORES\"").gameObject;
            GameObject headerHolder = GameObject.Instantiate(leaderboardHeader, _leaderBoard.transform);

            Font yoinktextfont = headerHolder.GetComponent<Text>().font;
            UnityEngine.Object.DestroyImmediate(headerHolder.GetComponent<Text>());
            Text myText = headerHolder.AddComponent<Text>();
            myText.font = yoinktextfont;

            _leaderboardHeaderPrefab = headerHolder.AddComponent<LeaderboardHeaderText>();
            _leaderboardHeaderPrefab.ConstructLeaderboardHeaderText(myText, headerHolder.GetComponent<RectTransform>());
            headerHolder.SetActive(false);

            foreach (List<string> dataList in fakeDataMatrix)
            {
                LeaderBoardRowContainer rowContainer = CreateLeaderboardRow(lbContainer, diffBar);

                for (int i = 0; i < dataList.Count; i++)
                {
                    LeaderBoardColumnContainer colContainer = CreateLeaderboardColumn(rowContainer, diffBar);

                    LeaderboardHeaderText header = CreateLeaderBoardHeader(colContainer.transform, dataList[i], "LeaderboardHeader" + dataList[i]);
                    colContainer.AddGameObjectToList(header.gameObject);

                    if (i == 1 || i == 4)
                    {
                        i++;

                        LeaderboardHeaderText header2 = CreateLeaderBoardHeader(colContainer.transform, dataList[i], "LeaderboardHeader" + dataList[i]);
                        colContainer.AddGameObjectToList(header2.gameObject);
                    }

                }
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

        private class ScoreData
        {
            public int rank, name, score, maxCombo;
            public float percentage;
            public Grade grade;

            public enum Grade
            {
                S,
                A,
                B,
                C,
                D,
                E,
            }
        }

        private class LeaderboardHeaderText : MonoBehaviour
        {
            public Text textHolder;
            public RectTransform rectTransform;

            public void ConstructLeaderboardHeaderText(Text text, RectTransform rectTransform)
            {
                textHolder = text;
                this.rectTransform = rectTransform;
            }
        }


        private class LeaderboardText : MonoBehaviour
        {
            public Text text;
            public Transform rectTransform;

            public void ConstructLeaderboardText(Text text, Transform rectTransform)
            {
                this.text = text;
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

        private static LeaderboardHeaderText CreateLeaderBoardHeader(Transform canvasTransform, string text, string name)
        {
            LeaderboardHeaderText headerText = GameObject.Instantiate(_leaderboardHeaderPrefab, canvasTransform);

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
