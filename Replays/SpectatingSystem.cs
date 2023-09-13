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
        private static Stack<SocketFrameData> _receivedFrameDataStack;
        private static Stack<SocketSongInfo> _receivedSongInfoStack;
        private static Stack<SocketUserState> _receivedUserStateStack;
        private static SocketSongInfo _currentSongInfo;
        private static UserState _currentUserState;

        public static Action<SocketFrameData> OnSocketFrameDataReceived;
        public static Action<SocketUserState> OnSocketUserStateReceived;
        public static Action<SocketSongInfo> OnSocketSongInfoReceived;

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

        public void SendFrameData(float noteHolder, float pointerPosition, bool isTooting)
        {
            var json = JsonConvert.SerializeObject(new SocketFrameData() { dataType = DataType.FrameData.ToString(), noteHolder = noteHolder, pointerPosition = pointerPosition, isTooting = isTooting });
            SendToSocket(json);
        }

        protected override void OnDataReceived(object sender, MessageEventArgs e)
        {
            TootTallyLogger.LogInfo(e.Data);
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
                    /*if (FSharpOption<TromboneTrack>.get_IsNone(TrackLookup.tryLookup(_currentSongInfo.trackRef)))
                        ReplaySystemManager.SetTrackToSpectatingTrackref(_currentSongInfo.trackRef);
                    else
                        TootTallyLogger.LogInfo("Do not own the song " + _currentSongInfo.trackRef);*/

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
                }
                else
                {
                    TootTallyLogger.DebugModeLog("Nothing Detected");
                }
            }
        }

        public void UpdateStacks()
        {
            if (_receivedFrameDataStack.TryPop(out SocketFrameData frameData))
                OnSocketFrameDataReceived?.Invoke(frameData);
            if (_receivedSongInfoStack.TryPop(out SocketSongInfo songInfo))
            {
                OnSocketSongInfoReceived?.Invoke(songInfo);
                _currentSongInfo = songInfo;
            }
            if (_receivedUserStateStack.TryPop(out SocketUserState userState))
            {
                OnSocketUserStateReceived?.Invoke(userState);
                _currentUserState = (UserState)userState.userState;
            }

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
