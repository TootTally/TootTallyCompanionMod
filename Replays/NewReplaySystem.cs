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
using TootTally.Utils.Helpers;
using TrombLoader.CustomTracks;
using UnityEngine;
using static TootTally.Utils.APIServices.SerializableClass;

namespace TootTally.Replays
{
    public class NewReplaySystem
    {
        public static List<string> incompatibleReplayVersions = new List<string> { "1.0.0" };
        public const string REPLAY_VERSION = "2.0.0";

        private int _frameIndex, _tootIndex;

        private ReplayData _replayData;
        private dynamic[] _lastFrame, _currentFrame;
        private dynamic[] _currentToot;
        private dynamic[] _currentNote;

        public float GetReplaySpeed { get => _replayData.gamespeedmultiplier; }

        private bool _wasTouchScreenUsed;
        private bool _wasTabletUsed;
        private bool _isTooting;
        private int _maxCombo;
        private bool _isLastNote;
        public bool GetIsTooting { get => _isTooting; }

        public string GetUsername { get => _replayData.username; }
        public string GetSongName { get => _replayData.song; }

        public bool IsFullCombo { get => GlobalVariables.gameplay_notescores[0] == 0 && GlobalVariables.gameplay_notescores[1] == 0 && GlobalVariables.gameplay_notescores[2] == 0; }
        public bool IsTripleS { get => GlobalVariables.gameplay_notescores[0] == 0 && GlobalVariables.gameplay_notescores[1] == 0 && GlobalVariables.gameplay_notescores[2] == 0 && GlobalVariables.gameplay_notescores[3] == 0; }

        private bool _isOldReplay;

        public NewReplaySystem()
        {
            _replayData = new ReplayData(60);
        }

        #region ReplayRecorder

        public void SetupRecording(int targetFramerate)
        {
            _replayData = new ReplayData(targetFramerate);
            _replayData.gamemodifiers = GameModifierManager.GetModifiersString();
            _replayData.scrollspeed = GlobalVariables.gamescrollspeed;
            _replayData.gamespeedmultiplier = float.Parse(ReplaySystemManager.gameSpeedMultiplier.ToString("0.00"));
            var track = TrackLookup.lookup(GlobalVariables.chosen_track_data.trackref);
            _replayData.song = track.trackname_long;
            if (track is CustomTrack)
                _replayData.songhash = SongDataHelper.GetSongHash(track);
            else
                _replayData.songhash = SongDataHelper.CalcSHA256Hash(Encoding.UTF8.GetBytes(SongDataHelper.GenerateBaseTmb(track)));

            _currentFrame = new dynamic[5];
            _currentNote = new dynamic[9];
            _currentToot = new dynamic[3];
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

            dynamic[] frame = new dynamic[5]
            {
                Round(time, 10f),
                Round(noteHolderPosition, 10f),
                Round(__instance.pointer.transform.localPosition.y),
                (int)Input.mousePosition.x,
                (int)Input.mousePosition.y,
            };

            if (_lastFrame == null || (frame[2] != _lastFrame[2] && frame[1] != _lastFrame[1] && frame[4] != _lastFrame[4]))
            {
                _replayData.framedata.Add(frame);
                _lastFrame = frame;
            }

        }

        private const float FLOAT_PRECISION = 1000f;
        public float Round(float f, float mult = 1f) => Mathf.Round(f * FLOAT_PRECISION * mult) / (FLOAT_PRECISION * mult);

        public void RecordNoteDataPrefix(GameController __instance)
        {
            _currentNote = new dynamic[9]
            {
                Round(__instance.notescoreaverage),
                __instance.released_button_between_notes ? 1 : 0,
                0, 0, 0, 0, 0f, 0, 0
            };
        }

        public void RecordNoteDataPostfix(GameController __instance)
        {
            _currentNote[2] = __instance.currentnoteindex;
            _currentNote[3] = __instance.highestcombocounter;
            _currentNote[4] = __instance.multiplier;
            _currentNote[5] = __instance.totalscore;
            _currentNote[6] = Round(__instance.currenthealth);
            _currentNote[7] = _maxCombo = __instance.highestcombo_level;
            _currentNote[8] = _lastTally;
            _replayData.notedata.Add(_currentNote);
        }

        private int _lastTally;

        public void SaveLastNoteTally(int tallyIndex) => _lastTally = tallyIndex;

        public void RecordToot(float time, float noteHolderPosition, bool isTooting)
        {
            dynamic[] toot = new dynamic[3]
            {
                Round(time, 10f),
                Round(noteHolderPosition, 10f),
                isTooting ? 1 : 0
            };
            _replayData.tootdata.Add(toot);
        }

        public string GetRecordedReplayJson(string uuid)
        {
            _replayData.uuid = uuid;
            _replayData.input = GetInputTypeString();
            _replayData.finalscore = GlobalVariables.gameplay_scoretotal;
            _replayData.maxcombo = _maxCombo;
            //That's absolutely disgusting I love it.
            _replayData.finalnotetallies = new int[]
            {
                _replayData.notedata.Count(x => x[(int)NDStruct.TL] == 4),
                _replayData.notedata.Count(x => x[(int)NDStruct.TL] == 3),
                _replayData.notedata.Count(x => x[(int)NDStruct.TL] == 2),
                _replayData.notedata.Count(x => x[(int)NDStruct.TL] == 1),
                _replayData.notedata.Count(x => x[(int)NDStruct.TL] == 0),
            };

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
            _isLastNote = false;
            _currentFrame = _replayData.framedata.First();
            _currentNote = _replayData.notedata.First();
            _currentToot = new dynamic[] { 0, 0, 0 };
        }

        public void OnReplayRewind(float newTiming, GameController __instance)
        {
            _frameIndex = Mathf.Clamp(_replayData.framedata.FindIndex(frame => (float)frame[(int)FDStruct.T] > newTiming) - 1, 0, _replayData.framedata.Count - 1);
            _tootIndex = Mathf.Clamp(_replayData.tootdata.FindIndex(frame => (float)frame[(int)TDStruct.T] > newTiming) - 1, 0, _replayData.tootdata.Count - 1);

            _currentFrame = _replayData.framedata[_frameIndex];
            _currentToot = _replayData.tootdata[_tootIndex];
            _isTooting = false;

            if (__instance.currentnoteindex != 0)
                __instance.currentscore = (int)_replayData.notedata.Find(note => (int)note[(int)NDStruct.I] == __instance.currentnoteindex - 1)[(int)NDStruct.S];
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

            var replayVersion = JsonConvert.DeserializeObject<ReplayVersion>(jsonFileFromZip).version;
            _replayData = JsonConvert.DeserializeObject<ReplayData>(jsonFileFromZip);
            _isOldReplay = replayVersion == null || IsOldReplayVersion(replayVersion);
            if (_isOldReplay)
                ConvertToCurrentReplayVersion(ref _replayData);

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

        private bool IsOldReplayVersion(string version) => string.Compare(version, REPLAY_VERSION) < 0;

        private void ConvertToCurrentReplayVersion(ref ReplayData replayData)
        {
            PopUpNotifManager.DisplayNotif("Converting old replay format...");

            dynamic[][] frameData = new dynamic[replayData.framedata.Count][];
            for (int i = 0; i < replayData.framedata.Count; i++)
            {
                frameData[i] = new dynamic[5]
                {
                    (int)Math.Abs(replayData.framedata[i][0]), //Replace time with noteholder
                    (int)replayData.framedata[i][0],
                    (float)replayData.framedata[i][1] / 100f, //Pointer position
                    0,
                    0
                };
            }

            dynamic[][] noteData = new dynamic[replayData.notedata.Count][];
            for (int i = 0; i < replayData.notedata.Count; i++)
            {
                noteData[i] = new dynamic[9]
                {
                    (float)replayData.notedata[i][5] / 1000f, //NoteScore
                    -1,
                    (int)replayData.notedata[i][0], //Index
                    -1,
                    (int)replayData.notedata[i][2], //Multiplier
                    (int)replayData.notedata[i][1], //TotalScore
                    (int)replayData.notedata[i][3], //Health
                    -1,
                    (int)replayData.notedata[i][4] //Note Tally
                };
            }

            dynamic[][] tootData = new dynamic[replayData.tootdata.Count][];
            for (int i = 0; i < replayData.tootdata.Count; i++)
            {
                tootData[i] = new dynamic[3]
                {
                    (int)Math.Abs(replayData.tootdata[i][0]), //Replace time with noteholder
                    replayData.tootdata[i][0],
                    i % 2 == 0 ? 1 : 0,
                };

            }


            replayData.framedata = frameData.ToList();
            replayData.notedata = noteData.ToList();
            replayData.tootdata = tootData.ToList();
        }



        public void PlaybackReplay(GameController __instance, float time)
        {
            Cursor.visible = true;
            if (!__instance.controllermode) __instance.controllermode = true; //Still required to not make the mouse position update

            if (_isOldReplay)
                time = (_replayData.pluginbuilddate < 20230705 ?
                 Math.Abs(__instance.noteholder.transform.position.x) : Math.Abs(__instance.noteholderr.anchoredPosition.x)) * GetNoteHolderPrecisionMultiplier();
            else
                time = time + ((_replayData.audiolatency / 1000f) - (__instance.latency_offset / 1000f));
            PlaybackTimeFrameData(time);
            PlaybackTimeTootData(time);
            if (_replayData.framedata.Count > _frameIndex && _lastFrame != null && _currentFrame != null)
                InterpolateTimeCursorPosition(time, __instance);



        }

        public static float GetNoteHolderPrecisionMultiplier() => 10 / (GlobalVariables.gamescrollspeed <= 1 ? GlobalVariables.gamescrollspeed : 1);

        private void InterpolateTimeCursorPosition(float time, GameController __instance)
        {
            if (_currentFrame[(int)FDStruct.T] - _lastFrame[(int)FDStruct.T] > 0)
            {
                var newCursorPosition = EasingHelper.Lerp((float)_lastFrame[(int)FDStruct.P], (float)_currentFrame[(int)FDStruct.P], (float)((time - (float)_lastFrame[(int)FDStruct.T]) / ((float)_currentFrame[(int)FDStruct.T] - (float)_lastFrame[(int)FDStruct.T])));
                SetCursorPosition(__instance, newCursorPosition);
                __instance.puppet_humanc.doPuppetControl(-newCursorPosition / 225); //225 is half of the Gameplay area:450
            }
        }

        private void PlaybackTimeFrameData(float time)
        {

            if (_lastFrame != _currentFrame && time >= _currentFrame[(int)FDStruct.T])
                _lastFrame = _currentFrame;

            if (_replayData.framedata.Count > _frameIndex && (_currentFrame == null || time >= _currentFrame[(int)FDStruct.T]))
            {
                _frameIndex = _replayData.framedata.FindIndex(_frameIndex > 1 ? _frameIndex - 1 : 0, x => time < x[(int)FDStruct.T]);
                if (_replayData.framedata.Count > _frameIndex && _frameIndex != -1)
                    _currentFrame = _replayData.framedata[_frameIndex];
            }
        }

        private void PlaybackTimeTootData(float time)
        {
            if (time >= _currentToot[(int)TDStruct.T] && _isTooting != (_currentToot[(int)TDStruct.O] == 1))
                _isTooting = _currentToot[(int)TDStruct.O] == 1;

            if (_replayData.tootdata.Count > _tootIndex && time >= _currentToot[(int)TDStruct.T])
                _currentToot = _replayData.tootdata[_tootIndex++];
        }

        public void SetNoteScorePrefix(GameController __instance)
        {
            if (!_isLastNote)
            {
                _currentNote = _replayData.notedata.Find(x => x[(int)NDStruct.I] == __instance.currentnoteindex);
                _isLastNote = _replayData.notedata.Last()[(int)NDStruct.I] == __instance.currentnoteindex;
            }
            if (_currentNote != null)
            {
                __instance.notescoreaverage = (float)_currentNote[(int)NDStruct.NS];
                if (!_isOldReplay)
                    __instance.released_button_between_notes = _currentNote[(int)NDStruct.R] == 1;

            }
        }

        public void SetNoteScorePostFix(GameController __instance)
        {
            if (_currentNote != null)
            {
                __instance.rainbowcontroller.champmode = _currentNote[(int)NDStruct.H] == 100;
                __instance.multiplier = (int)_currentNote[(int)NDStruct.M];
                if (__instance.currentscore < 0)
                    __instance.currentscore = (int)_currentNote[(int)NDStruct.S];
                __instance.totalscore = (int)_currentNote[(int)NDStruct.S];
                __instance.currenthealth = (float)_currentNote[(int)NDStruct.H];
                if (!_isOldReplay)
                {
                    __instance.highestcombocounter = (int)_currentNote[(int)NDStruct.C];
                    __instance.highestcombo_level = (int)_currentNote[(int)NDStruct.HC];
                }
                _currentNote = null;
            }
        }

        #endregion

        #region Utils
        public void SetCursorPosition(GameController __instance, float newPosition)
        {
            Vector2 pointerPosition = __instance.pointer.transform.localPosition;
            pointerPosition.y = newPosition;
            __instance.pointer.transform.localPosition = pointerPosition;
        }
        public void ClearData()
        {
            TootTallyLogger.LogInfo("Replay data cleared");
            _replayData?.ClearData();
        }

        [Serializable]
        private class ReplayVersion
        {
            public string version { get; set; }
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
            public float samplerate { get; set; }
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
            public List<dynamic[]> framedata { get; set; }
            public List<dynamic[]> notedata { get; set; }
            public List<dynamic[]> tootdata { get; set; }

            public ReplayData(float sampleRate)
            {
                framedata = new List<dynamic[]>();
                notedata = new List<dynamic[]>();
                tootdata = new List<dynamic[]>();
                pluginbuilddate = Plugin.BUILDDATE;
                version = REPLAY_VERSION;
                username = Plugin.userInfo.username;
                audiolatency = GlobalVariables.localsettings.latencyadjust;
                gameversion = Application.version;
                samplerate = sampleRate;
            }

            public void ClearData()
            {
                framedata?.Clear();
                notedata?.Clear();
                tootdata?.Clear();
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

        public enum FDStruct
        {
            T, //Time
            N, //NoteHolder
            P, //CursorPosition
            MX, //MouseX
            MY //MouseY
        }

        public enum TDStruct
        {
            T, //Time
            N, //NoteHolder
            O //IsTooting
        }

        public enum NDStruct
        {
            NS, //NoteScoreAverage
            R, //ReleasedBetweenNotes
            I, //Index
            C, //Combo
            M, //Multiplier
            S, //Score
            H, //Health
            HC, //HighestCombo
            TL, //TallyScore
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
