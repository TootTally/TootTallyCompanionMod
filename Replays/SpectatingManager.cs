using HarmonyLib;
using Microsoft.FSharp.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TootTally.CustomLeaderboard;
using UnityEngine;

namespace TootTally.Replays
{
    public class SpectatingManager : MonoBehaviour
    {
        public static JsonConverter[] _dataConverter = new JsonConverter[] { new SocketDataConverter() };
        private static List<SpectatingSystem> _spectatingSystemList;
        public static SpectatingSystem hostedSpectator;
        public static bool IsHosting => hostedSpectator != null && hostedSpectator.GetIsHost;

        public void Awake()
        {
            _spectatingSystemList ??= new List<SpectatingSystem>();
            if (Plugin.Instance.AllowSpectate.Value)
                CreateUniqueSpectatingConnection(Plugin.userInfo.id);
        }

        public void Update()
        {
            _spectatingSystemList?.ForEach(s => s.UpdateStacks());
        }

        public static SpectatingSystem CreateNewSpectatingConnection(int id)
        {
            var spec = new SpectatingSystem(id);
            _spectatingSystemList.Add(spec);
            if (id == Plugin.userInfo.id)
                hostedSpectator = spec;
            return spec;
        }

        public static void RemoveSpectator(SpectatingSystem spectator)
        {
            spectator.Disconnect();
            if (_spectatingSystemList.Contains(spectator))
                _spectatingSystemList.Remove(spectator);
        }

        public static SpectatingSystem CreateUniqueSpectatingConnection(int id)
        {
            if (_spectatingSystemList != null)
                for (int i = 0; i < _spectatingSystemList.Count;)
                    RemoveSpectator(_spectatingSystemList[i]);

            return CreateNewSpectatingConnection(id);
        }

        public enum DataType
        {
            UserState,
            SongInfo,
            FrameData,
        }

        public enum UserState
        {
            SelectingSong,
            Paused,
            Playing,
            Restarting,
            Quitting,
        }

        public class SocketMessage
        {
            public string dataType { get; set; }
        }

        public class SocketUserState : SocketMessage
        {
            public int userState { get; set; }
        }

        public class SocketFrameData : SocketMessage
        {
            public float noteHolder { get; set; }
            public float pointerPosition { get; set; }
            public bool isTooting { get; set; }
        }

        public class SocketSongInfo : SocketMessage
        {
            public string trackRef { get; set; }
            public int songID { get; set; }
            public float gameSpeed { get; set; }
            public float scrollSpeed { get; set; }
        }

        public class SocketDataConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(SocketMessage);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jo = JObject.Load(reader);
                if (jo["dataType"].Value<string>() == DataType.UserState.ToString())
                    return jo.ToObject<SocketUserState>(serializer);

                if (jo["dataType"].Value<string>() == DataType.FrameData.ToString())
                    return jo.ToObject<SocketFrameData>(serializer);

                if (jo["dataType"].Value<string>() == DataType.SongInfo.ToString())
                    return jo.ToObject<SocketSongInfo>(serializer);

                return null;
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            #region patches
            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
            [HarmonyPostfix]
            public static void SetLevelSelectUserStatusOnAdvanceSongs()
            {
                hostedSpectator?.SendUserStateToSocket(UserState.SelectingSong);
            }


            [HarmonyPatch(typeof(GameController), nameof(GameController.startSong))]
            [HarmonyPostfix]
            public static void SetPlayingUserStatus()
            {
                hostedSpectator?.SendUserStateToSocket(UserState.Playing);
            }

            [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.showPausePanel))]
            [HarmonyPostfix]
            public static void OnResumeSetUserStatus()
            {
                hostedSpectator?.SendUserStateToSocket(UserState.Paused);
            }

            [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.resumeFromPause))]
            [HarmonyPostfix]
            public static void OnPauseSetUserStatus()
            {
                hostedSpectator?.SendUserStateToSocket(UserState.Playing);
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.pauseQuitLevel))]
            [HarmonyPostfix]
            public static void OnQuitSetUserStatus()
            {
                hostedSpectator?.SendUserStateToSocket(UserState.Quitting);
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.pauseRetryLevel))]
            [HarmonyPostfix]
            public static void OnRetryingSetUserStatus()
            {
                hostedSpectator?.SendUserStateToSocket(UserState.Restarting);
            }

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPlay))]
            [HarmonyPostfix]
            public static void OnLevelSelectControllerClickPlaySendToSocket(LevelSelectController __instance)
            {
                hostedSpectator.SendSongInfoToSocket(__instance.alltrackslist[__instance.songindex].trackref, 0, ReplaySystemManager.gameSpeedMultiplier, GlobalVariables.gamescrollspeed);
            }
        }
        #endregion
    }
}