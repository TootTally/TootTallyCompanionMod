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
            string query = isCustom ? $"{APIURL}/hashcheck/{songHash}/" : $"{APIURL}/api/hashcheck/official/?trackref={songHash}";
            UnityWebRequest webRequest = UnityWebRequest.Get(query);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                callback(int.Parse(webRequest.downloadHandler.text)); //.text returns the digit of ex: https://toottally.com/api/songs/182/leaderboard/
            }
            else
                callback(0); //hash 0 is null

        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetUserFromAPIKey(Action<SerializableClass.User> callback)
        {
            // TODO: Might have to redo this to follow the same pattern as SubmitScore
            var apiObj = new SerializableClass.APISubmission() { apiKey = Plugin.Instance.APIKey.Value };
            var apiKey = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest($"{APIURL}/api/profile/self/", apiKey);
            SerializableClass.User user;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest))
            {
                user = JsonConvert.DeserializeObject<SerializableClass.User>(webRequest.downloadHandler.text);
                TootTallyLogger.LogInfo($"Welcome, {user.username}!");
            }
            else
            {
                user = new SerializableClass.User()
                {
                    username = "Guest",
                    id = 0,
                };
                TootTallyLogger.LogInfo($"Logged in with Guest Account");
            }
            callback(user);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetUserFromID(int id, Action<SerializableClass.User> callback)
        {
            var query = $"{APIURL}/api/profile/{id}";
            UnityWebRequest webRequest = UnityWebRequest.Get(query);
            SerializableClass.User user;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                user = JsonConvert.DeserializeObject<SerializableClass.User>(webRequest.downloadHandler.text);
                TootTallyLogger.LogInfo($"Welcome, {user.username}!");
                callback(user);
            }
        }


        public static IEnumerator<UnityWebRequestAsyncOperation> GetMessageFromAPIKey(Action<SerializableClass.APIMessages> callback)
        {
            var query = $"{APIURL}/api/announcements/?apiKey={Plugin.Instance.APIKey.Value}";
            var webRequest = UnityWebRequest.Get(query);
            SerializableClass.APIMessages messages;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                messages = JsonConvert.DeserializeObject<SerializableClass.APIMessages>(webRequest.downloadHandler.text);
                if (messages.results.Count > 0)
                    callback(messages);
            }
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetUserFromToken(string token, Action<SerializableClass.User> callback)
        {
            var query = $"{APIURL}/auth/self/";
            UnityWebRequest webRequest = UnityWebRequest.Get(query);
            webRequest.SetRequestHeader("Authorization", $"Token {token}");
            SerializableClass.User user;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                user = JsonConvert.DeserializeObject<SerializableClass.User>(webRequest.downloadHandler.text);
                TootTallyLogger.LogInfo($"Welcome, {user.username}!");
            }
            else
            {
                user = new SerializableClass.User()
                {
                    username = "Guest",
                    id = 0,
                };
                TootTallyLogger.LogInfo($"Logged in with Guest Account");
            }
            callback(user);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetLoginToken(string username, string password, Action<SerializableClass.LoginToken> callback)
        {
            var query = $"{APIURL}/auth/token/";
            var apiObj = new SerializableClass.APILogin() { username = username, password = password };
            var apiLogin = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest(query, apiLogin);
            SerializableClass.LoginToken token;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                token = JsonConvert.DeserializeObject<SerializableClass.LoginToken>(webRequest.downloadHandler.text);
            }
            else
            {
                token = new SerializableClass.LoginToken()
                {
                    token = ""
                };
                TootTallyLogger.LogInfo($"Error Logging in");
            }
            callback(token);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SignUpRequest(string username, string password, string pass_check, Action<bool> callback)
        {
            var query = $"{APIURL}/auth/signup/";
            var apiObj = new SerializableClass.APISignUp() { username = username, password = password, pass_check = pass_check };
            var apiSignUp = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest(query, apiSignUp);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                TootTallyLogger.LogInfo($"Account {username} created!");
                callback(true);
            }
            callback(false);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> AddChartInDB(SerializableClass.TMBFile chart, Action callback)
        {

            string query = $"{APIURL}/api/upload/";
            string jsonified = JsonUtility.ToJson(chart);
            var jsonbin = System.Text.Encoding.UTF8.GetBytes(jsonified);

            UnityWebRequest webRequest = PostUploadRequest(query, jsonbin);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                if (webRequest.downloadHandler.text.Equals("Chart requested to skip"))
                    PopUpNotifManager.DisplayNotif(webRequest.downloadHandler.text, GameTheme.themeColors.notification.warningText);
                else
                {
                    TootTallyLogger.LogInfo($"Chart Sent.");
                    PopUpNotifManager.DisplayNotif("New chart sent to TootTally", Color.green);
                }
            }
            else
                PopUpNotifManager.DisplayNotif("Error in sending chart", GameTheme.themeColors.notification.errorText);
            callback();
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetReplayUUID(string songHash, Action<string> callback)
        {
            var query = $"{APIURL}/api/replay/start/";
            var apiObj = new SerializableClass.ReplayUUIDSubmission() { apiKey = Plugin.Instance.APIKey.Value, songHash = songHash, speed = Replays.ReplaySystemManager.gameSpeedMultiplier };
            var apiKeyAndSongHash = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest(query, apiKeyAndSongHash);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                string replayUUID = JsonConvert.DeserializeObject<SerializableClass.ReplayStart>(webRequest.downloadHandler.text).id;
                TootTallyLogger.LogInfo("Current Replay UUID: " + replayUUID);
                callback(replayUUID);
            }
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> OnReplayStopUUID(string songHash, string replayUUID)
        {
            var query = $"{APIURL}/api/replay/stop/";
            var apiObj = new SerializableClass.ReplayStopSubmission() { apiKey = Plugin.Instance.APIKey.Value, replayId = replayUUID };
            var apiKeyAndSongHash = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest(query, apiKeyAndSongHash);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
                TootTallyLogger.LogInfo("Stopped UUID: " + replayUUID);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SubmitReplay(string replayFileName, string uuid, Action<SerializableClass.ReplaySubmissionReply> callback)
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

            string query = $"{APIURL}/api/replay/submit/";
            WWWForm form = new WWWForm();
            form.AddField("apiKey", Plugin.Instance.APIKey.Value);
            form.AddField("replayId", uuid);
            form.AddBinaryData("replayFile", replayFile);

            TootTallyLogger.LogInfo($"Sending Replay for {uuid}.");
            var webRequest = UnityWebRequest.Post(query, form);

            yield return webRequest.SendWebRequest();
            if (!HasError(webRequest, query))
            {
                TootTallyLogger.LogInfo($"Replay Sent.");
                callback(JsonConvert.DeserializeObject<SerializableClass.ReplaySubmissionReply>(webRequest.downloadHandler.text));
            }
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> DownloadReplay(string uuid, Action<string> callback)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            var query = REPLAYURL + uuid + ".ttr";
            UnityWebRequest webRequest = UnityWebRequest.Get(query);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                File.WriteAllBytes(replayDir + uuid + ".ttr", webRequest.downloadHandler.data);

                TootTallyLogger.LogInfo("Replay Downloaded.");
                callback(uuid);
            }
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetSongDataFromDB(int songID, Action<SerializableClass.SongDataFromDB> callback)
        {
            string query = $"{APIURL}/api/songs/{songID}";

            UnityWebRequest webRequest = UnityWebRequest.Get(query);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                var songData = JsonConvert.DeserializeObject<SerializableClass.SongInfoFromDB>(webRequest.downloadHandler.text).results[0];
                callback(songData);
            }
            else
                callback(null);

        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetLeaderboardScoresFromDB(int songID, Action<List<SerializableClass.ScoreDataFromDB>> callback)
        {
            string query = $"{APIURL}/api/songs/{songID}/leaderboard/";

            UnityWebRequest webRequest = UnityWebRequest.Get(query);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                List<SerializableClass.ScoreDataFromDB> scoreList = new List<SerializableClass.ScoreDataFromDB>();

                var leaderboardInfo = JsonConvert.DeserializeObject<SerializableClass.LeaderboardInfo>(webRequest.downloadHandler.text);
                foreach (SerializableClass.ScoreDataFromDB score in leaderboardInfo.results)
                {
                    scoreList.Add(score);
                }
                callback(scoreList);
            }
            else
                callback(null);

        }

        //Unused for now because we're storing textures locally, but could be useful in the future...
        public static IEnumerator<UnityWebRequestAsyncOperation> LoadTextureFromServer(string query, Action<Texture2D> callback)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(query);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
                callback(DownloadHandlerTexture.GetContent(webRequest));
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> DownloadTextureFromServer(string query, string outputPath, Action<bool> callback)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(query);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                File.WriteAllBytes(outputPath, webRequest.downloadHandler.data);
                callback(true);
            }
            else
                callback(false);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> TryLoadingTextureLocal(string filePath, Action<Texture2D> callback)
        {
            var query = $"file://{filePath}";
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(query);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
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

                mods.Add(mod);
            }

            sendableModInfo.apiKey = Plugin.Instance.APIKey.Value;
            sendableModInfo.mods = mods.ToArray();
            string query = $"{APIURL}/api/mods/submit/";
            var jsonbin = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendableModInfo));

            UnityWebRequest webRequest = PostUploadRequest(query, jsonbin);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                TootTallyLogger.LogInfo("Request successful");
            }
            callback(allowSubmit);
        }

        private static UnityWebRequest PostUploadRequest(string query, byte[] data, string contentType = "application/json")
        {

            DownloadHandler dlHandler = new DownloadHandlerBuffer();
            UploadHandler ulHandler = new UploadHandlerRaw(data);
            ulHandler.contentType = contentType;


            UnityWebRequest webRequest = new UnityWebRequest(query, "POST", dlHandler, ulHandler);
            return webRequest;
        }
        private static UnityWebRequest PostUploadRequestWithHeader(string query, byte[] data, List<string[]> headers, string contentType = "application/json")
        {
            DownloadHandler dlHandler = new DownloadHandlerBuffer();
            UploadHandler ulHandler = new UploadHandlerRaw(data);
            ulHandler.contentType = contentType;


            UnityWebRequest webRequest = new UnityWebRequest(query, "POST", dlHandler, ulHandler);
            foreach (string[] s in headers)
                webRequest.SetRequestHeader(s[0], s[1]);
            return webRequest;
        }

        private static bool HasError(UnityWebRequest webRequest)
        {
            return webRequest.isNetworkError || webRequest.isHttpError;
        }

        private static bool HasError(UnityWebRequest webRequest, string query)
        {
            if (webRequest.isNetworkError || webRequest.isHttpError)
                TootTallyLogger.LogError($"QUERY ERROR: {query}");
            if (webRequest.isNetworkError)
                TootTallyLogger.LogError($"NETWORK ERROR: {webRequest.error}");
            if (webRequest.isHttpError)
                TootTallyLogger.LogError($"HTTP ERROR {webRequest.error}");

            return webRequest.isNetworkError || webRequest.isHttpError;
        }
    }
}
