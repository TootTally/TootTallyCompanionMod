using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TootTally.Graphics;
using TootTally.Utils;
using TootTally.Utils.TootTallySettings;
using UnityEngine;
using UnityEngine.UI;
using static TootTally.Utils.APIServices.SerializableClass;

namespace TootTally.SongDownloader
{
    internal class SongDownloadPage : TootTallySettingPage
    {
        private const string DEFAULT_INPUT_TEXT = "SearchHere";
        private TMP_InputField _inputField;
        private GameObject _searchButton;
        private GameObject _nextButton, _prevButton;
        private GameObject _downloadAllButton;
        private Toggle _toggleRated, _toggleUnrated;
        private LoadingIcon _loadingIcon;
        internal GameObject songRowPrefab;
        private List<string> _trackRefList;
        private List<string> _newDownloadedTrackRefs;
        private List<SongDownloadObject> _downloadObjectList;


        public SongDownloadPage() : base("MoreSongs", "More Songs", 20f, new Color(0, 0, 0, 0.1f))
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _trackRefList = new List<string>();
            _newDownloadedTrackRefs = new List<string>();
            _downloadObjectList = new List<SongDownloadObject>();

            _inputField = TootTallySettingObjectFactory.CreateInputField(_fullPanel.transform, $"{name}InputField", DEFAULT_OBJECT_SIZE, DEFAULT_FONTSIZE, DEFAULT_INPUT_TEXT, false);
            _inputField.onSubmit.AddListener((value) => Search(_inputField.text));
            _inputField.GetComponent<RectTransform>().anchoredPosition = new Vector2(1375, 750);

            _loadingIcon = GameObjectFactory.CreateLoadingIcon(_fullPanel.transform, new Vector2(-300, -75), new Vector2(128, 128), AssetManager.GetSprite("icon.png"), false, "SongSearchLoadingSwirly");

            _searchButton = GameObjectFactory.CreateCustomButton(_fullPanel.transform, new Vector2(-375, -175), DEFAULT_OBJECT_SIZE, "Search", $"{name}SearchButton", () => Search(_inputField.text)).gameObject;

            _toggleRated = TootTallySettingObjectFactory.CreateToggle(_fullPanel.transform, $"{name}ToggleRated", new Vector2(200, 60), "Rated", null);
            _toggleRated.GetComponent<RectTransform>().anchoredPosition = new Vector2(-725, -450);
            _toggleRated.onValueChanged.AddListener(value => { if (value) _toggleUnrated.SetIsOnWithoutNotify(!value); });

            _toggleUnrated = TootTallySettingObjectFactory.CreateToggle(_fullPanel.transform, $"{name}ToggleUnrated", new Vector2(200, 60), "Unrated", null);
            _toggleUnrated.GetComponent<RectTransform>().anchoredPosition = new Vector2(-725, -550);
            _toggleUnrated.onValueChanged.AddListener(value => { if (value) _toggleRated.SetIsOnWithoutNotify(!value); });

            _downloadAllButton = GameObjectFactory.CreateCustomButton(_fullPanel.transform, new Vector2(-1330, -87), new Vector2(200, 60), "Download All", "DownloadAllButton", DownloadAll).gameObject;
            _downloadAllButton.SetActive(false);

            SetSongRowPrefab();
            _backButton.button.onClick.AddListener(() =>
            {
                if (_newDownloadedTrackRefs.Count > 0)
                {
                    PopUpNotifManager.DisplayNotif("New tracks detected, Reloading songs...\nLagging is normal.");
                    _newDownloadedTrackRefs.Clear();
                    Plugin.Instance.Invoke("ReloadTracks", 0.35f);
                }
            });
            _scrollableSliderHandler.accelerationMult = 0.09f;
        }

        internal override void OnShow()
        {
            _loadingIcon.StartRecursiveAnimation();
        }

        internal override void OnHide()
        {
            _loadingIcon.StopRecursiveAnimation(true);
        }


        private void Search(string input, bool searchWithFilter = true)
        {
            if (input == DEFAULT_INPUT_TEXT)
                input = "";
            _downloadObjectList.Clear();
            _downloadAllButton.SetActive(false);
            RemoveAllObjects();
            _searchButton.SetActive(false);
            _loadingIcon.Show();
            if (_nextButton != null)
            {
                GameObject.DestroyImmediate(_nextButton);
                _nextButton = null;
            }
            if (_prevButton != null)
            {
                GameObject.DestroyImmediate(_prevButton);
                _prevButton = null;
            }

            _trackRefList.Clear();
            if (searchWithFilter)
                Plugin.Instance.StartCoroutine(TootTallyAPIService.SearchSongWithFilters(input, _toggleRated.isOn, _toggleUnrated.isOn, OnSearchInfoRecieved));
            else
                Plugin.Instance.StartCoroutine(TootTallyAPIService.SearchSongByURL(input, OnSearchInfoRecieved));

        }

        private void OnSearchInfoRecieved(SongInfoFromDB searchInfo)
        {
            _searchButton.SetActive(true);
            _loadingIcon.Hide();
            _verticalSlider.value = 0;
            searchInfo.results.OrderByDescending(x => x.id).ToList()?.ForEach(AddSongToPage);
            if (searchInfo.next != null)
                _nextButton = GameObjectFactory.CreateCustomButton(_fullPanel.transform, new Vector2(-350, -175), new Vector2(50, 50), ">>", $"{name}NextButton", () => Search(searchInfo.next, false)).gameObject;
            if (searchInfo.previous != null)
                _prevButton = GameObjectFactory.CreateCustomButton(_fullPanel.transform, new Vector2(-700, -175), new Vector2(50, 50), "<<", $"{name}PrevButton", () => Search(searchInfo.previous, false)).gameObject;
        }

        public void UpdateDownloadAllButton()
        {
            if (!_downloadAllButton.activeSelf)
                _downloadAllButton.SetActive(_downloadObjectList.Any(o => o.isDownloadAvailable));
        }

        private void DownloadAll()
        {
            _downloadAllButton.SetActive(false);
            _downloadObjectList.Where(o => o.isDownloadAvailable).Do(o => o.DownloadChart());
        }

        private void AddSongToPage(SongDataFromDB song)
        {
            if (_trackRefList.Contains(song.track_ref)) return;
            _trackRefList.Add(song.track_ref);
            var songDownloadObj = new SongDownloadObject(gridPanel.transform, song, this);
            _downloadObjectList.Add(songDownloadObj);
            AddSettingObjectToList(songDownloadObj);
        }

        public void SetSongRowPrefab()
        {
            var tempRow = GameObjectFactory.CreateOverlayPanel(_fullPanel.transform, Vector2.zero, new Vector2(1030, 140), 5f, $"TwitchRequestRowTemp").transform.Find("FSLatencyPanel").gameObject;
            songRowPrefab = GameObject.Instantiate(tempRow);
            GameObject.DestroyImmediate(tempRow.gameObject);

            songRowPrefab.name = "RequestRowPrefab";
            songRowPrefab.transform.localScale = Vector3.one;
            songRowPrefab.GetComponent<Image>().maskable = true;
            songRowPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(1050, 160);

            var container = songRowPrefab.transform.Find("LatencyFG/MainPage").gameObject;
            container.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            container.GetComponent<RectTransform>().sizeDelta = new Vector2(1050, 160);

            GameObject.DestroyImmediate(container.transform.parent.Find("subtitle").gameObject);
            GameObject.DestroyImmediate(container.transform.parent.Find("title").gameObject);
            GameObject.DestroyImmediate(container.GetComponent<VerticalLayoutGroup>());
            var horizontalLayoutGroup = container.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.padding = new RectOffset(20, 20, 20, 20);
            horizontalLayoutGroup.spacing = 30f;
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
            horizontalLayoutGroup.childControlHeight = horizontalLayoutGroup.childControlWidth = false;
            horizontalLayoutGroup.childForceExpandHeight = horizontalLayoutGroup.childForceExpandWidth = false;
            songRowPrefab.transform.Find("LatencyFG").GetComponent<Image>().maskable = true;
            songRowPrefab.transform.Find("LatencyBG").GetComponent<Image>().maskable = true;

            GameObject.DontDestroyOnLoad(songRowPrefab);
            songRowPrefab.SetActive(false);
        }

        public void AddTrackRefToDownloadedSong(string trackref) => _newDownloadedTrackRefs.Add(trackref);
        public bool IsAlreadyDownloaded(string trackref) => _newDownloadedTrackRefs.Contains(trackref);
    }
}
