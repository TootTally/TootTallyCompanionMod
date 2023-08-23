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
using static TootTally.Utils.APIServices.SerializableClass;

namespace TootTally.SongDownloader
{
    internal class SongDownloadPage : TootTallySettingPage
    {
        private TMP_InputField _inputField;
        private GameObject _searchButton;
        public SongDownloadPage() : base("MoreSongs", "More Songs", 20f, new Color(0, 0, 0, 0.1f))
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _inputField = TootTallySettingObjectFactory.CreateInputField(_fullPanel.transform, $"{name}InputField", DEFAULT_OBJECT_SIZE, DEFAULT_FONTSIZE, "Search", false);
            _inputField.GetComponent<RectTransform>().anchoredPosition = new Vector2(750, 850);
            _searchButton = GameObjectFactory.CreateCustomButton(_fullPanel.transform, new Vector2(-600, -200), DEFAULT_OBJECT_SIZE, "Search", $"{name}SearchButton", Search).gameObject;
        }

        private void Search()
        {
            var text = _inputField.text;
            RemoveAllObjects();
            _searchButton.SetActive(false);
            Plugin.Instance.StartCoroutine(TootTallyAPIService.SearchSongBySongName(text, songList =>
            {
                _searchButton.SetActive(true);
                songList?.ForEach(AddSongToPage);
            }));
        }

        private void AddSongToPage(SongDataFromDB song)
        {
            //toString("HH:mm:ss") wasn't working, need to find a better way
            var time = TimeSpan.FromSeconds(song.song_length);
            var stringTime = $"{(time.Hours != 0 ? (time.Hours+":") : "")}{(time.Minutes != 0 ? time.Minutes : "00")}:{(time.Seconds != 0 ? time.Seconds : "00")}";
            AddButton($"{song.short_name}Button", new Vector2(400, 150), $"{song.name}\n<size=16>Mapped by {song.charter}\nDuration: {stringTime}</size>", () => Application.OpenURL($"https://toottally.com/song/{song.id}/"));
        }
    }
}
