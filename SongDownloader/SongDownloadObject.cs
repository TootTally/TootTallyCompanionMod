using BaboonAPI.Hooks.Tracks;
using BepInEx;
using Microsoft.FSharp.Core;
using System;
using System.IO;
using TMPro;
using TootTally.Graphics;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using TootTally.Utils.TootTallySettings;
using UnityEngine;
using static TootTally.Utils.APIServices.SerializableClass;

namespace TootTally.SongDownloader
{
    internal class SongDownloadObject : BaseTootTallySettingObject
    {
        private const string _DOWNLOAD_MIRROR_LINK = "https://sgp1.digitaloceanspaces.com/toottally/chartmirrors/";
        private const string _DOWNLOAD_DOWNLOAD_LINK = "https://cdn.discordapp.com/";
        private GameObject _songRowContainer;
        private GameObject _songRow;
        private SongDataFromDB _song;
        private GameObject _downloadButton;
        private ProgressBar _progressBar;
        private TMP_Text _fileSizeText;
        private TMP_Text _durationText;

        public SongDownloadObject(Transform canvasTransform, SongDataFromDB song, SongDownloadPage page) : base($"Song{song.track_ref}", page)
        {
            _song = song;
            _songRow = GameObject.Instantiate(page.songRowPrefab, canvasTransform);
            _songRow.name = $"Song{song.track_ref}";
            _songRowContainer = _songRow.transform.Find("LatencyFG/MainPage").gameObject;

            var time = TimeSpan.FromSeconds(song.song_length);
            var stringTime = $"{(time.Hours != 0 ? (time.Hours + ":") : "")}{(time.Minutes != 0 ? time.Minutes : "0")}:{(time.Seconds != 0 ? time.Seconds : "00"):00}";

            var songNameText = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "SongName", song.name, GameTheme.themeColors.leaderboard.text);
            var charterText = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "Charter", song.charter != null ? $"Mapped by {song.charter}" : "Unknown", GameTheme.themeColors.leaderboard.text);
            _durationText = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "Duration", stringTime, GameTheme.themeColors.leaderboard.text);
            _fileSizeText = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "FileSize", "", GameTheme.themeColors.leaderboard.text);
            _fileSizeText.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 128);
            _fileSizeText.gameObject.SetActive(false);
            //fuck that shit :skull:
            songNameText.GetComponent<RectTransform>().sizeDelta = charterText.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 128);
            _durationText.GetComponent<RectTransform>().sizeDelta = new Vector2(230, 128);
            songNameText.overflowMode = charterText.overflowMode = _durationText.overflowMode = TMPro.TextOverflowModes.Ellipsis;

            //lol
            if (FSharpOption<TromboneTrack>.get_IsNone(TrackLookup.tryLookup(song.track_ref)) && !(_page as SongDownloadPage).IsAlreadyDownloaded(song.track_ref))
            {
                string link = "";
                if (song.mirror != null && Path.GetExtension(song.mirror).Contains(".zip"))
                    link = song.mirror;
                else if (song.download != null && song.download.Contains(_DOWNLOAD_DOWNLOAD_LINK) && Path.GetExtension(song.download).Contains(".zip"))
                    link = song.download;

                if (link != "")
                {
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetFileSize(link, size =>
                    {
                        var stringSize = FileHelper.SizeSuffix(size, 2);
                        _fileSizeText.text = stringSize;
                        _fileSizeText.gameObject.SetActive(true);
                        _durationText.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 128);
                    }));
                    _downloadButton = GameObjectFactory.CreateCustomButton(_songRowContainer.transform, Vector2.zero, new Vector2(64, 64), AssetManager.GetSprite("Download64.png"), "DownloadButton", DownloadChart).gameObject;
                    _progressBar = GameObjectFactory.CreateProgressBar(_songRow.transform.Find("LatencyFG"), Vector2.zero, new Vector2(900, 20), false, "ProgressBar");
                }
                else
                {
                    var notAvailableText = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "N/A", "N/A", GameTheme.themeColors.leaderboard.text);
                    notAvailableText.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 128);
                    notAvailableText.overflowMode = TMPro.TextOverflowModes.Overflow;
                    notAvailableText.enableWordWrapping = false;
                }
            }
            else
            {
                var ownedText = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "Owned", "Owned", GameTheme.themeColors.leaderboard.text);
                ownedText.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 128);
                ownedText.overflowMode = TMPro.TextOverflowModes.Overflow;
                ownedText.enableWordWrapping = false;
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
            _downloadButton.SetActive(false);
            _fileSizeText.gameObject.SetActive(false);
            string link = _song.mirror ?? _song.download;
            Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadZipFromServer(link, _progressBar, data =>
            {
                if (data != null)
                {
                    string downloadDir = Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location), "Downloads/");
                    string fileName = $"{_song.id}.zip";
                    FileHelper.WriteBytesToFile(downloadDir, fileName, data);

                    string source = Path.Combine(downloadDir, fileName);
                    string destination = Path.Combine(Paths.BepInExRootPath, "CustomSongs/");
                    FileHelper.ExtractZipToDirectory(source, destination);

                    FileHelper.DeleteFile(downloadDir, fileName);

                    var t4 = GameObjectFactory.CreateSingleText(_songRowContainer.transform, "Owned", "Owned", GameTheme.themeColors.leaderboard.text);
                    t4.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 128);
                    t4.overflowMode = TMPro.TextOverflowModes.Overflow;
                    t4.enableWordWrapping = false;
                    t4.transform.SetSiblingIndex(3);
                    _durationText.GetComponent<RectTransform>().sizeDelta = new Vector2(230, 128);
                    (_page as SongDownloadPage).AddTrackRefToDownloadedSong(_song.track_ref);
                }
                else
                {
                    PopUpNotifManager.DisplayNotif("Download failed.");
                    _downloadButton.SetActive(true);
                }
                
            }));
        }
    }
}
