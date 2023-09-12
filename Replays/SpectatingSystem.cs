using BaboonAPI.Hooks.Tracks;
using Microsoft.FSharp.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TootTally.Replays.SpectatingManager;
using TootTally.Utils;
using UnityEngine.Playables;
using WebSocketSharp;
using TMPro;

namespace TootTally.Replays
{
    public class SpectatingSystem
    {
        private WebsocketManager _websocketManager;

        private static List<SocketFrameData> _receivedFrameData;
        private static List<SocketSongInfo> _receivedSongInfo;
        private static List<SocketUserState> _receivedUserState;
        private static SocketSongInfo _currentSongInfo;
        private static UserState _currentUserState;

        public static Action<SocketFrameData> OnSocketFrameDataReceiveStack;
        public static Action<SocketUserState> OnSocketUserStateReceiveStack;
        public static Action<SocketSongInfo> OnSocketSongInfoStack;
        public bool GetIsHost => _websocketManager.IsHost;

        public SpectatingSystem(int id)
        {
            _receivedFrameData = new List<SocketFrameData>();
            _receivedSongInfo = new List<SocketSongInfo>();
            _receivedUserState = new List<SocketUserState>();
            _websocketManager = new WebsocketManager(id);
        }

        public void SendSongInfoToSocket(string trackRef, int id, float gameSpeed, float scrollSpeed)
        {
            var json = JsonConvert.SerializeObject(new SocketSongInfo() { dataType = DataType.SongInfo.ToString(), trackRef = trackRef, songID = id, gameSpeed = gameSpeed, scrollSpeed = scrollSpeed });
            _websocketManager?.SendToSocket(json);
        }

        public void SendUserStateToSocket(UserState userState)
        {
            var json = JsonConvert.SerializeObject(new SocketUserState() { dataType = DataType.UserState.ToString(), userState = (int)userState });
            _websocketManager?.SendToSocket(json);
        }

        public void SendFrameData(float noteHolder, float pointerPosition, bool isTooting)
        {
            var json = JsonConvert.SerializeObject(new SocketFrameData() { dataType = DataType.FrameData.ToString(), noteHolder = noteHolder, pointerPosition = pointerPosition, isTooting = isTooting });
            _websocketManager?.SendToSocket(json);
        }

        public void OnDataReceived(MessageEventArgs e)
        {
            TootTallyLogger.LogInfo(e.Data);
            if (e.IsText && !_websocketManager.IsHost)
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
                    _receivedSongInfo.Add(socketMessage as SocketSongInfo);
                    /*if (FSharpOption<TromboneTrack>.get_IsNone(TrackLookup.tryLookup(_currentSongInfo.trackRef)))
                        ReplaySystemManager.SetTrackToSpectatingTrackref(_currentSongInfo.trackRef);
                    else
                        TootTallyLogger.LogInfo("Do not own the song " + _currentSongInfo.trackRef);*/

                }
                else if (socketMessage is SocketFrameData)
                {
                    TootTallyLogger.DebugModeLog("FrameData Detected");
                    _receivedFrameData.Add(socketMessage as SocketFrameData);
                }
                else if (socketMessage is SocketUserState)
                {
                    TootTallyLogger.DebugModeLog("UserState Detected");
                    _receivedUserState.Add(socketMessage as SocketUserState);
                }
                else
                {
                    TootTallyLogger.DebugModeLog("Nothing Detected");
                }
            }
        }

        public void UpdateStacks()
        {
            
        }

        public void Disconnect()
        {
            if (_websocketManager.IsConnected)
                _websocketManager.Disconnect();
        }

        public void RemoveFromManager()
        {
            SpectatingManager.RemoveSpectator(this);
        }

    }
}
