using System;
using System.Collections.Generic;
using TootTally.Utils;
using WebSocketSharp;

namespace TootTally.Spectating
{
    public class WebsocketManager
    {
        private const string SPEC_URL = "wss://spec.toottally.com:443/spec/";
        private const string VERSION = "1.3.0";

        private WebSocket _websocket;
        public bool IsHost { get; private set; }
        public bool IsConnected { get; private set; }
        public bool ConnectionPending { get; private set; }
        protected int _id { get; private set; }

        public WebsocketManager(int id)
        {
            _id = id;
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

        protected virtual void OnDataReceived(object sender, MessageEventArgs e) { }

        protected virtual void CloseWebsocket()
        {
            _websocket.Close();
            TootTallyLogger.LogInfo("Disconnecting from " + _websocket.Url);
            _websocket = null;
        }

        protected virtual void OnWebSocketOpen(object sender, EventArgs e)
        {
            IsConnected = true;
            ConnectionPending = false;
            TootTallyLogger.LogInfo($"Connected to WebSocket server {_websocket.Url}");
        }

        protected virtual void OnWebSocketClose(object sender, EventArgs e)
        {
            IsConnected = false;
            IsHost = false;
            ConnectionPending = false;
            TootTallyLogger.LogInfo("Disconnected from websocket");
        }


        public void ConnectToWebSocketServer(int userId)
        {
            _websocket = CreateNewWebSocket(SPEC_URL + userId);
            _websocket.CustomHeaders = new Dictionary<string, string>() { { "Authorization", "APIKey " + Plugin.Instance.APIKey.Value }, { "Version", VERSION } };
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
