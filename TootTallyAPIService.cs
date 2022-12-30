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
            bool inDatabase = false;
            UnityWebRequest webRequest = UnityWebRequest.Get($"{APIURL}/hashcheck/{songHash}/");
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest))
            {
                LogInfo("HTTP 200 OK: It's in the database!");
                inDatabase = true;
            }

            callback(inDatabase);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> AddChartInDB(string tmb)
        {

            SerializableSubmissionClass.Chart chart = new SerializableSubmissionClass.Chart { tmb = tmb };
            string apiLink = $"{APIURL}/api/upload/";
            string jsonified = JsonUtility.ToJson(chart);
            LogDebug($"Chart JSON: {jsonified}");
            var jsonbin = System.Text.Encoding.UTF8.GetBytes(jsonified);

            UnityWebRequest webRequest = PostUploadRequest(apiLink, jsonbin);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest))
                LogInfo($"CHART SENT SUCCESFULLY");


        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SubmitScore(SerializableSubmissionClass.Score score)
        {
            string apiLink = $"{APIURL}/api/submitscore/";
            string jsonified = JsonUtility.ToJson(score);
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

        public static IEnumerator<UnityWebRequestAsyncOperation> PostUploadRequest(string apiLink, byte[] data, string contentType = "application/json")
        {

            DownloadHandler dlHandler = new DownloadHandler();
            UploadHandler ulHandler = new UploadHandlerRaw(data);
            ulHandler.contentType = contentType;

            UnityWebRequest webRequest = new UnityWebRequest(apiLink, "POST", dlHandler, ulHandler);
            yield return webRequest.SendWebRequest();
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
