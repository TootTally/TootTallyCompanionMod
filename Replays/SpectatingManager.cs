using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Channels;
using TootTally.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.AI;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using WebSocketSharp;
using static Mono.Security.X509.X520;

namespace TootTally.Replays
{
    public static class SpectatingManager
    {
        private const string SPEC_URL = "wss://spec.toottally.com:443/spec/";
        private static WebSocket _websocket;
        private static JsonConverter[] _dataConverter = new JsonConverter[] { new SocketDataConverter() };
        public static bool IsHost;
        public static bool IsConnected;

        public static void OpenNewWebSocketConnection()
        {
            _websocket = CreateNewWebSocket(SPEC_URL + Plugin.userInfo.id);
            IsHost = true;
            _websocket.CustomHeaders = new Dictionary<string, string>() { { "Authorization", "APIKey " + Plugin.Instance.APIKey.Value } };
            TootTallyLogger.LogInfo($"Connecting to WebSocket server...");
            _websocket.ConnectAsync();
        }

        public static void SendToSocket(byte[] data)
        {
            _websocket.Send(data);
        }

        public static void SendToSocket(string data)
        {
            _websocket.Send(data);
        }

        public static void SendSongInfoToSocket(string trackRef, int id)
        {
            var json = JsonConvert.SerializeObject(new SocketSongInfo() { dataType = DataType.SongInfo.ToString(), trackRef = trackRef, songID = id });
            SendToSocket(json);
        }

        public static void SendUserStateToSocket(UserState userState)
        {
            var json = JsonConvert.SerializeObject(new SocketUserState() { dataType = DataType.UserState.ToString(),  userState = userState.ToString() });
            SendToSocket(json);
        }

        public static void SendFrameData(float noteHolder, float pointerPosition, bool isTooting)
        {
            var json = JsonConvert.SerializeObject(new SocketFrameData() { dataType = DataType.FrameData.ToString(), noteHolder = noteHolder, pointerPosition = pointerPosition, isTooting = isTooting });
            SendToSocket(json);
        }

        public static void OnDataReceived(object sender, MessageEventArgs e)
        {
            TootTallyLogger.LogInfo(e.Data);
            if (e.IsText)
            {
                var socketMessage = JsonConvert.DeserializeObject<SocketMessage>(e.Data, _dataConverter);
                if (socketMessage is SocketSongInfo)
                {
                    TootTallyLogger.DebugModeLog("SongInfo Detected");
                }
                else if (socketMessage is SocketFrameData)
                {
                    TootTallyLogger.DebugModeLog("FrameData Detected");
                }
                else if (socketMessage is SocketUserState)
                {
                    TootTallyLogger.DebugModeLog("Userstate Detected");
                }
                else
                {
                    TootTallyLogger.DebugModeLog("Nothing Detected");
                }
            }

        }

        public static void OnWebSocketOpen(object sender, EventArgs e)
        {
            TootTallyLogger.LogInfo($"Connected to WebSocket server {_websocket.Url}");
            IsConnected = true;
        }

        public static void OnWebSocketClose(object sender, EventArgs e)
        {
            TootTallyLogger.LogInfo("Disconnected from websocket");
            IsConnected = false;
            IsHost = false;
        }

        public static void Disconnect()
        {
            TootTallyLogger.LogInfo("Disconnecting from " + _websocket.Url);
            _websocket.Close();
            _websocket = null;
        }

        public static void ConnectToWebSocketServer(int userId)
        {
            _websocket = CreateNewWebSocket(SPEC_URL + userId);
            _websocket.CustomHeaders = new Dictionary<string, string>() { { "Authorization", "APIKey " + Plugin.Instance.APIKey.Value } };
            TootTallyLogger.LogInfo($"Connecting to WebSocket server...");
            IsHost = userId == Plugin.userInfo.id;
            _websocket.ConnectAsync();
        }

        private static WebSocket CreateNewWebSocket(string url)
        {
            var ws = new WebSocket(url);
            ws.Log.Level = LogLevel.Debug;
            ws.OnError += (sender, e) => { TootTallyLogger.LogError(e.Message); };
            ws.OnOpen += OnWebSocketOpen;
            ws.OnClose += OnWebSocketClose;
            ws.OnMessage += OnDataReceived;
            ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            return ws;
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
            public string userState { get; set; }
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
