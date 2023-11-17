using Newtonsoft.Json;
using System;
using TootTally.Utils;
using WebSocketSharp;
using System.Collections.Concurrent;
using static TootTally.Spectating.SpectatingManager;
using System.Security.Policy;

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

        public Action<SocketFrameData> OnSocketFrameDataReceived;
        public Action<SocketTootData> OnSocketTootDataReceived;
        public Action<SocketNoteData> OnSocketNoteDataReceived;
        public Action<SocketUserState> OnSocketUserStateReceived;
        public Action<SocketSongInfo> OnSocketSongInfoReceived;
        public Action<SocketSpectatorInfo> OnSocketSpecInfoReceived;

        public Action<SpectatingSystem> OnWebSocketOpenCallback;

        public string GetSpectatorUserId => _id;
        public string spectatorName;

        public SpectatingSystem(int id, string name) : base(id.ToString(), "wss://spec.toottally.com:443/spec/", "1.3.0")
        {
            spectatorName = name;
            _receivedFrameDataQueue = new ConcurrentQueue<SocketFrameData>();
            _receivedTootDataQueue = new ConcurrentQueue<SocketTootData>();
            _receivedNoteDataQueue = new ConcurrentQueue<SocketNoteData>();
            _receivedSongInfoQueue = new ConcurrentQueue<SocketSongInfo>();
            _receivedUserStateQueue = new ConcurrentQueue<SocketUserState>();
            _receivedSpecInfoQueue = new ConcurrentQueue<SocketSpectatorInfo>();

            ConnectionPending = true;
            ConnectToWebSocketServer(_url + id, id == Plugin.userInfo.id);
        }

        public void SendSongInfoToSocket(string trackRef, int id, float gameSpeed, float scrollSpeed, string gamemodifiers)
        {
            var json = JsonConvert.SerializeObject(new SocketSongInfo() { dataType = DataType.SongInfo.ToString(), trackRef = trackRef, songID = id, gameSpeed = gameSpeed, scrollSpeed = scrollSpeed, gamemodifiers = gamemodifiers });
            SendToSocket(json);
        }

        public void SendSongInfoToSocket(SocketSongInfo songInfo)
        {
            songInfo.dataType = DataType.SongInfo.ToString();
            SendToSocket(JsonConvert.SerializeObject(songInfo));
        }


        public void SendUserStateToSocket(UserState userState)
        {
            var json = JsonConvert.SerializeObject(new SocketUserState() { dataType = DataType.UserState.ToString(), userState = (int)userState });
            SendToSocket(json);
        }

        private SocketNoteData _socketNoteDataHolder = new SocketNoteData();
        public void SendNoteData(bool champMode, int multiplier, int noteID, double noteScoreAverage, bool releasedButtonBetweenNotes, int totalScore, float health, int highestCombo)
        {
            _socketNoteDataHolder.dataType = DataType.NoteData.ToString();
            _socketNoteDataHolder.champMode = champMode;
            _socketNoteDataHolder.multiplier = multiplier;
            _socketNoteDataHolder.noteID = noteID;
            _socketNoteDataHolder.noteScoreAverage = noteScoreAverage;
            _socketNoteDataHolder.releasedButtonBetweenNotes = releasedButtonBetweenNotes;
            _socketNoteDataHolder.totalScore = totalScore;
            _socketNoteDataHolder.health = health;
            _socketNoteDataHolder.highestCombo = highestCombo;

            var json = JsonConvert.SerializeObject(_socketNoteDataHolder);
            SendToSocket(json);
        }

        private SocketFrameData _socketFrameDataHolder = new SocketFrameData();
        public void SendFrameData(double time, double noteHolder, float pointerPosition)
        {
            _socketFrameDataHolder.dataType = DataType.FrameData.ToString();
            _socketFrameDataHolder.time = time;
            _socketFrameDataHolder.noteHolder = noteHolder;
            _socketFrameDataHolder.pointerPosition = pointerPosition;
            var json = JsonConvert.SerializeObject(_socketFrameDataHolder);
            SendToSocket(json);
        }

        private SocketTootData _socketTootDataHolder = new SocketTootData();
        public void SendTootData(double time, double noteHolder, bool isTooting)
        {
            _socketTootDataHolder.dataType = DataType.TootData.ToString();
            _socketTootDataHolder.time = time;
            _socketTootDataHolder.noteHolder = noteHolder;
            _socketTootDataHolder.isTooting = isTooting;
            var json = JsonConvert.SerializeObject(_socketTootDataHolder);
            SendToSocket(json);
        }

        private SocketMessage _socketMessage;

        protected override void OnDataReceived(object sender, MessageEventArgs e)
        {
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
                if (!IsHost)
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
            if (ConnectionPending) return;

            if (OnSocketFrameDataReceived != null && _receivedFrameDataQueue.TryDequeue(out SocketFrameData frameData))
                OnSocketFrameDataReceived.Invoke(frameData);

            if (OnSocketTootDataReceived != null && _receivedTootDataQueue.TryDequeue(out SocketTootData tootData))
                OnSocketTootDataReceived.Invoke(tootData);

            if (OnSocketSongInfoReceived != null && _receivedSongInfoQueue.TryDequeue(out SocketSongInfo songInfo))
                OnSocketSongInfoReceived.Invoke(songInfo);

            if (OnSocketUserStateReceived != null && _receivedUserStateQueue.TryDequeue(out SocketUserState userState))
                OnSocketUserStateReceived.Invoke(userState);

            if (OnSocketNoteDataReceived != null && _receivedNoteDataQueue.TryDequeue(out SocketNoteData noteData))
                OnSocketNoteDataReceived.Invoke(noteData);

            if (OnSocketSpecInfoReceived != null && _receivedSpecInfoQueue.TryDequeue(out SocketSpectatorInfo specInfo))
                OnSocketSpecInfoReceived.Invoke(specInfo);

        }

        protected override void OnWebSocketOpen(object sender, EventArgs e)
        {
            PopUpNotifManager.DisplayNotif($"Connected to spectating server.");
            OnWebSocketOpenCallback?.Invoke(this);                   
            base.OnWebSocketOpen(sender, e);
        }

        public void Disconnect()
        {
            if (!IsHost)
                PopUpNotifManager.DisplayNotif($"Disconnected from Spectating server.");
            if (IsConnected)
                CloseWebsocket();
        }

        public void CancelConnection()
        {
            CloseWebsocket();
        }

        public void RemoveFromManager()
        {
            RemoveSpectator(this);
        }

    }
}
