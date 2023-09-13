using System;
using System.Collections.Generic;
using TootTally.Utils;
using WebSocketSharp;

namespace TootTally.Replays
{
    public class WebsocketManager
    {
        private const string SPEC_URL = "wss://spec.toottally.com:443/spec/";

        private WebSocket _websocket;
        public bool IsHost { get; private set; }
        public bool IsConnected { get; private set; }
        public bool ConnectionPending { get; private set; }
        public Action<MessageEventArgs> OnMessageReceived;

        public WebsocketManager(int id)
        {
            ConnectionPending = true;
            ConnectToWebSocketServer(id);
        }

        public void SendToSocket(byte[] data)
        {
            _websocket.Send(data);
        }

        public void SendToSocket(string data)
        {
            _websocket.Send(data);
        }

        public void OnDataReceived(object sender, MessageEventArgs e)
        {
            OnMessageReceived?.Invoke(e);
        }

        public void CloseWebsocket()
        {
            TootTallyLogger.LogInfo("Disconnecting from " + _websocket.Url);
            _websocket.Close();
            _websocket = null;
        }

        private void OnWebSocketOpen(object sender, EventArgs e)
        {
            TootTallyLogger.LogInfo($"Connected to WebSocket server {_websocket.Url}");
            IsConnected = true;
            ConnectionPending = false;
        }

        private void OnWebSocketClose(object sender, EventArgs e)
        {
            TootTallyLogger.LogInfo("Disconnected from websocket");
            IsConnected = false;
            IsHost = false;
        }


        public void ConnectToWebSocketServer(int userId)
        {
            _websocket = CreateNewWebSocket(SPEC_URL + userId);
            _websocket.CustomHeaders = new Dictionary<string, string>() { { "Authorization", "APIKey " + Plugin.Instance.APIKey.Value } };
            TootTallyLogger.LogInfo($"Connecting to WebSocket server...");
            IsHost = userId == Plugin.userInfo.id;
            _websocket.ConnectAsync();
        }

        private WebSocket CreateNewWebSocket(string url)
        {
            var ws = new WebSocket(url);
            //if (Plugin.Instance.DebugMode.Value) 
                //ws.Log.Level = LogLevel.Debug; //Too risky since it shows API KEY in the logs
            ws.OnError += (sender, e) => { TootTallyLogger.LogError(e.Message); };
            ws.OnOpen += OnWebSocketOpen;
            ws.OnClose += OnWebSocketClose;
            ws.OnMessage += OnDataReceived;
            ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            return ws;
        }
    }
}
