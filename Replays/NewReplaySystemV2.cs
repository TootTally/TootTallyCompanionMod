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
        public bool GetIsTooting { get => _currentToot.O == 1; }

        private float _nextPositionTarget, _lastPosition;
        private float _nextTimingTarget, _lastTiming;
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

        public void RecordNoteDataPrefix(GameController __instance)
        {
            _currentNote = new NoteData(__instance.currentnoteindex, __instance.notescoreaverage, __instance.highestcombocounter, __instance.multiplier, __instance.totalscore, __instance.released_button_between_notes ? 1 : 0, __instance.currenthealth, __instance.highestcombo_level);
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
            _replayData.finalscore = _finalScore;
            _replayData.maxcombo = _highestCombo;
            _replayData.finalnotetallies = new int[]
            {
                _scores_A,
                _scores_B,
                _scores_C,
                _scores_D,
                _scores_F
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
        private static bool CheckIfSameValue(int index1, int index2, int dataIndex, List<int[]> dataList) => dataList[index1][dataIndex] == dataList[index2][dataIndex];

        private static List<int[]> GetDuplicatesFromDataList(float valueToFind, int dataIndex, List<int[]> dataList) => dataList.FindAll(data => data[dataIndex] == valueToFind);
        #endregion

        #region ReplayPlayer

        public void OnReplayPlayerStart()
        {
            _frameIndex = 0;
            _tootIndex = 0;
            _noteIndex = 0;
            _replayNoteTally = new int[5];
            _isTooting = false;
        }

        public void OnReplayRewind(float newTiming, GameController __instance)
        {
            _frameIndex = Mathf.Clamp(_replayData.framedata.FindIndex(frame => frame.T > newTiming) - 1, 0, _replayData.framedata.Count - 1);
            _tootIndex = Mathf.Clamp(_replayData.tootdata.FindIndex(frame => frame.T > newTiming) - 1, 0, _replayData.tootdata.Count - 1);

            if (__instance.currentnoteindex != 0)
                __instance.currentscore = _noteData.Find(note => note[(int)NoteData.I] == __instance.currentnoteindex - 1)[(int)NoteData.S];
        }


        public void OnReplayPlayerStop()
        {
            GlobalVariables.gameplay_notescores = _finalNoteTally;
            GlobalVariables.gameplay_scoretotal = _finalTotalScore;
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

            __instance.totalscore = _totalScore;
            if (_frameData.Count > _frameIndex && _lastPosition != 0)
                InterpolateCursorPosition(time, __instance);

            PlaybackFrameData(time, __instance);
            PlaybackTootData(time, __instance);
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

        public void SetNoteScore(GameController __instance)
        {
            var note = _noteData.Find(x => x[(int)NoteData.I] == __instance.currentnoteindex);

            if (note != null)
            {
                __instance.totalscore = _totalScore = note[(int)NoteData.S]; //total score has to be set postfix as well because notes SOMEHOW still give more points than they should during replay...
                __instance.multiplier = note[(int)NoteData.M];
                __instance.currenthealth = note[(int)NoteData.CurrentHealth];
                int tallyIndex = Mathf.Clamp(note[(int)NoteData.NoteJudgement], 0, 4); //Temporary fix for note tally being -1 sometimes?
                _replayNoteTally[tallyIndex]++;
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

        public void SetUsernameAndSongName(string username, string songname)
        {
            _replayUsername = username;
            _replaySong = songname;
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
            public int I { get; set; } //NoteIndex
            public float NS { get; set; } //NoteScoreAverage
            public int C { get; set; } //Combo
            public int M { get; set; } // Multiplier
            public int S { get; set; } //TotalScore
            public int R { get; set; } //ReleasedBetweenNotes
            public float H { get; set; } //Health
            public int HC { get; set; } //HighestCombo
            public NoteData(int index, float noteScore, int combo, int multiplier, int totalScore, int releasedBetweenNotes, float health, int highestCombo)
            {
                I = index;
                NS = noteScore;
                C = combo;
                M = multiplier;
                S = totalScore;
                R = releasedBetweenNotes;
                H = health;
                HC = highestCombo;
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
