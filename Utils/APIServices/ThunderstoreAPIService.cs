using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace TootTally.Utils
{
    public static class ThunderstoreAPIService
    {
        public const string APIURL = "https://trombone-champ.thunderstore.io";

        public static IEnumerator<UnityWebRequestAsyncOperation> GetMostRecentModVersion(Action<string> callback)
        {
            string apiLink = $"{APIURL}/api/experimental/package/TootTally/TootTally/";

            UnityWebRequest webRequest = UnityWebRequest.Get(apiLink);
            yield return webRequest.SendWebRequest();

            if (!HasError(webRequest, true))
            {
                JSONNode json = JSON.Parse(webRequest.downloadHandler.text);
                callback(json["latest"]["version_number"]);
            }
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
