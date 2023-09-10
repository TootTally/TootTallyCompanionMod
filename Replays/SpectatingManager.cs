using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Channels;
using TootTally.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
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

        public static void OpenNewWebSocketConnection()
        {
            _websocket = CreateNewWebSocket(SPEC_URL + Plugin.userInfo.id);
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
            TootTallyLogger.LogInfo(data);
            _websocket.Send(data);
        }

        public static void SendSongInfoToSocket(string trackRef, int id)
        {
            var json = JsonConvert.SerializeObject(new SocketMessage() { dataType = DataType.songInfo.ToString(), data = new SocketSongData() { trackRef = trackRef, songID = id } });
            _websocket.Send(json);
        }

        public static void OnDataReceived(object sender, MessageEventArgs e)
        {
            TootTallyLogger.LogInfo(e.Data);
        }

        public static void OnWebSocketOpen(object sender, EventArgs e)
        {
            TootTallyLogger.LogInfo($"Connected to WebSocket server {_websocket.Url}");
        }

        public static void ConnectToWebSocketServer(int userId)
        {
            _websocket = CreateNewWebSocket(SPEC_URL + userId);
            _websocket.CustomHeaders = new Dictionary<string, string>() { { "Authorization", "APIKey " + Plugin.Instance.APIKey.Value } };
            TootTallyLogger.LogInfo($"Connecting to WebSocket server...");
            _websocket.ConnectAsync();
        }

        private static WebSocket CreateNewWebSocket(string url)
        {
            var ws = new WebSocket(url);
            ws.Log.Level = LogLevel.Debug;
            ws.OnError += (sender, e) => { TootTallyLogger.LogError(e.Message); };
            ws.OnOpen += OnWebSocketOpen;
            ws.OnMessage += OnDataReceived;
            ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            return ws;
        }

        public enum DataType
        {
            userState,
            songInfo,
            frameData,
            
        }

        public enum UserState
        {
            SelectingSong,
            Paused,
            Playing,
            Restarting,
            Quitting,
        }

        [Serializable]
        public class SocketMessage
        {
            public string dataType { get; set; }
            public ISocketData data { get; set; }

        }

        public class SocketUserState : ISocketData
        {
            string userState { get; set; }
        }

        public class SocketFrameData : ISocketData
        {
            //TODO
        }

        public class SocketSongData : ISocketData
        {
            public string trackRef { get; set; }
            public int songID { get; set; }
        }

        public interface ISocketData { }
    }
}
