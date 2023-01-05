using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TootTally
{
    public static class TootTallyAPIService
    {
        //public const string APIURL = "https://toottally.com";
        public const string APIURL = "http://localhost"; //localTesting
        #region Logs
        internal static void LogDebug(string msg) => Plugin.LogDebug(msg);
        internal static void LogInfo(string msg) => Plugin.LogInfo(msg);
        internal static void LogError(string msg) => Plugin.LogError(msg);
        internal static void LogWarning(string msg) => Plugin.LogWarning(msg);
        #endregion

        public static IEnumerator<UnityWebRequestAsyncOperation> GetHashInDB(string songHash, Action<int> callback)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get($"{APIURL}/hashcheck/{songHash}/");
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, false))
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
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, false))
            {
                var jsonData = JSONObject.Parse(webRequest.downloadHandler.text);
                SerializableClass.User user = new SerializableClass.User()
                {
                    username = jsonData["username"],
                    id = jsonData["id"],
                };
                LogInfo($"Welcome, {user.username}!");
                callback(user);
            }
            else
            {
                var user = new SerializableClass.User()
                {
                    username = "Guest",
                    id = 0,
                };
                callback(user);
            }
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
                LogInfo($"Chart Sent.");


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

        public static IEnumerator<UnityWebRequestAsyncOperation> SubmitReplay(string replayData, string uuid)
        {
            string apiLink = $"{APIURL}/api/replay/submit/";
            var replayObj = new SerializableClass.ReplayJsonSubmission()
            {
                apiKey = Plugin.Instance.APIKey.Value,
                replayData = replayData,
                uuid = uuid
            };

            var replaySubmission = Encoding.UTF8.GetBytes(JsonUtility.ToJson(replayObj));

            var webRequest = PostUploadRequest(apiLink, replaySubmission);

            yield return webRequest.SendWebRequest();
            if (!HasError(webRequest, true))
                LogInfo($"Replay Sent.");
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetLeaderboardScoresFromDB(int songID, Action<List<SerializableClass.ScoreDataFromDB>> callback)
        {
            string apiLink = $"{APIURL}/api/songs/{songID}/leaderboard/";

            UnityWebRequest webRequest = UnityWebRequest.Get(apiLink);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
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
                        max_combo = scoreJson["max_combo"],
                        percentage = scoreJson["percentage"],
                        game_version = scoreJson["game_version"],
                    };
                    scoreList.Add(score);
                }
                callback(scoreList);
            }

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
