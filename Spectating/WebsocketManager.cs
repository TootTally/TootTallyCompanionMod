using System;
using System.Collections.Generic;
using TootTally.Utils;
using WebSocketSharp;

namespace TootTally.Spectating
{
    public class WebsocketManager
    {
        private const string VERSION = "1.3.0";

        private WebSocket _websocket;
        public bool IsHost { get; private set; }
        public bool IsConnected { get; private set; }
        public bool ConnectionPending { get; protected set; }

        protected string _id { get; private set; }
        protected string _url { get; private set; }
        protected string _version { get; private set; }

        public WebsocketManager(string id, string url, string version)
        {
            _url = url;
            _id = id;
            _version = version;
        }

        public void SendToSocket(byte[] data)
        {
            _websocket.SendAsync(data, delegate { });
        }

        public void SendToSocket(string data)
        {
            _websocket.SendAsync(data, delegate { });
        }

        protected virtual void OnDataReceived(object sender, MessageEventArgs e) { }

        protected virtual void CloseWebsocket()
        {
            _websocket.CloseAsync();
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


        public void ConnectToWebSocketServer(string url, bool isHost)
        {
            _websocket = CreateNewWebSocket(url);
            _websocket.CustomHeaders = new Dictionary<string, string>() { { "Authorization", "APIKey " + Plugin.Instance.APIKey.Value }, { "Version", VERSION } };
            TootTallyLogger.LogInfo($"Connecting to WebSocket server...");
            IsHost = isHost;
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
