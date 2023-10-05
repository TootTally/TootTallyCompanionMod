using Newtonsoft.Json;
using System;
using static TootTally.Spectating.SpectatingManager;
using TootTally.Utils;
using WebSocketSharp;
using System.Collections.Concurrent;

namespace TootTally.Spectating
{
    public class SpectatingSystem : WebsocketManager
    {
        private ConcurrentQueue<SocketFrameData> _receivedFrameDataQueue;
        private ConcurrentQueue<SocketTootData> _receivedTootDataQueue;
        private ConcurrentQueue<SocketNoteData> _receivedNoteDataQueue;
        private ConcurrentQueue<SocketSongInfo> _receivedSongInfoQueue;
        private ConcurrentQueue<SocketUserState> _receivedUserStateQueue;
        private ConcurrentQueue<SocketSpectatorInfo> _receivedSpecInfoQueue;

        public Action<int, SocketFrameData> OnSocketFrameDataReceived;
        public Action<int, SocketTootData> OnSocketTootDataReceived;
        public Action<int, SocketNoteData> OnSocketNoteDataReceived;
        public Action<int, SocketUserState> OnSocketUserStateReceived;
        public Action<int, SocketSongInfo> OnSocketSongInfoReceived;
        public Action<int, SocketSpectatorInfo> OnSocketSpecInfoReceived;

        public SpectatingSystem(int id) : base(id)
        {
            _receivedFrameDataQueue = new ConcurrentQueue<SocketFrameData>();
            _receivedTootDataQueue = new ConcurrentQueue<SocketTootData>();
            _receivedNoteDataQueue = new ConcurrentQueue<SocketNoteData>();
            _receivedSongInfoQueue = new ConcurrentQueue<SocketSongInfo>();
            _receivedUserStateQueue = new ConcurrentQueue<SocketUserState>();
            _receivedSpecInfoQueue = new ConcurrentQueue<SocketSpectatorInfo>();
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

        public void SendNoteData(bool champMode, int multiplier, int noteID, double noteScoreAverage, bool releasedButtonBetweenNotes, int totalScore, float health, int highestCombo)
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

        private static SocketMessage _socketMessage;

        protected override void OnDataReceived(object sender, MessageEventArgs e)
        {
            //TootTallyLogger.LogInfo(e.Data);
            if (e.IsText)
            {
                try
                {
                    _socketMessage = JsonConvert.DeserializeObject<SocketMessage>(e.Data, _dataConverter);
                }
                catch (Exception)
                {
                    TootTallyLogger.LogInfo("Couldn't parse to data: " + e.Data);
                    _socketMessage = null;
                    return;
                }
                if (!IsHosting)
                {
                    if (_socketMessage is SocketSongInfo info)
                        _receivedSongInfoQueue.Enqueue(info);
                    else if (_socketMessage is SocketFrameData frame)
                        _receivedFrameDataQueue.Enqueue(frame);
                    else if (_socketMessage is SocketTootData toot)
                        _receivedTootDataQueue.Enqueue(toot);
                    else if (_socketMessage is SocketUserState state)
                        _receivedUserStateQueue.Enqueue(state);
                    else if (_socketMessage is SocketNoteData note)
                        _receivedNoteDataQueue.Enqueue(note);
                }

                if (_socketMessage is SocketSpectatorInfo spec)
                {
                    TootTallyLogger.LogInfo(e.Data);
                    _receivedSpecInfoQueue.Enqueue(spec);
                }
                //if end up here, nothing was found
                _socketMessage = null;
            }
        }


        public void UpdateStacks()
        {
            if (OnSocketFrameDataReceived != null && _receivedFrameDataQueue.TryDequeue(out SocketFrameData frameData))
                OnSocketFrameDataReceived.Invoke(_id, frameData);

            if (OnSocketTootDataReceived != null && _receivedTootDataQueue.TryDequeue(out SocketTootData tootData))
                OnSocketTootDataReceived.Invoke(_id, tootData);

            if (OnSocketSongInfoReceived != null && _receivedSongInfoQueue.TryDequeue(out SocketSongInfo songInfo))
                OnSocketSongInfoReceived.Invoke(_id, songInfo);

            if (OnSocketUserStateReceived != null && _receivedUserStateQueue.TryDequeue(out SocketUserState userState))
                OnSocketUserStateReceived.Invoke(_id, userState);

            if (OnSocketNoteDataReceived != null && _receivedNoteDataQueue.TryDequeue(out SocketNoteData noteData))
                OnSocketNoteDataReceived.Invoke(_id, noteData);

            if (OnSocketSpecInfoReceived != null && _receivedSpecInfoQueue.TryDequeue(out SocketSpectatorInfo specInfo))
                OnSocketSpecInfoReceived.Invoke(_id, specInfo);

        }

        protected override void OnWebSocketOpen(object sender, EventArgs e)
        {
            PopUpNotifManager.DisplayNotif($"Connected to spectating server.");
            if (!IsHost)
            {
                OnSocketSongInfoReceived = SpectatingManagerPatches.OnSongInfoReceived;
                OnSocketUserStateReceived = SpectatingManagerPatches.OnUserStateReceived;
                OnSocketFrameDataReceived = SpectatingManagerPatches.OnFrameDataReceived;
                OnSocketTootDataReceived = SpectatingManagerPatches.OnTootDataReceived;
                OnSocketNoteDataReceived = SpectatingManagerPatches.OnNoteDataReceived;
                OnSpectatingConnection();
                PopUpNotifManager.DisplayNotif($"Waiting for host to pick a song...");
            }
            else
            {
                OnHostConnection();
                SpectatingManagerPatches.SendCurrentUserState();
            }
            OnSocketSpecInfoReceived = SpectatingManagerPatches.OnSpectatorDataReceived;
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
