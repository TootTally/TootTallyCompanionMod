using BaboonAPI.Hooks.Tracks;
using Microsoft.FSharp.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Channels;
using TootTally.CustomLeaderboard;
using TootTally.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.AI;
using UnityEngine.Networking;
using UnityEngine.Playables;
using UnityEngine.UIElements;
using WebSocketSharp;

namespace TootTally.Replays
{
    public static class SpectatingManager
    {
        public static JsonConverter[] _dataConverter = new JsonConverter[] { new SocketDataConverter() };
        private static List<SpectatingSystem> _spectatingSystemList;


        public static SpectatingSystem CreateNewSpectatingConnection(int id)
        {
            _spectatingSystemList ??= new List<SpectatingSystem>();
            var spec = new SpectatingSystem(id);
            _spectatingSystemList.Add(spec);
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
                return (objectType == typeof(SocketMessage));
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
        }
    }
}
