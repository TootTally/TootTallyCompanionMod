﻿using SimpleJSON;
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
        public const string APIURL = "https://toottally.com";

        #region Logs
        internal static void LogDebug(string msg) => Plugin.LogDebug(msg);
        internal static void LogInfo(string msg) => Plugin.LogInfo(msg);
        internal static void LogError(string msg) => Plugin.LogError(msg);
        internal static void LogWarning(string msg) => Plugin.LogWarning(msg);
        #endregion

        public static IEnumerator<UnityWebRequestAsyncOperation> CheckHashInDB(string songHash, Action<bool> callback)
        {
            bool isInDatabase = false;
            UnityWebRequest webRequest = UnityWebRequest.Get($"{APIURL}/hashcheck/{songHash}/");
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest))
            {
                LogInfo("HTTP 200 OK: It's in the database!");
                isInDatabase = true;
            }

            callback(isInDatabase);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> AddChartInDB(SerializableSubmissionClass.Chart chart)
        {

            string apiLink = $"{APIURL}/api/upload/";
            string jsonified = JsonUtility.ToJson(chart);
            LogDebug($"Chart JSON: {jsonified}");
            var jsonbin = System.Text.Encoding.UTF8.GetBytes(jsonified);

            UnityWebRequest webRequest = PostUploadRequest(apiLink, jsonbin);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest))
                LogInfo($"CHART SENT SUCCESFULLY");


        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SubmitScore(SerializableSubmissionClass.SendableScore score)
        {
            string apiLink = $"{APIURL}/api/submitscore/";
            string jsonified = JsonUtility.ToJson(score);
            var jsonbin = System.Text.Encoding.UTF8.GetBytes(jsonified);

            DownloadHandler dlHandler = new DownloadHandler();
            UploadHandler ulHandler = new UploadHandlerRaw(jsonbin);
            ulHandler.contentType = "application/json";

            UnityWebRequest webRequest = new UnityWebRequest(apiLink, "POST", dlHandler, ulHandler);
            yield return webRequest.SendWebRequest();
            if (!HasError(webRequest))
                LogInfo($"SCORE SENT SUCCESFULLY");
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetLeaderboardScoresFromDB(int songID, Action<List<SerializableSubmissionClass.ScoreDataFromDB>> callback)
        {
            string apiLink = $"{Plugin.APIURL}/api/songs/{songID}/leaderboard/";

            UnityWebRequest webRequest = UnityWebRequest.Get(apiLink);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest))
                Plugin.LogInfo($"LEADERBOARD SCORES LOADED!");
            List<SerializableSubmissionClass.ScoreDataFromDB> scoreList = new List<SerializableSubmissionClass.ScoreDataFromDB>();

            var leaderboardJson = JSONObject.Parse(webRequest.downloadHandler.GetText());
            foreach (JSONObject scoreJson in leaderboardJson["results"])
            {
                SerializableSubmissionClass.ScoreDataFromDB score = new SerializableSubmissionClass.ScoreDataFromDB()
                {
                    score = scoreJson["score"],
                    player = scoreJson["player"],
                    played_on = scoreJson["played_on"],
                    grade = scoreJson["grade"],
                    noteTally = new int[] 
                    { scoreJson["perfect"],
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

        private static UnityWebRequest PostUploadRequest(string apiLink, byte[] data, string contentType = "application/json")
        {

            DownloadHandler dlHandler = new DownloadHandler();
            UploadHandler ulHandler = new UploadHandlerRaw(data);
            ulHandler.contentType = contentType;

            UnityWebRequest webRequest = new UnityWebRequest(apiLink, "POST", dlHandler, ulHandler);
            return webRequest;
        }


        private static bool HasError(UnityWebRequest webRequest)
        {
            if (webRequest.isNetworkError)
                LogError($"NETWORK ERROR: {webRequest.error}");
            else if (webRequest.isHttpError)
                LogError($"HTTP ERROR {webRequest.error}");
            return webRequest.isNetworkError || webRequest.isHttpError;
        }
    }
}