﻿using Newtonsoft.Json;
using System;
using TootTally.Utils;
using WebSocketSharp;
using System.Collections.Concurrent;
using static TootTally.Spectating.SpectatingManager;

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

        //int ID is for future tournament host so you can sort data when receiving it :)
        public Action<SocketFrameData> OnSocketFrameDataReceived;
        public Action<SocketTootData> OnSocketTootDataReceived;
        public Action<SocketNoteData> OnSocketNoteDataReceived;
        public Action<SocketUserState> OnSocketUserStateReceived;
        public Action<SocketSongInfo> OnSocketSongInfoReceived;
        public Action<SocketSpectatorInfo> OnSocketSpecInfoReceived;

        public Action<SpectatingSystem> OnWebSocketOpenCallback;

        public int GetSpectatorUserId => _id;
        public string spectatorName;

        public SpectatingSystem(int id, string name) : base(id)
        {
            spectatorName = name;
            _receivedFrameDataQueue = new ConcurrentQueue<SocketFrameData>();
            _receivedTootDataQueue = new ConcurrentQueue<SocketTootData>();
            _receivedNoteDataQueue = new ConcurrentQueue<SocketNoteData>();
            _receivedSongInfoQueue = new ConcurrentQueue<SocketSongInfo>();
            _receivedUserStateQueue = new ConcurrentQueue<SocketUserState>();
            _receivedSpecInfoQueue = new ConcurrentQueue<SocketSpectatorInfo>();
        }

        public void SendSongInfoToSocket(string trackRef, int id, float gameSpeed, float scrollSpeed, string gamemodifiers)
        {
            var json = JsonConvert.SerializeObject(new SocketSongInfo() { dataType = DataType.SongInfo.ToString(), trackRef = trackRef, songID = id, gameSpeed = gameSpeed, scrollSpeed = scrollSpeed, gamemodifiers = gamemodifiers });
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

        public void SendTootData(double time, double noteHolder, bool isTooting)
        {
            var tootFrame = new SocketTootData()
            {
                dataType = DataType.TootData.ToString(),
                time = time,
                noteHolder = noteHolder,
                isTooting = isTooting
            };
            var json = JsonConvert.SerializeObject(tootFrame);
            SendToSocket(json);
        }

        private static SocketMessage _socketMessage;

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

        public void RemoveFromManager()
        {
            RemoveSpectator(this);
        }

    }
}