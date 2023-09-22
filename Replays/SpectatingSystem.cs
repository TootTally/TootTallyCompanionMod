using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static TootTally.Replays.SpectatingManager;
using TootTally.Utils;
using WebSocketSharp;
using System.Collections.Concurrent;

namespace TootTally.Replays
{
    public class SpectatingSystem : WebsocketManager
    {
        private ConcurrentQueue<SocketFrameData> _receivedFrameDataStack;
        private ConcurrentQueue<SocketTootData> _receivedTootDataStack;
        private ConcurrentQueue<SocketNoteData> _receivedNoteDataStack;
        private ConcurrentQueue<SocketSongInfo> _receivedSongInfoStack;
        private ConcurrentQueue<SocketUserState> _receivedUserStateStack;

        public Action<int, SocketFrameData> OnSocketFrameDataReceived;
        public Action<int, SocketTootData> OnSocketTootDataReceived;
        public Action<int, SocketNoteData> OnSocketNoteDataReceived;
        public Action<int, SocketUserState> OnSocketUserStateReceived;
        public Action<int, SocketSongInfo> OnSocketSongInfoReceived;

        public SpectatingSystem(int id) : base(id)
        {
            _receivedFrameDataStack = new ConcurrentQueue<SocketFrameData>();
            _receivedTootDataStack = new ConcurrentQueue<SocketTootData>();
            _receivedNoteDataStack = new ConcurrentQueue<SocketNoteData>();
            _receivedSongInfoStack = new ConcurrentQueue<SocketSongInfo>();
            _receivedUserStateStack = new ConcurrentQueue<SocketUserState>();
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

        public void SendNoteData(bool champMode, int multiplier, int noteID, float noteScoreAverage, bool releasedButtonBetweenNotes, int totalScore, float health, int highestCombo)
        {
            var note = new SocketNoteData()
            {
                dataType = DataType.NoteData.ToString(),
                champMode = champMode,
                multiplier = multiplier,
                noteID = noteID,
                noteScoreAverage = noteScoreAverage,
                releasedButtonBetweenNotes = releasedButtonBetweenNotes,
                totalScore = totalScore,
                health = health,
                highestCombo = highestCombo
            };
            var json = JsonConvert.SerializeObject(note);
            SendToSocket(json);
        }

        public void SendFrameData(double time, double noteHolder, float pointerPosition)
        {
            var frame = new SocketFrameData()
            {
                dataType = DataType.FrameData.ToString(),
                time = time,
                noteHolder = noteHolder,
                pointerPosition = pointerPosition,
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
                    _receivedSongInfoStack.Enqueue(socketMessage as SocketSongInfo);
                    TootTallyLogger.LogInfo("SongInfo:" + e.Data);
                }
                else if (socketMessage is SocketFrameData)
                {
                    _receivedFrameDataStack.Enqueue(socketMessage as SocketFrameData);
                }
                else if (socketMessage is SocketTootData)
                {
                    TootTallyLogger.DebugModeLog("TootData Detected");
                    _receivedTootDataStack.Enqueue(socketMessage as SocketTootData);

                }
                else if (socketMessage is SocketUserState)
                {
                    TootTallyLogger.DebugModeLog("UserState Detected");
                    _receivedUserStateStack.Enqueue(socketMessage as SocketUserState);
                    TootTallyLogger.LogInfo("UserState:" + e.Data);
                }
                else if (socketMessage is SocketNoteData)
                {
                    TootTallyLogger.DebugModeLog("NoteData Detected");
                    _receivedNoteDataStack.Enqueue(socketMessage as SocketNoteData);
                }
                else
                {
                    TootTallyLogger.DebugModeLog("Nothing Detected");
                }
            }
        }


        public void UpdateStacks()
        {
            if (OnSocketFrameDataReceived != null && _receivedFrameDataStack.TryDequeue(out SocketFrameData frameData))
                OnSocketFrameDataReceived.Invoke(_id, frameData);

            if (OnSocketTootDataReceived != null && _receivedTootDataStack.TryDequeue(out SocketTootData tootData))
                OnSocketTootDataReceived.Invoke(_id, tootData);

            if (OnSocketSongInfoReceived != null && _receivedSongInfoStack.TryDequeue(out SocketSongInfo songInfo))
                OnSocketSongInfoReceived.Invoke(_id, songInfo);

            if (OnSocketUserStateReceived != null && _receivedUserStateStack.TryDequeue(out SocketUserState userState))
                OnSocketUserStateReceived.Invoke(_id, userState);

            if (OnSocketNoteDataReceived != null && _receivedNoteDataStack.TryDequeue(out SocketNoteData noteData))
                OnSocketNoteDataReceived.Invoke(_id, noteData);

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
                OnSocketNoteDataReceived = SpectatorManagerPatches.OnNoteDataReceived;
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
