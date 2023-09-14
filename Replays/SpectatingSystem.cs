using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static TootTally.Replays.SpectatingManager;
using TootTally.Utils;
using WebSocketSharp;
using UnityEngine.UIElements;

namespace TootTally.Replays
{
    public class SpectatingSystem : WebsocketManager
    {
        private Stack<SocketFrameData> _receivedFrameDataStack;
        private Stack<SocketSongInfo> _receivedSongInfoStack;
        private Stack<SocketUserState> _receivedUserStateStack;
        private UserState _currentUserState;

        public Action<int, SocketFrameData> OnSocketFrameDataReceived;
        public Action<int, SocketUserState> OnSocketUserStateReceived;
        public Action<int, SocketSongInfo> OnSocketSongInfoReceived;

        public SpectatingSystem(int id) : base(id)
        {
            _receivedFrameDataStack = new Stack<SocketFrameData>();
            _receivedSongInfoStack = new Stack<SocketSongInfo>();
            _receivedUserStateStack = new Stack<SocketUserState>();
        }

        public void SendSongInfoToSocket(string trackRef, int id, float gameSpeed, float scrollSpeed)
        {
            var json = JsonConvert.SerializeObject(new SocketSongInfo() { dataType = DataType.SongInfo.ToString(), trackRef = trackRef, songID = id, gameSpeed = gameSpeed, scrollSpeed = scrollSpeed });
            SendToSocket(json);
        }

        public void SendUserStateToSocket(UserState userState)
        {
            var json = JsonConvert.SerializeObject(new SocketUserState() { dataType = DataType.UserState.ToString(), userState = (int)userState });
            SendToSocket(json);
        }

        public void SendFrameData(float time, float noteHolder, float pointerPosition, int totalScore, bool isTooting)
        {
            var json = JsonConvert.SerializeObject(new SocketFrameData() { dataType = DataType.FrameData.ToString(), time = time, noteHolder = noteHolder, pointerPosition = pointerPosition, totalScore = totalScore, isTooting = isTooting });
            SendToSocket(json);
        }

        protected override void OnDataReceived(object sender, MessageEventArgs e)
        {
            //TootTallyLogger.LogInfo(e.Data);
            if (e.IsText)
            {
                SocketMessage socketMessage;
                try
                {
                    socketMessage = JsonConvert.DeserializeObject<SocketMessage>(e.Data, _dataConverter);
                }
                catch (Exception)
                {
                    TootTallyLogger.LogInfo("Couldn't parse to data.");
                    TootTallyLogger.LogInfo("Raw message: " + e.Data);
                    return;
                }

                if (socketMessage is SocketSongInfo)
                {
                    TootTallyLogger.DebugModeLog("SongInfo Detected");
                    _receivedSongInfoStack.Push(socketMessage as SocketSongInfo);
                }
                else if (socketMessage is SocketFrameData)
                {
                    TootTallyLogger.DebugModeLog("FrameData Detected");
                    _receivedFrameDataStack.Push(socketMessage as SocketFrameData);
                }
                else if (socketMessage is SocketUserState)
                {
                    TootTallyLogger.DebugModeLog("UserState Detected");
                    _receivedUserStateStack.Push(socketMessage as SocketUserState);
                    _currentUserState = (UserState)(socketMessage as SocketUserState).userState;
                }
                else
                {
                    TootTallyLogger.DebugModeLog("Nothing Detected");
                }
            }
        }

        public void UpdateStacks()
        {
            if (OnSocketFrameDataReceived != null && _receivedFrameDataStack.TryPop(out SocketFrameData frameData))
                OnSocketFrameDataReceived.Invoke(_id, frameData);

            if (OnSocketSongInfoReceived != null && _receivedSongInfoStack.TryPop(out SocketSongInfo songInfo))
                OnSocketSongInfoReceived.Invoke(_id, songInfo);

            if (OnSocketUserStateReceived != null && _receivedUserStateStack.TryPop(out SocketUserState userState))
                OnSocketUserStateReceived.Invoke(_id, userState);

        }

        protected override void OnWebSocketOpen(object sender, EventArgs e)
        {
            if (!IsHost)
            {
                OnSocketSongInfoReceived = SpectatorManagerPatches.OnSongInfoReceived;
                OnSocketFrameDataReceived = SpectatorManagerPatches.OnFrameDataReceived;
            }
            base.OnWebSocketOpen(sender, e);
        }

        public void Disconnect()
        {
            if (IsConnected)
                CloseWebsocket();
        }

        public void RemoveFromManager()
        {
            RemoveSpectator(this);
        }

    }
}
