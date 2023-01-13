using BepInEx;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TootTally.Utils
{
    public static class TootTallyAPIService
    {
        public const string APIURL = "https://toottally.com";
        //public const string APIURL = "http://localhost"; //localTesting
        public const string REPLAYURL = "http://cdn.toottally.com/replays/";
        #region Logs
        internal static void LogDebug(string msg) => Plugin.LogDebug(msg);
        internal static void LogInfo(string msg) => Plugin.LogInfo(msg);
        internal static void LogError(string msg) => Plugin.LogError(msg);
        internal static void LogWarning(string msg) => Plugin.LogWarning(msg);
        #endregion

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
                var jsonData = JSONObject.Parse(webRequest.downloadHandler.text);
                user = new SerializableClass.User()
                {
                    username = jsonData["username"],
                    id = jsonData["id"],
                };
                PopUpNotifManager.DisplayNotif($"Welcome, {user.username}!", 9f);
                LogInfo($"Welcome, {user.username}!");
            }
            else
            {
                user = new SerializableClass.User()
                {
                    username = "Guest",
                    id = 0,
                };
            }
            callback(user);

        }

        public static IEnumerator<UnityWebRequestAsyncOperation> AddChartInDB(SerializableClass.Chart chart)
        {

            string apiLink = $"{APIURL}/api/upload/";
            string jsonified = JsonUtility.ToJson(chart);
            LogDebug($"Chart JSON: {jsonified}");
            var jsonbin = System.Text.Encoding.UTF8.GetBytes(jsonified);

            UnityWebRequest webRequest = PostUploadRequest(apiLink, jsonbin);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
            {
                LogInfo($"Chart Sent.");
                PopUpNotifManager.DisplayNotif("New chart sent to TootTally");
            }
            else
            {
                PopUpNotifManager.DisplayNotif("Error in sending chart");
            }


        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SubmitScore(SerializableClass.SendableScore score)
        {
            string apiLink = $"{APIURL}/api/submitscore/";
            string jsonified = JsonUtility.ToJson(score);
            var jsonbin = System.Text.Encoding.UTF8.GetBytes(jsonified);

            DownloadHandler dlHandler = new DownloadHandler();
            UploadHandler ulHandler = new UploadHandlerRaw(jsonbin);
            ulHandler.contentType = "application/json";

            UnityWebRequest webRequest = new UnityWebRequest(apiLink, "POST", dlHandler, ulHandler);
            yield return webRequest.SendWebRequest();
            if (!HasError(webRequest, true))
                LogInfo($"Score Sent.");
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetReplayUUID(string songHash, Action<string> callback)
        {
            var apiObj = new SerializableClass.ReplayUUIDSubmission() { apiKey = Plugin.Instance.APIKey.Value, songHash = songHash };
            var apiKeyAndSongHash = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest($"{APIURL}/api/replay/start/", apiKeyAndSongHash);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
                callback(JSONObject.Parse(webRequest.downloadHandler.text)["id"]);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SubmitReplay(string replayFileName, string uuid)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");

            byte[] replayFile;
            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = new FileStream(replayDir + replayFileName, FileMode.Open))
                    fileStream.CopyTo(memoryStream);
                replayFile = memoryStream.ToArray();
            }

            string apiLink = $"{APIURL}/api/replay/submit/";
            WWWForm form = new WWWForm();
            form.AddField("apiKey", Plugin.Instance.APIKey.Value);
            form.AddField("replayId", uuid);
            form.AddBinaryData("replayFile", replayFile);

            var webRequest = UnityWebRequest.Post(apiLink, form);

            yield return webRequest.SendWebRequest();
            if (!HasError(webRequest, true))
                LogInfo($"Replay Sent.");
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> DownloadReplay(string uuid, Action<string> callback)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");

            UnityWebRequest webRequest = UnityWebRequest.Get(REPLAYURL + uuid + ".ttr");

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
            {
                File.WriteAllBytes(replayDir + uuid + ".ttr", webRequest.downloadHandler.data);
                
                LogInfo("Replay Downloaded.");
                callback(uuid);
            }
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetLeaderboardScoresFromDB(int songID, Action<List<SerializableClass.ScoreDataFromDB>> callback)
        {
            string apiLink = $"{APIURL}/api/songs/{songID}/leaderboard/";

            UnityWebRequest webRequest = UnityWebRequest.Get(apiLink);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, false))
            {
                List<SerializableClass.ScoreDataFromDB> scoreList = new List<SerializableClass.ScoreDataFromDB>();

                var leaderboardJson = JSONObject.Parse(webRequest.downloadHandler.GetText());
                foreach (JSONObject scoreJson in leaderboardJson["results"])
                {
                    SerializableClass.ScoreDataFromDB score = new SerializableClass.ScoreDataFromDB()
                    {
                        score = scoreJson["score"],
                        player = scoreJson["player"],
                        played_on = scoreJson["played_on"],
                        grade = scoreJson["grade"],
                        noteTally = new int[]
                        {   scoreJson["perfect"],
                            scoreJson["nice"],
                            scoreJson["okay"],
                            scoreJson["meh"],
                            scoreJson["nasty"]},
                        replay_id = scoreJson["replay_id"] != null ? scoreJson["replay_id"] : "NA", //if no replay_id, set to NA
                        max_combo = scoreJson["max_combo"],
                        percentage = scoreJson["percentage"],
                        game_version = scoreJson["game_version"],
                    };
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
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(filePath);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, false))
                callback(DownloadHandlerTexture.GetContent(webRequest));
            else
                callback(null);
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
                    LogError($"NETWORK ERROR: {webRequest.error}");
                else if (webRequest.isHttpError)
                    LogError($"HTTP ERROR {webRequest.error}");
            return webRequest.isNetworkError || webRequest.isHttpError;
        }
    }
}
