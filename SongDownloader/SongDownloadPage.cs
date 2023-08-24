using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using TootTally.Utils;
using TootTally.Utils.TootTallySettings;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.UIR;
using static TootTally.Utils.APIServices.SerializableClass;

namespace TootTally.SongDownloader
{
    internal class SongDownloadPage : TootTallySettingPage
    {
        private TMP_InputField _inputField;
        private GameObject _searchButton;
        private Toggle _toggleRated, _toggleUnrated;
        internal GameObject songRowPrefab;
        private List<string> _trackRefList;
        public SongDownloadPage() : base("MoreSongs", "More Songs", 20f, new Color(0, 0, 0, 0.1f))
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _trackRefList = new List<string>();

            _inputField = TootTallySettingObjectFactory.CreateInputField(_fullPanel.transform, $"{name}InputField", DEFAULT_OBJECT_SIZE, DEFAULT_FONTSIZE, "Search", false);
            _inputField.onSubmit.AddListener((value) => Search());
            _inputField.GetComponent<RectTransform>().anchoredPosition = new Vector2(1375, 750);

            _searchButton = GameObjectFactory.CreateCustomButton(_fullPanel.transform, new Vector2(-375, -175), DEFAULT_OBJECT_SIZE, "Search", $"{name}SearchButton", Search).gameObject;

            _toggleRated = TootTallySettingObjectFactory.CreateToggle(_fullPanel.transform, $"{name}ToggleRated", new Vector2(200, 60), "Rated", null);
            _toggleRated.GetComponent<RectTransform>().anchoredPosition = new Vector2(-725, -450);
            _toggleRated.onValueChanged.AddListener(value => { if (value) _toggleUnrated.SetIsOnWithoutNotify(!value); });

            _toggleUnrated = TootTallySettingObjectFactory.CreateToggle(_fullPanel.transform, $"{name}ToggleUnrated", new Vector2(200, 60), "Unrated", null);
            _toggleUnrated.GetComponent<RectTransform>().anchoredPosition = new Vector2(-725, -550);
            _toggleUnrated.onValueChanged.AddListener(value => { if (value) _toggleRated.SetIsOnWithoutNotify(!value); });

            SetSongRowPrefab();
        }

        private void Search()
        {
            var text = _inputField.text;
            RemoveAllObjects();
            _trackRefList.Clear();
            _searchButton.SetActive(false);
            Plugin.Instance.StartCoroutine(TootTallyAPIService.SearchSongWithFilters(text, _toggleRated.isOn, _toggleUnrated.isOn, songList =>
            {
                _searchButton.SetActive(true);
                songList?.OrderByDescending(x => x.id).ToList().ForEach(AddSongToPage);
            }));
        }

        private void AddSongToPage(SongDataFromDB song)
        {
            if (_trackRefList.Contains(song.track_ref)) return;
            _trackRefList.Add(song.track_ref);
            AddSettingObjectToList(new SongDownloadObject(gridPanel.transform, song, this));
        }

        public void SetSongRowPrefab()
        {
            var tempRow = GameObjectFactory.CreateOverlayPanel(_fullPanel.transform, Vector2.zero, new Vector2(1000, 140), 5f, $"TwitchRequestRowTemp").transform.Find("FSLatencyPanel").gameObject;
            songRowPrefab = GameObject.Instantiate(tempRow);
            GameObject.DestroyImmediate(tempRow.gameObject);

            songRowPrefab.name = "RequestRowPrefab";
            songRowPrefab.transform.localScale = Vector3.one;
            songRowPrefab.GetComponent<Image>().maskable = true;
            songRowPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(1020, 160);

            var container = songRowPrefab.transform.Find("LatencyFG/MainPage").gameObject;
            container.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            container.GetComponent<RectTransform>().sizeDelta = new Vector2(1020, 160);

            GameObject.DestroyImmediate(container.transform.parent.Find("subtitle").gameObject);
            GameObject.DestroyImmediate(container.transform.parent.Find("title").gameObject);
            GameObject.DestroyImmediate(container.GetComponent<VerticalLayoutGroup>());
            var horizontalLayoutGroup = container.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.padding = new RectOffset(20, 20, 20, 20);
            horizontalLayoutGroup.spacing = 40f;
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
            horizontalLayoutGroup.childControlHeight = horizontalLayoutGroup.childControlWidth = false;
            horizontalLayoutGroup.childForceExpandHeight = horizontalLayoutGroup.childForceExpandWidth = false;
            songRowPrefab.transform.Find("LatencyFG").GetComponent<Image>().maskable = true;
            songRowPrefab.transform.Find("LatencyBG").GetComponent<Image>().maskable = true;

            GameObject.DontDestroyOnLoad(songRowPrefab);
            songRowPrefab.SetActive(false);
        }
    }
}
