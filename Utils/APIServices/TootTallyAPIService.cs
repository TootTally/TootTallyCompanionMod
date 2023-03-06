using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using TootTally.Graphics;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.Networking;

namespace TootTally.Utils
{
    public static class TootTallyAPIService
    {
        public const string APIURL = "https://toottally.com";
        //public const string APIURL = "http://localhost"; //localTesting
        public const string REPLAYURL = "http://cdn.toottally.com/replays/";

        public static IEnumerator<UnityWebRequestAsyncOperation> GetHashInDB(string songHash, bool isCustom, Action<int> callback)
        {
            UnityWebRequest webRequest = isCustom ? UnityWebRequest.Get($"{APIURL}/hashcheck/{songHash}/") : UnityWebRequest.Get($"{APIURL}/api/hashcheck/official/?trackref={songHash}");
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
            {
                callback(int.Parse(webRequest.downloadHandler.text)); //.text returns the digit of ex: https://toottally.com/api/songs/182/leaderboard/
            }
            else
                callback(0); //hash 0 is null

        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetUser(Action<SerializableClass.User> callback)
        {
            // TODO: Might have to redo this to follow the same pattern as SubmitScore
            var apiObj = new SerializableClass.APISubmission() { apiKey = Plugin.Instance.APIKey.Value };
            var apiKey = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest($"{APIURL}/api/profile/self/", apiKey);
            SerializableClass.User user;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, false))
            {
                user = JsonConvert.DeserializeObject<SerializableClass.User>(webRequest.downloadHandler.text);
                Plugin.LogInfo($"Welcome, {user.username}!");
            }
            else
            {
                user = new SerializableClass.User()
                {
                    username = "Guest",
                    id = 0,
                };
                Plugin.LogInfo($"Logged in with Guest Account");
            }
            callback(user);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> AddChartInDB(SerializableClass.TMBFile chart, Action callback)
        {

            string apiLink = $"{APIURL}/api/upload/";
            string jsonified = JsonUtility.ToJson(chart);
            var jsonbin = System.Text.Encoding.UTF8.GetBytes(jsonified);

            UnityWebRequest webRequest = PostUploadRequest(apiLink, jsonbin);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
            {
                if (webRequest.downloadHandler.text.Equals("Chart requested to skip"))
                    PopUpNotifManager.DisplayNotif(webRequest.downloadHandler.text, GameTheme.themeColors.notification.warningText);
                else
                {
                    Plugin.LogInfo($"Chart Sent.");
                    PopUpNotifManager.DisplayNotif("New chart sent to TootTally", Color.green);
                }
            }
            else
                PopUpNotifManager.DisplayNotif("Error in sending chart", GameTheme.themeColors.notification.errorText);
            callback();
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetReplayUUID(string songHash, Action<string> callback)
        {
            var apiObj = new SerializableClass.ReplayUUIDSubmission() { apiKey = Plugin.Instance.APIKey.Value, songHash = songHash };
            var apiKeyAndSongHash = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest($"{APIURL}/api/replay/start/", apiKeyAndSongHash);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
            {
                string replayUUID = JsonConvert.DeserializeObject<SerializableClass.ReplayStart>(webRequest.downloadHandler.text).id;
                Plugin.LogInfo("Current Replay UUID: " + replayUUID);
                callback(replayUUID);
            }
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> OnReplayStopUUID(string songHash, string replayUUID)
        {
            var apiObj = new SerializableClass.ReplayStopSubmission() { apiKey = Plugin.Instance.APIKey.Value, replayId = replayUUID };
            var apiKeyAndSongHash = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest($"{APIURL}/api/replay/stop/", apiKeyAndSongHash);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
                Plugin.LogInfo("Stopped UUID: " + replayUUID);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SubmitReplay(string replayFileName, string uuid)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");

            byte[] replayFile;

            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = new FileStream(replayDir + replayFileName, FileMode.Open))
                {
                    fileStream.CopyTo(memoryStream);
                }
                replayFile = memoryStream.ToArray();
            }

            string apiLink = $"{APIURL}/api/replay/submit/";
            WWWForm form = new WWWForm();
            form.AddField("apiKey", Plugin.Instance.APIKey.Value);
            form.AddField("replayId", uuid);
            form.AddBinaryData("replayFile", replayFile);

            Plugin.LogInfo($"Sending Replay for {uuid}.");
            var webRequest = UnityWebRequest.Post(apiLink, form);

            yield return webRequest.SendWebRequest();
            if (!HasError(webRequest, true))
                Plugin.LogInfo($"Replay Sent.");
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> DownloadReplay(string uuid, Action<string> callback)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");

            UnityWebRequest webRequest = UnityWebRequest.Get(REPLAYURL + uuid + ".ttr");

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
            {
                File.WriteAllBytes(replayDir + uuid + ".ttr", webRequest.downloadHandler.data);

                Plugin.LogInfo("Replay Downloaded.");
                callback(uuid);
            }
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetSongDataFromDB(int songID, Action<SerializableClass.SongDataFromDB> callback)
        {
            string apiLink = $"{APIURL}/api/songs/{songID}";

            UnityWebRequest webRequest = UnityWebRequest.Get(apiLink);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, false))
            {
                var songData = JsonConvert.DeserializeObject<SerializableClass.SongInfoFromDB>(webRequest.downloadHandler.GetText()).results[0];
                callback(songData);
            }
            else
                callback(null);

        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetLeaderboardScoresFromDB(int songID, Action<List<SerializableClass.ScoreDataFromDB>> callback)
        {
            string apiLink = $"{APIURL}/api/songs/{songID}/leaderboard/";

            UnityWebRequest webRequest = UnityWebRequest.Get(apiLink);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, false))
            {
                List<SerializableClass.ScoreDataFromDB> scoreList = new List<SerializableClass.ScoreDataFromDB>();

                var leaderboardInfo = JsonConvert.DeserializeObject<SerializableClass.LeaderboardInfo>(webRequest.downloadHandler.GetText());
                foreach (SerializableClass.ScoreDataFromDB score in leaderboardInfo.results)
                {
                    scoreList.Add(score);
                }
                callback(scoreList);
            }
            else
                callback(null);

        }

        public static IEnumerator<UnityWebRequestAsyncOperation> LoadTextureFromServer(string apiLink, Action<Texture2D> callback)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(apiLink);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
                callback(DownloadHandlerTexture.GetContent(webRequest));
        }


        public static IEnumerator<UnityWebRequestAsyncOperation> DownloadTextureFromServer(string apiLink, string outputPath, Action<bool> callback)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(apiLink);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, false))
            {
                File.WriteAllBytes(outputPath, webRequest.downloadHandler.data);
                callback(true);
            }
            else
                callback(false);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> TryLoadingTextureLocal(string filePath, Action<Texture2D> callback)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture("file://" + filePath);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, false))
                callback(DownloadHandlerTexture.GetContent(webRequest));
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SendModInfo(Dictionary<string, BepInEx.PluginInfo> modsDict, Action<bool> callback)
        {
            var sendableModInfo = new SerializableClass.ModInfoAPI();
            var mods = new List<SerializableClass.SendableModInfo>();
            bool allowSubmit = true;

            foreach (string key in modsDict.Keys)
            {
                var mod = new SerializableClass.SendableModInfo
                {
                    name = modsDict[key].Metadata.Name,
                    version = modsDict[key].Metadata.Version.ToString(),
                    hash = SongDataHelper.CalcSHA256Hash(File.ReadAllBytes(modsDict[key].Location))
                };

                if (mod.name == "CircularBreathing")
                {
                    PopUpNotifManager.DisplayNotif("Circular Breathing detected!\n Uninstall the mod to submit scores on TootTally.", GameTheme.themeColors.notification.warningText, 9.5f);
                    allowSubmit = false;
                }
                mods.Add(mod);
            }

            sendableModInfo.apiKey = Plugin.Instance.APIKey.Value;
            sendableModInfo.mods = mods.ToArray();
            string apiLink = $"{APIURL}/api/mods/submit/";
            var jsonbin = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendableModInfo));

            UnityWebRequest webRequest = PostUploadRequest(apiLink, jsonbin);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
            {
                Plugin.LogInfo("Request successful");
            }
            callback(allowSubmit);
        }

        private static UnityWebRequest PostUploadRequest(string apiLink, byte[] data, string contentType = "application/json")
        {

            DownloadHandler dlHandler = new DownloadHandlerBuffer();
            UploadHandler ulHandler = new UploadHandlerRaw(data);
            ulHandler.contentType = contentType;

            UnityWebRequest webRequest = new UnityWebRequest(apiLink, "POST", dlHandler, ulHandler);
            return webRequest;
        }

        private static bool HasError(UnityWebRequest webRequest, bool isLoggingErrors)
        {
            if (isLoggingErrors)
                if (webRequest.isNetworkError)
                    Plugin.LogError($"NETWORK ERROR: {webRequest.error}");
                else if (webRequest.isHttpError)
                    Plugin.LogError($"HTTP ERROR {webRequest.error}");
            return webRequest.isNetworkError || webRequest.isHttpError;
        }
    }
}
