using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using TrombLoader.Helpers;
using UnityEngine;

namespace TootTally.Utils.Helpers
{
    public static class SongDataHelper
    {

        public static string CalcSHA256Hash(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string ret = "";
                byte[] hashArray = sha256.ComputeHash(data);
                foreach (byte b in hashArray)
                {
                    ret += $"{b:x2}";
                }
                return ret;
            }
        }

        public static string CalcFileHash(string fileLocation)
        {
            if (!File.Exists(fileLocation))
                return "";
            return CalcSHA256Hash(File.ReadAllBytes(fileLocation));
        }

        public static string GetSongFilePath(string trackRef)
        {
            return Globals.IsCustomTrack(trackRef) ?
                Path.Combine(Globals.ChartFolders[trackRef], "song.tmb") :
                $"{Application.streamingAssetsPath}/leveldata/{trackRef}.tmb";
        }

        public static string GetChoosenSongHash()
        {
            string trackRef = GlobalVariables.chosen_track_data.trackref;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            return isCustom ? GetSongHash(trackRef) : trackRef;
        }
        public static string GetSongHash(string trackref) => SongDataHelper.CalcFileHash(GetSongFilePath(trackref));

        public static string GenerateBaseTmb(string songFilePath, SingleTrackData singleTrackData = null)
        {
            if (singleTrackData == null) singleTrackData = GlobalVariables.chosen_track_data;

            var tmb = new SerializableClass.TMBData
            {
                name = singleTrackData.trackname_long,
                shortName = singleTrackData.trackname_short,
                trackRef = singleTrackData.trackref,
                author = singleTrackData.artist,
                genre = singleTrackData.genre,
                description = singleTrackData.desc,
                difficulty = singleTrackData.difficulty
            };

            int.TryParse(new string(singleTrackData.year.Where(char.IsDigit).ToArray()), out tmb.year);

            using (FileStream fileStream = File.Open(songFilePath, FileMode.Open))
            {
                var binaryFormatter = new BinaryFormatter();
                var savedLevel = (SavedLevel)binaryFormatter.Deserialize(fileStream);
                var levelData = new List<float[]>();
                savedLevel.savedleveldata.ForEach(arr =>
                {
                    var noteData = new List<float>();
                    foreach (var note in arr) noteData.Add(note);
                    levelData.Add(noteData.ToArray());
                });
                tmb.savednotespacing = savedLevel.savednotespacing;
                tmb.endpoint = savedLevel.endpoint;
                tmb.timesig = savedLevel.timesig;
                tmb.tempo = savedLevel.tempo;
                tmb.notes = levelData;
            }
            return JsonConvert.SerializeObject(tmb);
        }
    }
}
