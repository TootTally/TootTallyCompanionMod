using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using Newtonsoft.Json;
using TootTally.GameplayModifier;
using TootTally.Graphics;
using TootTally.Utils;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using TrombLoader.CustomTracks;
using UnityEngine;
using UnityEngine.Playables;
using static TootTally.Utils.APIServices.SerializableClass;

namespace TootTally.Replays
{
    public class NewReplaySystemV2
    {
        public static List<string> incompatibleReplayVersions = new List<string> { "1.0.0" };
        public const string REPLAY_VERSION = "2.0.0";

        private int _frameIndex, _tootIndex;

        private ReplayData _replayData;
        private FrameData _lastFrame, _currentFrame;
        private TootData _currentToot;
        private NoteData _currentNote;

        public float GetReplaySpeed { get => _replayData.gamespeedmultiplier; }

        private bool _wasTouchScreenUsed;
        private bool _wasTabletUsed;
        private bool _isTooting;
        private int _maxCombo;
        public bool GetIsTooting { get => _currentToot.O == 1; }

        public string GetUsername { get => _replayData.username; }
        public string GetSongName { get => _replayData.song; }

        public bool IsFullCombo { get => GlobalVariables.gameplay_notescores[0] == 0 && GlobalVariables.gameplay_notescores[1] == 0 && GlobalVariables.gameplay_notescores[2] == 0; }
        public bool IsTripleS { get => GlobalVariables.gameplay_notescores[0] == 0 && GlobalVariables.gameplay_notescores[1] == 0 && GlobalVariables.gameplay_notescores[2] == 0 && GlobalVariables.gameplay_notescores[3] == 0; }

        #region ReplayRecorder

        public void SetupRecording(int targetFramerate)
        {
            _replayData = new ReplayData(targetFramerate);
            TootTallyLogger.LogInfo("Started recording replay");
        }

        public void SetStartTime()
        {
            _replayData.SetStartTime();
            TootTallyLogger.LogInfo($"Replay started recording at {_replayData.starttime}");
        }

        public void SetEndTime()
        {
            _replayData.SetEndTime();
            TootTallyLogger.LogInfo($"Replay recording finished at {_replayData.endtime}");
        }

        public void SetUsernameAndSongName(string username, string songname)
        {
            _replayData.username = username;
            _replayData.song = songname;
        }

        public void RecordFrameData(GameController __instance, float time, float noteHolderPosition)
        {
            if (Input.touchCount > 0) _wasTouchScreenUsed = true;

            _currentFrame = new FrameData(time, noteHolderPosition, __instance.pointer.transform.localPosition.y, Input.mousePosition.x, Input.mousePosition.y);

            if (_currentFrame.P != _lastFrame.P && _currentFrame.N != _lastFrame.N && _currentFrame.MY != _lastFrame.MY)
            {
                _replayData.framedata.Add(_currentFrame);
                _lastFrame = _currentFrame;
            }

        }

        public void RecordNoteDataPostfix(GameController __instance)
        {
            _currentNote = new NoteData(__instance.notescoreaverage, __instance.released_button_between_notes ? 1 : 0);
        }

        public void RecordNoteDataPrefix(GameController __instance)
        {
            _currentNote.PostFixConstructor(__instance.currentnoteindex, __instance.highestcombocounter, __instance.multiplier, __instance.totalscore, __instance.currenthealth, __instance.highestcombo_level,
                new int[] {__instance.scores_A, __instance.scores_B, __instance.scores_C, __instance.scores_D, __instance.scores_F, });
            _replayData.notedata.Add(_currentNote);
        }


        public void RecordToot(float time, float noteHolderPosition, bool isTooting)
        {
            _replayData.tootdata.Add(new TootData(time, noteHolderPosition, isTooting ? 1 : 0));
        }

        public string GetRecordedReplayJson(string uuid)
        {
            _replayData.uuid = uuid;
            _replayData.input = GetInputTypeString();
            _replayData.finalscore = GlobalVariables.gameplay_scoretotal;
            _replayData.maxcombo = _maxCombo;
            _replayData.finalnotetallies = _replayData.notedata.Last().TL;

            return JsonConvert.SerializeObject(_replayData);
        }
        private string GetInputTypeString()
        {
            if (_wasTabletUsed)
                return "Tablet";
            else if (_wasTouchScreenUsed)
                return "Touch";
            return "Mouse";
        }
        #endregion

        #region ReplayPlayer

        public void OnReplayPlayerStart()
        {
            _frameIndex = 0;
            _tootIndex = 0;
            _isTooting = false;
        }

        public void OnReplayRewind(float newTiming, GameController __instance)
        {
            _frameIndex = Mathf.Clamp(_replayData.framedata.FindIndex(frame => frame.T > newTiming) - 1, 0, _replayData.framedata.Count - 1);
            _tootIndex = Mathf.Clamp(_replayData.tootdata.FindIndex(frame => frame.T > newTiming) - 1, 0, _replayData.tootdata.Count - 1);

            if (__instance.currentnoteindex != 0)
                __instance.currentscore = _replayData.notedata.Find(note => note.I == __instance.currentnoteindex - 1).S;
        }

        public ReplayState LoadReplay(string replayFileName)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            if (!Directory.Exists(replayDir))
            {
                TootTallyLogger.LogInfo("Replay folder not found");
                return ReplayState.ReplayLoadError;
            }
            if (!File.Exists(replayDir + replayFileName + ".ttr"))
            {
                TootTallyLogger.LogInfo("Replay File does not exist");
                return ReplayState.ReplayLoadNotFound;
            }

            string jsonFileFromZip = FileHelper.ReadJsonFromFile(replayDir, replayFileName + ".ttr");

            _replayData = JsonConvert.DeserializeObject<ReplayData>(jsonFileFromZip);
            if (incompatibleReplayVersions.Contains(_replayData.pluginbuilddate.ToString()))
            {
                PopUpNotifManager.DisplayNotif($"Replay incompatible:\nReplay Build Date is {_replayData.pluginbuilddate}\nCurrent Build Date is {Plugin.BUILDDATE}", GameTheme.themeColors.notification.errorText);
                TootTallyLogger.LogError("Cannot load replay:");
                TootTallyLogger.LogError("   Replay Build Date is " + _replayData.pluginbuilddate);
                TootTallyLogger.LogError("   Current Plugin Build Date " + Plugin.BUILDDATE);
                return ReplayState.ReplayLoadErrorIncompatible;
            }
            GlobalVariables.gamescrollspeed = _replayData.scrollspeed;
            GameModifierManager.LoadModifiersFromString(_replayData.gamemodifiers ?? "");

            return ReplayState.ReplayLoadSuccess;
        }

        public void PlaybackReplay(GameController __instance, float time)
        {
            Cursor.visible = true;
            if (!__instance.controllermode) __instance.controllermode = true; //Still required to not make the mouse position update

            PlaybackFrameData(time);
            PlaybackTootData(time);

            if (_replayData.framedata.Count > _frameIndex && _lastFrame != null && _currentFrame != null)
                InterpolateCursorPosition(time, __instance);
            else if (_currentFrame != null && _replayData.framedata.Count < _frameIndex)
                SetCursorPosition(__instance, _currentFrame.P);

        }

        private void InterpolateCursorPosition(float time, GameController __instance)
        {
            if (_currentFrame.T - _lastFrame.T > 0)
            {
                var newCursorPosition = EasingHelper.Lerp(_lastFrame.P, _currentFrame.P, (float)((time - _lastFrame.T) / (_currentFrame.T - _lastFrame.T)));
                SetCursorPosition(__instance, newCursorPosition);
                __instance.puppet_humanc.doPuppetControl(-newCursorPosition / 225); //225 is half of the Gameplay area:450
            }
        }

        private void PlaybackFrameData(float time)
        {

            if (_lastFrame != _currentFrame && time >= _currentFrame.T)
                _lastFrame = _currentFrame;

            if (_replayData.framedata.Count > _frameIndex && (_currentFrame == null || time >= _currentFrame.T))
            {
                _frameIndex = _replayData.framedata.FindIndex(_frameIndex > 1 ? _frameIndex - 1 : 0, x => time < x.T);
                if (_replayData.framedata.Count > _frameIndex && _frameIndex != -1)
                    _currentFrame = _replayData.framedata[_frameIndex];
            }
        }

        private void PlaybackTootData(float currentMapPosition)
        {
            if (currentMapPosition >= _currentToot.T && _isTooting != (_currentToot.O == 1))
                _isTooting = _currentToot.O == 1;

            if (_replayData.tootdata.Count > _tootIndex && currentMapPosition >= _currentToot.T)
                _currentToot = _replayData.tootdata[_tootIndex++];
        }

        public void SetNoteScorePrefix(GameController __instance)
        {
            if (_replayData.notedata.Last().I > __instance.currentnoteindex)
                _currentNote = _replayData.notedata.Find(x => x.I == __instance.currentnoteindex);
            if (_currentNote != null)
            {
                __instance.notescoreaverage = _currentNote.NS;
                __instance.released_button_between_notes = _currentNote.R == 1;
            }
        }

        public void SetNoteScorePostFix(GameController __instance)
        {
            if (_currentNote != null)
            {
                __instance.rainbowcontroller.champmode = _currentNote.C == 1;
                __instance.multiplier = _currentNote.M;
                if (__instance.currentscore < 0)
                    __instance.currentscore = _currentNote.S;
                __instance.totalscore = _currentNote.S;
                __instance.currenthealth = _currentNote.H;
                __instance.highestcombo_level = _currentNote.HC;
                _currentNote = null;
            }
        }

        #endregion

        #region Utils
        public void SetCursorPosition(GameController __instance, float newPosition)
        {
            Vector3 pointerPosition = __instance.pointer.transform.localPosition;
            pointerPosition.y = newPosition;
            __instance.pointer.transform.localPosition = pointerPosition;
        }
        public void ClearData()
        {
            TootTallyLogger.LogInfo("Replay data cleared");
            _replayData.ClearData();
        }

        [Serializable]
        private class ReplayData
        {
            public string version { get; set; }
            public string username { get; set; }
            public string starttime { get; set; }
            public string endtime { get; set; }
            public string input { get; set; }
            public string song { get; set; }
            public string uuid { get; set; }
            public int samplerate { get; set; }
            public float scrollspeed { get; set; }
            public float gamespeedmultiplier { get; set; }
            public string gamemodifiers { get; set; }
            public float audiolatency { get; set; }
            public int pluginbuilddate { get; set; }
            public string gameversion { get; set; }
            public string songhash { get; set; }
            public int finalscore { get; set; }
            public int maxcombo { get; set; }
            public int[] finalnotetallies { get; set; }
            public List<FrameData> framedata { get; set; }
            public List<NoteData> notedata { get; set; }
            public List<TootData> tootdata { get; set; }

            public ReplayData(int targetFramerate)
            {
                framedata = new List<FrameData>();
                notedata = new List<NoteData>();
                tootdata = new List<TootData>();
                pluginbuilddate = Plugin.BUILDDATE;
                version = REPLAY_VERSION;
                username = Plugin.userInfo.username;
                scrollspeed = GlobalVariables.gamescrollspeed;
                gamespeedmultiplier = float.Parse(ReplaySystemManager.gameSpeedMultiplier.ToString("0.00"));
                audiolatency = GlobalVariables.localsettings.latencyadjust;
                gamemodifiers = GameModifierManager.GetModifiersString();
                gameversion = Application.version;
                samplerate = targetFramerate;

                var track = TrackLookup.lookup(GlobalVariables.chosen_track_data.trackref);
                if (track is CustomTrack)
                    songhash = SongDataHelper.GetSongHash(track);
                else
                    songhash = SongDataHelper.CalcSHA256Hash(Encoding.UTF8.GetBytes(SongDataHelper.GenerateBaseTmb(track)));


            }

            public void ClearData()
            {
                framedata.Clear();
                notedata.Clear();
                tootdata.Clear();
            }

            public void SetStartTime()
            {
                starttime = new DateTimeOffset(DateTime.Now.ToUniversalTime()).ToUnixTimeSeconds().ToString();
            }

            public void SetEndTime()
            {
                endtime = new DateTimeOffset(DateTime.Now.ToUniversalTime()).ToUnixTimeSeconds().ToString();
            }

        }

        private class FrameData
        {
            public float T { get; set; } //Time
            public float N { get; set; } //NoteHolderPosition
            public float P { get; set; } //CursorPosY
            public float MX { get; set; } //MousePosX
            public float MY { get; set; } //MousePosY

            public FrameData(float time, float noteHolder, float cursorPosition, float mousePositionX, float mousePositionY)
            {
                T = time;
                N = noteHolder;
                P = cursorPosition;
                MX = mousePositionX;
                MY = mousePositionY;
            }
        }

        private class TootData
        {
            public float T { get; set; } //Time
            public float N { get; set; } //NoteHolderPosition
            public int O { get; set; } //IsTooting

            public TootData(float time, float noteHolder, int isTooting)
            {
                T = time;
                N = noteHolder;
                O = isTooting;
            }
        }

        private class NoteData
        {
            public int I { get; set; } //NoteIndex Post
            public float NS { get; set; } //NoteScoreAverage Pre
            public int C { get; set; } //Combo Post
            public int M { get; set; } // Multiplier Post 
            public int S { get; set; } //TotalScore Post
            public int R { get; set; } //ReleasedBetweenNotes Pre
            public float H { get; set; } //Health Post
            public int HC { get; set; } //HighestCombo Post
            public int[] TL { get; set; }
            public NoteData(float noteScore,int releasedBetweenNotes)
            {
                NS = noteScore;
                R = releasedBetweenNotes;
            }

            public void PostFixConstructor(int index, int combo, int multiplier, int totalScore, float health, int highestCombo, int[] tL)
            {
                I = index;
                C = combo;
                M = multiplier;
                S = totalScore;
                H = health;
                HC = highestCombo;
                TL = tL;
            }

        }

        public enum ReplayState
        {
            None,
            ReplayLoadErrorIncompatible,
            ReplayLoadError,
            ReplayLoadSuccess,
            ReplayLoadNotFound,
        }

        #endregion
    }
}
