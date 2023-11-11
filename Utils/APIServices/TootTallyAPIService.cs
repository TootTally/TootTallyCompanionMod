using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using Newtonsoft.Json;
using TootTally.Graphics;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.Networking;
using static TootTally.Utils.APIServices.SerializableClass;

namespace TootTally.Utils
{
    public static class TootTallyAPIService
    {
        public const string APIURL = "https://toottally.com";
        public const string SPECURL = "https://spec.toottally.com";
        public const string MULTURL = "https://spec.toottally.com/mp";
        //public const string APIURL = "http://localhost"; //localTesting
        public const string REPLAYURL = "http://cdn.toottally.com/replays/";
        public const string PFPURL = "https://cdn.toottally.com/profile/";

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

        /*public static async void GetLocalChartRatings(string tmbPath, Action<Chart> callback)
        {
            Chart chart = null;
            TootTallyLogger.LogInfo($"Looking for {tmbPath}");
            await Task.Run(() => chart = ChartReader.LoadChart(tmbPath));
            if (chart != null)
            {
                TootTallyLogger.LogInfo($"{chart.shortName} processed locally in {chart.calculationTime.TotalMilliseconds}ms");
                callback(chart);
            }

        }*/

        public static IEnumerator<UnityWebRequestAsyncOperation> GetUserFromAPIKey(Action<User> callback)
        {
            // TODO: Might have to redo this to follow the same pattern as SubmitScore
            var apiObj = new APISubmission() { apiKey = Plugin.Instance.APIKey.Value };
            var apiKey = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest($"{APIURL}/api/profile/self/", apiKey);
            User user;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest))
            {
                user = JsonConvert.DeserializeObject<User>(webRequest.downloadHandler.text);
                TootTallyLogger.LogInfo($"Welcome, {user.username}!");
            }
            else
            {
                user = new User()
                {
                    username = "Guest",
                    id = 0,
                };
                TootTallyLogger.LogInfo($"Logged in with Guest Account");
            }
            callback(user);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetUserFromID(int id, Action<User> callback)
        {
            var query = $"{APIURL}/api/profile/{id}";
            UnityWebRequest webRequest = UnityWebRequest.Get(query);
            User user;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                user = JsonConvert.DeserializeObject<User>(webRequest.downloadHandler.text);
                TootTallyLogger.LogInfo($"Welcome, {user.username}!");
                callback(user);
            }
        }


        public static IEnumerator<UnityWebRequestAsyncOperation> GetMessageFromAPIKey(Action<APIMessages> callback)
        {
            var query = $"{APIURL}/api/announcements/?apiKey={Plugin.Instance.APIKey.Value}";
            var webRequest = UnityWebRequest.Get(query);
            APIMessages messages;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                messages = JsonConvert.DeserializeObject<APIMessages>(webRequest.downloadHandler.text);
                if (messages.results.Count > 0)
                    callback(messages);
            }
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetUserFromToken(string token, Action<User> callback)
        {
            var query = $"{APIURL}/auth/self/";
            UnityWebRequest webRequest = UnityWebRequest.Get(query);
            webRequest.SetRequestHeader("Authorization", $"Token {token}");
            User user;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                user = JsonConvert.DeserializeObject<User>(webRequest.downloadHandler.text);
                TootTallyLogger.LogInfo($"Welcome, {user.username}!");
                callback(user);
            }
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetLoginToken(string username, string password, Action<LoginToken> callback)
        {
            var query = $"{APIURL}/auth/token/";
            var apiObj = new APILogin() { username = username, password = password };
            var apiLogin = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest(query, apiLogin);
            LoginToken token;
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                token = JsonConvert.DeserializeObject<LoginToken>(webRequest.downloadHandler.text);
            }
            else
            {
                token = new LoginToken()
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
            var apiObj = new APISignUp() { username = username, password = password, pass_check = pass_check };
            var apiSignUp = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest(query, apiSignUp);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                TootTallyLogger.LogInfo($"Account {username} created!");
                callback(true);
            }
            else
                callback(false);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetReplayUUID(string songHash, Action<string> callback)
        {
            var query = $"{APIURL}/api/replay/start/";
            var apiObj = new ReplayUUIDSubmission() { apiKey = Plugin.Instance.APIKey.Value, songHash = songHash, speed = Replays.ReplaySystemManager.gameSpeedMultiplier };
            var apiKeyAndSongHash = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest(query, apiKeyAndSongHash);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                string replayUUID = JsonConvert.DeserializeObject<ReplayStart>(webRequest.downloadHandler.text).id;
                TootTallyLogger.LogInfo("Current Replay UUID: " + replayUUID);
                callback(replayUUID);
            }
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> OnReplayStopUUID(string songHash, string replayUUID)
        {
            var query = $"{APIURL}/api/replay/stop/";
            var apiObj = new ReplayStopSubmission() { apiKey = Plugin.Instance.APIKey.Value, replayId = replayUUID };
            var apiKeyAndSongHash = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest(query, apiKeyAndSongHash);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
                TootTallyLogger.LogInfo("Stopped UUID: " + replayUUID);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SubmitReplay(string replayFileName, string uuid, Action<ReplaySubmissionReply, bool> callback)
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
                callback(JsonConvert.DeserializeObject<ReplaySubmissionReply>(webRequest.downloadHandler.text), false);
            }
            else
                callback(null, webRequest.responseCode >= 500);
        }

        public static IEnumerator<WaitForSeconds> WaitForSecondsCallback(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback();
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

        public static IEnumerator<UnityWebRequestAsyncOperation> DownloadZipFromServer(string downloadlink, ProgressBar bar, Action<byte[]> callback)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(downloadlink);
            webRequest.SendWebRequest();

            bar.ToggleActive();

            while (!webRequest.isDone)
            {
                bar.UpdateValue(webRequest.downloadProgress);

                yield return null;
            }

            bar.ToggleActive();

            if (!HasError(webRequest, downloadlink))
                callback(webRequest.downloadHandler.data);
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> DownloadZipFromServer(string downloadlink, Action<byte[]> callback) //Progress barless for old twitch plugin versions
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(downloadlink);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, downloadlink))
                callback(webRequest.downloadHandler.data);
            else
                callback(null);
        }


        public static IEnumerator<UnityWebRequestAsyncOperation> GetSongDataFromDB(int songID, Action<SongDataFromDB> callback)
        {
            string query = $"{APIURL}/api/songs/{songID}";

            UnityWebRequest webRequest = UnityWebRequest.Get(query);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                var songData = JsonConvert.DeserializeObject<SongInfoFromDB>(webRequest.downloadHandler.text).results[0];
                callback(songData);
            }
            else
                callback(null);

        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetLeaderboardScoresFromDB(int songID, Action<List<ScoreDataFromDB>> callback)
        {
            string query = $"{APIURL}/api/songs/{songID}/leaderboard/";

            UnityWebRequest webRequest = UnityWebRequest.Get(query);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                List<ScoreDataFromDB> scoreList = new List<ScoreDataFromDB>();

                var leaderboardInfo = JsonConvert.DeserializeObject<LeaderboardInfo>(webRequest.downloadHandler.text);
                foreach (ScoreDataFromDB score in leaderboardInfo.results)
                {
                    scoreList.Add(score);
                }
                callback(scoreList);
            }
            else
                callback(null);

        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetValidTwitchAccessToken(Action<TwitchAccessToken> callback)
        {
            string query = $"{APIURL}/api/twitch/self/";
            var apiObj = new APISubmission() { apiKey = Plugin.Instance.APIKey.Value };
            var apiKey = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(apiObj));
            var webRequest = PostUploadRequest(query, apiKey);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest))
            {
                var token_info = JsonConvert.DeserializeObject<TwitchAccessToken>(webRequest.downloadHandler.text);
                callback(token_info);
            }
            else
            {
                TootTallyLogger.LogError($"Could not get active access token.");
                PopUpNotifManager.DisplayNotif("Could not get active access token, please re-authorize TootTally on Twitch", GameTheme.themeColors.notification.warningText, 10f);
                callback(null);
            }
        }

        //Unused for now because we're storing textures locally, but could be useful in the future...
        public static IEnumerator<UnityWebRequestAsyncOperation> LoadTextureFromServer(string query, Action<Texture2D> callback)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(query);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
                callback(DownloadHandlerTexture.GetContent(webRequest));
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> LoadPFPFromServer(int userID, Action<Texture2D> callback)
        {
            var query = PFPURL + userID + ".png";
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(query);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
                callback(DownloadHandlerTexture.GetContent(webRequest));
            else
                callback(null);
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
            var sendableModInfo = new ModInfoAPI();
            var mods = new List<SendableModInfo>();
            bool allowSubmit = true;

            foreach (string key in modsDict.Keys)
            {
                var mod = new SendableModInfo
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

        public static IEnumerator<UnityWebRequestAsyncOperation> SendUserStatus(int status, Action callback = null)
        {
            APIHeartbeat heartbeat = new APIHeartbeat() { apiKey = Plugin.Instance.APIKey.Value, status = status };

            string query = $"{APIURL}/api/profile/heartbeat/";
            var data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(heartbeat));
            UnityWebRequest webRequest = PostUploadRequest(query, data);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
                callback?.Invoke();
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetLatestOnlineUsers(Action<List<User>> callback)
        {
            string query = $"{APIURL}/api/users/latest/?userID={Plugin.userInfo.id}";

            UnityWebRequest webRequest = UnityWebRequest.Get(query);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                var userList = JsonConvert.DeserializeObject<APIUsers>(webRequest.downloadHandler.text).results;
                callback(userList);
            }
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetFirstPageUsers(Action<List<User>> callback)
        {
            string query = $"{APIURL}/api/users/?userID={Plugin.userInfo.id}";

            UnityWebRequest webRequest = UnityWebRequest.Get(query);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                var userList = JsonConvert.DeserializeObject<APIUsers>(webRequest.downloadHandler.text).results;
                callback(userList);
            }
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetAllUsersUpToPageID(int pageID, Action<List<User>> callback)
        {
            string query = $"{APIURL}/api/users/?userID={Plugin.userInfo.id}";
            List<User> userList = new List<User>();

            for (int i = 1; i < pageID; i++)
            {
                UnityWebRequest webRequest = UnityWebRequest.Get(query);

                yield return webRequest.SendWebRequest();

                if (!HasError(webRequest, query))
                {
                    var response = JsonConvert.DeserializeObject<APIUsers>(webRequest.downloadHandler.text);
                    userList.AddRange(response.results);
                    query = response.next;
                }
                else
                    callback(null);
            }

            callback(userList);

        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetOnlineUsersBySearch(string username, Action<List<User>> callback)
        {
            string query = $"{APIURL}/api/users/search/?username={username}?userID={Plugin.userInfo.id}";

            UnityWebRequest webRequest = UnityWebRequest.Get(query);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                var userList = JsonConvert.DeserializeObject<APIUsers>(webRequest.downloadHandler.text).results;
                callback(userList);
            }
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SearchSongByURL(string URL, Action<SongInfoFromDB> callback)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(URL);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, URL))
            {
                var userList = JsonConvert.DeserializeObject<SongInfoFromDB>(webRequest.downloadHandler.text);
                callback(userList);
            }
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> SearchSongWithFilters(string songName, bool isRated, bool isUnrated, Action<SongInfoFromDB> callback)
        {
            string filters = isRated ? "&rated=1" : "";
            filters += !isRated && isUnrated ? "&rated=0" : "";
            filters += "&page_size=100";
            string query = $"{APIURL}/api/search/?song_name={songName}{filters}";

            UnityWebRequest webRequest = UnityWebRequest.Get(query);

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                var userList = JsonConvert.DeserializeObject<SongInfoFromDB>(webRequest.downloadHandler.text);
                callback(userList);
            }
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetFriendList(Action<List<User>> callback)
        {

            APISubmission APIKey = new APISubmission() { apiKey = Plugin.Instance.APIKey.Value };

            string query = $"{APIURL}/api/friends/all/";
            var data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(APIKey));
            UnityWebRequest webRequest = PostUploadRequest(query, data);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                var userList = JsonConvert.DeserializeObject<APIUsers>(webRequest.downloadHandler.text).results;
                callback(userList);
            }
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetFileSize(string downloadLink, Action<FileHelper.FileData> callback)
        {
            UnityWebRequest webRequest = UnityWebRequest.Head(downloadLink);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest))
                callback(new FileHelper.FileData() { size = Convert.ToInt64(webRequest.GetResponseHeader("Content-Length")), extension = webRequest.GetResponseHeader("Content-Type").Split('/').Last() });
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetSpectatorIDList(Action<int[]> callback)
        {
            string query = $"{SPECURL}/active";
            UnityWebRequest webRequest = UnityWebRequest.Get(query);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                var idArray = JsonConvert.DeserializeObject<SerializableClass.APIActiveSpectator>(webRequest.downloadHandler.text).active;
                callback(idArray);
            }
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> GetOnlineFriends(Action<List<User>> callback)
        {

            APISubmission APIKey = new APISubmission() { apiKey = Plugin.Instance.APIKey.Value };

            string query = $"{APIURL}/api/friends/online/";
            var data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(APIKey));
            UnityWebRequest webRequest = PostUploadRequest(query, data);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
            {
                var userList = JsonConvert.DeserializeObject<APIUsers>(webRequest.downloadHandler.text).results;
                callback(userList);
            }
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> CreateMultiplayerServerRequest(string name, string description, string password, int maxPlayer, Action<string> callback)
        {
            string query = $"{MULTURL}/create";

            APISubmission APIKey = new APISubmission() { apiKey = Plugin.Instance.APIKey.Value };
            var data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(APIKey));
            UnityWebRequest webRequest = PostUploadRequestWithHeader(query, data, new List<string[]> { new string[] { "Authorization", "APIKey " + Plugin.Instance.APIKey.Value } });

            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, query))
                callback(webRequest.downloadHandler.text);
            else
                callback(null);
        }

        public static IEnumerator<UnityWebRequestAsyncOperation> AddFriend(int userID, Action<bool> callback = null)
        {
            APIFriendSubmission apiObj = new APIFriendSubmission() { apiKey = Plugin.Instance.APIKey.Value, userID = userID };

            string query = $"{APIURL}/api/friends/add/";
            var data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(apiObj));
            UnityWebRequest webRequest = PostUploadRequest(query, data);
            yield return webRequest.SendWebRequest();
            if (!HasError(webRequest, query))
                callback(true);
            else
                callback(false);
        }
        public static IEnumerator<UnityWebRequestAsyncOperation> RemoveFriend(int userID, Action<bool> callback = null)
        {

            APIFriendSubmission apiObj = new APIFriendSubmission() { apiKey = Plugin.Instance.APIKey.Value, userID = userID };

            string query = $"{APIURL}/api/friends/remove/";
            var data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(apiObj));
            UnityWebRequest webRequest = PostUploadRequest(query, data);
            yield return webRequest.SendWebRequest();
            if (!HasError(webRequest, query))
                callback(true);
            else
                callback(false);
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
