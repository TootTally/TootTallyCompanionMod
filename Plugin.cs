﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using TrombLoader.Helpers;

namespace TootTally
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("AutoToot", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("org.crispykevin.hovertoot", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("TrombSettings", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("TrombLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static void LogDebug(string msg) => Instance.Logger.LogDebug(msg);
        internal static void LogInfo(string msg) => Instance.Logger.LogInfo(msg);
        internal static void LogError(string msg) => Instance.Logger.LogError(msg);
        internal static void LogWarning(string msg) => Instance.Logger.LogWarning(msg);
        public static Plugin Instance;
        private Dictionary<string, string> plugins = new();
        public const string APIURL = "http://localhost";
        public ConfigEntry<string> APIKey { get; private set; }
        public ConfigEntry<bool> AllowTMBUploads { get; private set; }

        private string CalcSHA256Hash(byte[] data)
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

        public void Log(string msg)
        {
            LogInfo(msg);
        }

        public string CalcFileHash(string fileLocation)
        {
            if (!File.Exists(fileLocation))
                return "";
            return CalcSHA256Hash(File.ReadAllBytes(fileLocation));
        }
        
        private void Awake()
        {
            if (Instance != null) return; // Make sure that this is a singleton (even though it's highly unlikely for duplicates to happen)
            Instance = this;
            
            // Config
            APIKey = Config.Bind("API Setup", "API Key", "SignUpOnTootTally.com", "API Key for Score Submissions");
            AllowTMBUploads = Config.Bind("API Setup", "Allow Unknown Song Uploads", false, "Should this mod send unregistered charts to the TootTally server?");
            object settings = OptionalTrombSettings.GetConfigPage("TootTally");
            if (settings != null)
            {
                OptionalTrombSettings.Add(settings, AllowTMBUploads);
                OptionalTrombSettings.Add(settings, APIKey);
            }

            // Read every plugin being loaded by BepInEx and hash it
            // foreach (KeyValuePair<string, BepInEx.PluginInfo> plugin in Chainloader.PluginInfos)
            // {
            //     LogInfo($"PLUGIN: {plugin.Key} | HASH: {CalcFileHash(plugin.Value.Location)}");
            // }

            Harmony.CreateAndPatchAll(typeof(SongSelect));
            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [Serializable]
        public class ChartSubmission
        {
            public string tmb;
            public bool is_official;

            public static ChartSubmission GenerateChartSubmission(bool isCustom, string songFilePath)
            {
                ChartSubmission chart = new();
                if (isCustom)
                {
                    chart.tmb = File.ReadAllText(songFilePath, System.Text.Encoding.UTF8);
                }
                else
                {
                    var singleTrackData = GlobalVariables.chosen_track_data;
                    var tmb = new JSONObject();
                    tmb["name"] = singleTrackData.trackname_long;
                    tmb["shortName"] = singleTrackData.trackname_short;
                    tmb["trackRef"] = singleTrackData.trackref;
                    tmb["year"] = int.Parse(singleTrackData.year);
                    tmb["author"] = singleTrackData.artist;
                    tmb["genre"] = singleTrackData.genre;
                    tmb["description"] = singleTrackData.desc;
                    tmb["difficulty"] = singleTrackData.difficulty;
                    using (FileStream fileStream = File.Open(songFilePath, FileMode.Open))
                    {
                        var binaryFormatter = new BinaryFormatter();
                        var savedLevel = (SavedLevel)binaryFormatter.Deserialize(fileStream);
                        var levelData = new JSONArray();
                        savedLevel.savedleveldata.ForEach(arr =>
                        {
                            var noteData = new JSONArray();
                            foreach (var note in arr) noteData.Add(note);
                            levelData.Add(noteData);
                        });
                        tmb["savednotespacing"] = savedLevel.savednotespacing;
                        tmb["endpoint"] = savedLevel.endpoint;
                        tmb["timesig"] = savedLevel.timesig;
                        tmb["tempo"] = savedLevel.tempo;
                        tmb["notes"] = levelData;
                    }
                    LogDebug(tmb.ToString());
                    chart.tmb = tmb.ToString();
                }
                chart.is_official = !isCustom;
                return chart;
            }
        }

        [Serializable]
        public class ScoreSubmission
        {
            public string apiKey;
            public bool isCustom;
            public string letterScore;
            public int score;
            public int[] noteTally; // [nasties, mehs, okays, nices, perfects]
            public string songHash;
            public int maxCombo;

            public IEnumerator<UnityWebRequestAsyncOperation> SubmitScore()
            {
                apiKey = Instance.APIKey.Value;
                string apiLink = $"{APIURL}/api/submitscore/";
                string jsonified = JsonUtility.ToJson(this);
                LogDebug($"Score JSON: {jsonified}");
                var jsonbin = System.Text.Encoding.UTF8.GetBytes(jsonified);

                DownloadHandler dlHandler = new DownloadHandler();
                UploadHandler ulHandler = new UploadHandlerRaw(jsonbin);
                ulHandler.contentType = "application/json";

                UnityWebRequest www = new UnityWebRequest(apiLink, "POST", dlHandler, ulHandler);
                yield return www.SendWebRequest();

                if (www.isNetworkError)
                    LogError($"ERROR IN SENDING SCORE: {www.error}");
                else if (www.isHttpError)
                    LogError($"HTTP ERROR {www.error}");
                else
                    LogInfo($"SCORE SENT SUCCESFULLY");
            }
        }

        public static class SongSelect
        {
            internal static void LogDebug(string msg) => Instance.Logger.LogDebug(msg);
            internal static void LogInfo(string msg) => Instance.Logger.LogInfo(msg);
            internal static void LogError(string msg) => Instance.Logger.LogError(msg);
            internal static void LogWarning(string msg) => Instance.Logger.LogWarning(msg);
            private static string songHash;
            private static int maxCombo;
            
            public static IEnumerator<UnityWebRequestAsyncOperation> CheckHashInDB(bool isCustom, string songFilePath)
            {
                bool inDatabase = false;
                UnityWebRequest webRequest = UnityWebRequest.Get($"{APIURL}/hashcheck/{songHash}/");
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    LogError("Network error detected, will not attempt anything");
                }
                else if (webRequest.isHttpError)
                {
                    LogInfo("HTTP error returned, assuming not in database");
                }
                else
                {
                    LogInfo("HTTP 200 OK: It's in the database!");
                    inDatabase = true;
                }

                if (Instance.AllowTMBUploads.Value && !inDatabase)
                {
                    var chart = ChartSubmission.GenerateChartSubmission(isCustom, songFilePath);
                    string apiLink = $"{APIURL}/api/upload/";
                    string jsonified = JsonUtility.ToJson(chart);
                    LogDebug($"Chart JSON: {jsonified}");
                    var jsonbin = System.Text.Encoding.UTF8.GetBytes(jsonified);

                    DownloadHandler dlHandler = new DownloadHandler();
                    UploadHandler ulHandler = new UploadHandlerRaw(jsonbin);
                    ulHandler.contentType = "application/json";

                    UnityWebRequest www = new UnityWebRequest(apiLink, "POST", dlHandler, ulHandler);
                    yield return www.SendWebRequest();

                    if (www.isNetworkError)
                        LogError($"ERROR IN SENDING CHART: {www.error}");
                    else if (www.isHttpError)
                        LogError($"HTTP ERROR {www.error}");
                    else
                        LogInfo($"CHART SENT SUCCESFULLY");
                }
            }

            [HarmonyPatch(typeof(LoadController), nameof(LoadController.LoadGameplayAsync))]
            [HarmonyPrefix]
            public static void SetSongHash(LoadController __instance)
            {
                string trackRef = GlobalVariables.chosen_track;
                bool isCustom = Globals.IsCustomTrack(trackRef);
                string songFilePath = GetSongFilePath(isCustom, trackRef);
                LogInfo($"Hashing {songFilePath}");
                songHash = Instance.CalcFileHash(songFilePath);
                LogInfo($"Calculated hash: {songHash}");
                __instance.StartCoroutine(CheckHashInDB(isCustom, songFilePath));
                maxCombo = 0; // Reset tracked maxCombo
            }

            public static string GetSongFilePath(bool isCustom, string trackRef)
            {
                return isCustom
                    ? Path.Combine(Globals.ChartFolders[trackRef], "song.tmb")
                    : $"{Application.streamingAssetsPath}/leveldata/{trackRef}.tmb";
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.updateHighestCombo))]
            [HarmonyPostfix]
            public static void UpdateCombo(GameController __instance)
            {
                maxCombo = __instance.highestcombo_level;
            }

            [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
            [HarmonyPostfix]
            public static void GetPlayInfo(PointSceneController __instance)
            {
                if (AutoTootCompatibility.enabled && AutoTootCompatibility.WasAutoUsed) return; // Don't submit anything if AutoToot was used.
                if (HoverTootCompatibility.enabled && HoverTootCompatibility.DidToggleThisSong) return; // Don't submit anything if HoverToot was used.
                string trackRef = GlobalVariables.chosen_track;
                bool isCustom = Globals.IsCustomTrack(trackRef);
                LogInfo($"TrackRef: {GlobalVariables.chosen_track}");
                LogInfo($"Letter Score: {__instance.letterscore}");
                LogInfo($"Score: {GlobalVariables.gameplay_scoretotal}");
                LogInfo($"Max Combo: {maxCombo}");
                LogInfo($"Nasties: {GlobalVariables.gameplay_notescores[0]}");
                LogInfo($"Mehs: {GlobalVariables.gameplay_notescores[1]}");
                LogInfo($"Okays: {GlobalVariables.gameplay_notescores[2]}");
                LogInfo($"Nices: {GlobalVariables.gameplay_notescores[3]}");
                LogInfo($"Perfects: {GlobalVariables.gameplay_notescores[4]}");
                LogInfo($"Song Hash: {songHash}");

                ScoreSubmission score = new();
                score.isCustom = isCustom;
                score.letterScore = __instance.letterscore;
                score.score = GlobalVariables.gameplay_scoretotal;
                score.noteTally = GlobalVariables.gameplay_notescores;
                score.songHash = songHash;
                score.maxCombo = maxCombo;
                __instance.StartCoroutine(score.SubmitScore());
            }
        }
    }
}
