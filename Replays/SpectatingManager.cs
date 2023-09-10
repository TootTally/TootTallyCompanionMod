using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Channels;
using TootTally.Utils;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
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
            _websocket.CustomHeaders = new Dictionary<string, string>() { { "Authorization", "APIKey " + Plugin.userInfo.api_key } };
            _websocket.Connect();
        }

        public static void SendToSocket(byte[] data)
        {
            _websocket.Send(data);
        }

        public static void OnDataReceived(object sender, MessageEventArgs e)
        {
            PopUpNotifManager.DisplayNotif("Test");
        }

        public static void OnWebSocketOpen(object sender, EventArgs e)
        {
            TootTallyLogger.LogInfo("LetsCrash");
            PopUpNotifManager.DisplayNotif("YouShouldHaveCrashedLoL");
        }

        public static void ConnectToWebSocketServer(int userId)
        {
            _websocket = CreateNewWebSocket(SPEC_URL +  userId);
            _websocket.Connect();
        }

        private static WebSocket CreateNewWebSocket(string url)
        {
            var ws = new WebSocket(url);
            TootTallyLogger.LogInfo($"Connect to WebSocket server: {ws.Url}");
            ws.OnError += (sender, e) => { TootTallyLogger.LogError(e.Message); };
            ws.OnOpen += OnWebSocketOpen;
            ws.OnMessage += OnDataReceived;
            ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            return ws;
        }
    }
}
