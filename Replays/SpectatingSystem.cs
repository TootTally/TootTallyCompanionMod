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
        private Stack<SocketTootData> _receivedTootDataStack;
        private Stack<SocketSongInfo> _receivedSongInfoStack;
        private Stack<SocketUserState> _receivedUserStateStack;

        public Action<int, SocketFrameData> OnSocketFrameDataReceived;
        public Action<int, SocketTootData> OnSocketTootDataReceived;
        public Action<int, SocketUserState> OnSocketUserStateReceived;
        public Action<int, SocketSongInfo> OnSocketSongInfoReceived;

        public SpectatingSystem(int id) : base(id)
        {
            _receivedFrameDataStack = new Stack<SocketFrameData>();
            _receivedTootDataStack = new Stack<SocketTootData>();
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

        public void SendFrameData(double time, double noteHolder, float pointerPosition, int totalScore, int highestCombo, int currentCombo, float health)
        {
            var frame = new SocketFrameData()
            {
                dataType = DataType.FrameData.ToString(),
                time = time,
                noteHolder = noteHolder,
                pointerPosition = pointerPosition,
                totalScore = totalScore,
                highestCombo = highestCombo,
                currentCombo = currentCombo,
                health = health,
            };
            var json = JsonConvert.SerializeObject(frame);
            SendToSocket(json);
        }

        public void SendTootData(double noteHolder, bool isTooting)
        {
            var tootFrame = new SocketTootData()
            {
                dataType = DataType.TootData.ToString(),
                noteHolder = noteHolder,
                isTooting = isTooting
            };
            var json = JsonConvert.SerializeObject(tootFrame);
            SendToSocket(json);
        }

        protected override void OnDataReceived(object sender, MessageEventArgs e)
        {
            //TootTallyLogger.LogInfo(e.Data);
            if (e.IsText && !IsHosting)
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
                    TootTallyLogger.LogInfo("SongInfo:" + e.Data);
                }
                else if (socketMessage is SocketFrameData)
                {
                    _receivedFrameDataStack.Push(socketMessage as SocketFrameData);
                }
                else if (socketMessage is SocketTootData)
                {
                    TootTallyLogger.DebugModeLog("TootData Detected");
                    _receivedTootDataStack.Push(socketMessage as SocketTootData);

                }
                else if (socketMessage is SocketUserState)
                {
                    TootTallyLogger.DebugModeLog("UserState Detected");
                    _receivedUserStateStack.Push(socketMessage as SocketUserState);
                    TootTallyLogger.LogInfo("UserState:" + e.Data);
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

            if (OnSocketTootDataReceived != null && _receivedTootDataStack.TryPop(out SocketTootData tootData))
                OnSocketTootDataReceived.Invoke(_id, tootData);

            if (OnSocketSongInfoReceived != null && _receivedSongInfoStack.TryPop(out SocketSongInfo songInfo))
                OnSocketSongInfoReceived.Invoke(_id, songInfo);

            if (OnSocketUserStateReceived != null && _receivedUserStateStack.TryPop(out SocketUserState userState))
                OnSocketUserStateReceived.Invoke(_id, userState);

        }

        protected override void OnWebSocketOpen(object sender, EventArgs e)
        {
            PopUpNotifManager.DisplayNotif($"Connected to spectating server.");
            if (!IsHost)
            {
                OnSocketSongInfoReceived = SpectatorManagerPatches.OnSongInfoReceived;
                OnSocketUserStateReceived = SpectatorManagerPatches.OnUserStateReceived;
                OnSocketFrameDataReceived = SpectatorManagerPatches.OnFrameDataReceived;
                OnSocketTootDataReceived = SpectatorManagerPatches.OnTootDataReceived;
                PopUpNotifManager.DisplayNotif($"Waiting for host to pick a song...");
            }
            else
                SpectatorManagerPatches.SendCurrentUserState();
            base.OnWebSocketOpen(sender, e);
        }

        public void Disconnect()
        {
            if (!IsHost)
                PopUpNotifManager.DisplayNotif($"Disconnected from Spectating server.");
            if (IsConnected)
                CloseWebsocket();
        }

        public void RemoveFromManager()
        {
            RemoveSpectator(this);
        }

    }
}
