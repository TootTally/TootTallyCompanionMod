using BaboonAPI.Hooks.Tracks;
using Microsoft.FSharp.Core;
using Mono.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTally.Graphics;
using TootTally.Utils;
using TootTally.Utils.TootTallySettings;
using UnityEngine;
using UnityEngine.Networking.Match;
using static TootTally.Utils.APIServices.SerializableClass;

namespace TootTally.SongDownloader
{
    internal class SongDownloadObject : BaseTootTallySettingObject
    {
        private GameObject _songRowContainer;
        private GameObject _songRow;
        private SongDataFromDB _song;
        public SongDownloadObject(Transform canvasTransform, SongDataFromDB song, SongDownloadPage page) : base($"Song{song.track_ref}", page)
        {
            _song = song;
            _songRow = GameObject.Instantiate(page.songRowPrefab, canvasTransform);
            _songRow.name = $"Song{song.track_ref}";
            _songRowContainer = _songRow.transform.Find("LatencyFG/MainPage").gameObject;

            var time = TimeSpan.FromSeconds(song.song_length);
            var stringTime = $"{(time.Hours != 0 ? (time.Hours + ":") : "")}{(time.Minutes != 0 ? time.Minutes : "0")}:{(time.Seconds != 0 ? time.Seconds : "00"):00}";

            var t1 = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "SongName", song.name, GameTheme.themeColors.leaderboard.text);
            var t2 = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "Charter", "Mapped by " + song.charter ?? "Unknown", GameTheme.themeColors.leaderboard.text);
            var t3 = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "Duration", stringTime, GameTheme.themeColors.leaderboard.text);
            //fuck that shit :skull:
            t1.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 128);
            t2.GetComponent<RectTransform>().sizeDelta = t3.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 128);
            t1.overflowMode = t2.overflowMode = t3.overflowMode = TMPro.TextOverflowModes.Ellipsis;

            if (FSharpOption<TromboneTrack>.get_IsNone(TrackLookup.tryLookup(song.track_ref)))
                GameObjectFactory.CreateCustomButton(_songRowContainer.transform, Vector2.zero, new Vector2(64, 64), AssetManager.GetSprite("Download64.png"), "DownloadButton", DownloadChart);
            else
            {
                var t4 = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "Owned", "Owned", GameTheme.themeColors.leaderboard.text);
                t4.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 128);
                t4.overflowMode = TMPro.TextOverflowModes.Overflow;
                t4.enableWordWrapping = false;
            }

            GameObjectFactory.CreateCustomButton(_songRowContainer.transform, Vector2.zero, new Vector2(64, 64), AssetManager.GetSprite("global64.png"), "OpenWebButton", () => Application.OpenURL($"https://toottally.com/song/{song.id}/"));

            _songRow.SetActive(true);
        }

        public override void Dispose()
        {
            GameObject.DestroyImmediate(_songRow);
        }

        public void DownloadChart()
        {

        }
    }
}
