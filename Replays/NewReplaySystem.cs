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

namespace TootTally.Replays
{
    public class NewReplaySystem
    {
        public static List<string> incompatibleReplayPluginBuildDate = new List<string> { "20230106" };
        public const string REPLAY_VERSION = "1.0.0";

        private int _scores_A, _scores_B, _scores_C, _scores_D, _scores_F, _totalScore, _finalTotalScore;
        private int[] _finalNoteTally, _replayNoteTally; // [nasties, mehs, okays, nices, perfects]
        private int _frameIndex, _tootIndex;
        private int _maxCombo;
        private int _replaybuilddate;

        private List<int[]> _frameData = new List<int[]>(), _noteData = new List<int[]>(), _tootData = new List<int[]>();
        private int[] _lastFrameData;
        private float _replaySpeed;
        public float GetReplaySpeed { get => _replaySpeed; }
        private DateTimeOffset _startTime, _endTime;

        private bool _wasTouchScreenUsed;
        private bool _wasTabletUsed;
        private bool _isTooting;
        public bool GetIsTooting { get => _isTooting; }

        private float _nextPositionTarget, _lastPosition;
        private float _nextTimingTarget, _lastTiming;
        private string _replayUsername, _replaySong;
        public string GetUsername { get => _replayUsername; }
        public string GetSongName { get => _replaySong; }

        public bool IsFullCombo { get => GlobalVariables.gameplay_notescores[0] == 0 && GlobalVariables.gameplay_notescores[1] == 0 && GlobalVariables.gameplay_notescores[2] == 0; }
        public bool IsTripleS { get => GlobalVariables.gameplay_notescores[0] == 0 && GlobalVariables.gameplay_notescores[1] == 0 && GlobalVariables.gameplay_notescores[2] == 0 && GlobalVariables.gameplay_notescores[3] == 0; }

        #region ReplayRecorder

        public void SetupRecording(GameController __instance)
        {
            _scores_A = _scores_B = _scores_C = _scores_D = 0;
            _maxCombo = 0;
            _lastFrameData = new int[4];

            TootTallyLogger.LogInfo("Started recording replay");
        }

        public void SetStartTime()
        {
            _startTime = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            TootTallyLogger.LogInfo($"Replay started recording at {_startTime}");
        }

        public void SetEndTime()
        {
            _endTime = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            TootTallyLogger.LogInfo($"Replay recording finished at {_endTime}");
        }

        public void RecordFrameData(GameController __instance)
        {
            if (Input.touchCount > 0) _wasTouchScreenUsed = true;

            float noteHolderPosition = __instance.noteholderr.anchoredPosition.x * GetNoteHolderPrecisionMultiplier(); // the slower the scrollspeed , the better the precision
            float pointerPos = __instance.pointer.transform.localPosition.y * 100; // 2 decimal precision
            float mousePosX = Input.mousePosition.x;
            float mousePosY = Input.mousePosition.y;

            _lastFrameData[(int)FrameDataStructure.NoteHolder] = (int)noteHolderPosition;
            _lastFrameData[(int)FrameDataStructure.PointerPosition] = (int)pointerPos;
            _lastFrameData[(int)FrameDataStructure.MousePositionX] = (int)mousePosX;
            _lastFrameData[(int)FrameDataStructure.MousePositionY] = (int)mousePosY;

            if (_frameData.Count < 1 || (_frameData.Last()[(int)FrameDataStructure.PointerPosition] != _lastFrameData[(int)FrameDataStructure.PointerPosition]
                && _frameData.Last()[(int)FrameDataStructure.NoteHolder] != _lastFrameData[(int)FrameDataStructure.NoteHolder]
                && _frameData.Last()[(int)FrameDataStructure.MousePositionY] != _lastFrameData[(int)FrameDataStructure.MousePositionY]))
                _frameData.Add(new int[] { (int)noteHolderPosition, (int)pointerPos, (int)mousePosX, (int)mousePosY });

            if (SpectatingManager.IsConnected && SpectatingManager.IsHost)
                SpectatingManager.SendFrameData(noteHolderPosition, pointerPos, __instance.isNoteButtonPressed());
        }

        public void RecordNoteDataPrefix(GameController __instance)
        {
            var noteIndex = __instance.currentnoteindex;
            var totalScore = __instance.totalscore;
            var multiplier = __instance.multiplier;
            var currentHealth = __instance.currenthealth;
            var noteScoreAverage = __instance.notescoreaverage * 1000;

            _noteData.Add(new int[] { noteIndex, totalScore, multiplier, (int)currentHealth, -1, (int)noteScoreAverage });
        }

        public void RecordToot(GameController __instance)
        {
            float noteHolderPosition = __instance.noteholderr.anchoredPosition.x * GetNoteHolderPrecisionMultiplier(); // the slower the scrollspeed , the better the precision

            _tootData.Add(new int[] { (int)noteHolderPosition });
        }

        public void RecordNoteDataPostfix(GameController __instance)
        {
            _maxCombo = __instance.highestcombo_level;
            var noteLetter = _scores_A != __instance.scores_A ? 4 :
               _scores_B != __instance.scores_B ? 3 :
               _scores_C != __instance.scores_C ? 2 :
               _scores_D != __instance.scores_D ? 1 : 0;
            _noteData[_noteData.Count - 1][(int)NoteDataStructure.NoteJudgement] = noteLetter;

            _scores_A = __instance.scores_A;
            _scores_B = __instance.scores_B;
            _scores_C = __instance.scores_C;
            _scores_D = __instance.scores_D;
            _scores_F = __instance.scores_F;


        }

        public string GetRecordedReplayJson(string uuid, float targetFramerate)
        {
            var trackRef = GlobalVariables.chosen_track_data.trackref;
            var track = TrackLookup.lookup(trackRef);
            string songHash;

            if (track is CustomTrack)
            {
                songHash = SongDataHelper.GetSongHash(track);
            }
            else
            {
                var tmb = SongDataHelper.GenerateBaseTmb(track);
                //FileHelper.WriteJsonToFile(Paths.BepInExRootPath, "basetmb.tmb", tmb);
                songHash = SongDataHelper.CalcSHA256Hash(Encoding.UTF8.GetBytes(tmb));
            }

            string username = Plugin.userInfo.username;

            string startDateTimeUnix = _startTime.ToUnixTimeSeconds().ToString();
            string endDateTimeUnix = _endTime.ToUnixTimeSeconds().ToString();

            string inputType = GetInputTypeString();
            var replayJson = new SerializableClass.ReplayData();
            replayJson.version = REPLAY_VERSION;
            replayJson.username = username;
            replayJson.starttime = startDateTimeUnix;
            replayJson.endtime = endDateTimeUnix;
            replayJson.uuid = uuid;
            replayJson.input = inputType;
            replayJson.song = track.trackname_long;
            replayJson.samplerate = targetFramerate;
            replayJson.scrollspeed = GlobalVariables.gamescrollspeed;
            replayJson.gamespeedmultiplier = float.Parse(ReplaySystemManager.gameSpeedMultiplier.ToString("0.00"));
            replayJson.audiolatency = GlobalVariables.localsettings.latencyadjust;
            replayJson.gamemodifiers = GameModifierManager.GetModifiersString();
            replayJson.pluginbuilddate = Plugin.BUILDDATE;
            replayJson.gameversion = Application.version;
            replayJson.songhash = songHash;
            replayJson.finalscore = GlobalVariables.gameplay_scoretotal;
            replayJson.maxcombo = _maxCombo;

            var noteJudgmentData = new List<int>
            {
                _scores_A,
                _scores_B,
                _scores_C,
                _scores_D,
                _scores_F
            };

            replayJson.finalnotetallies = noteJudgmentData.ToArray();
            OptimizeNoteData(ref _noteData);
            OptimizeTootData(ref _tootData);
            _noteData[_noteData.Count - 1][1] = GlobalVariables.gameplay_scoretotal; // Manually set the last note's totalscore to the actual totalscore because game is weird...

            replayJson.framedata = _frameData;
            replayJson.notedata = _noteData;
            replayJson.tootdata = _tootData;

            return JsonConvert.SerializeObject(replayJson);
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

        private static void OptimizeTootData(ref List<int[]> rawReplayTootData)
        {
            for (int i = 0; i < rawReplayTootData.Count - 1; i++)
                //if two toot happens on the same frame, probably is inputFix so unsync the frames
                if (CheckIfSameValue(i, i + 1, (int)TootDataStructure.NoteHolder, rawReplayTootData))
                    rawReplayTootData[i][(int)TootDataStructure.NoteHolder]++;

        }

        private static void OptimizeNoteData(ref List<int[]> rawReplayNoteData)
        {
            //Current glitch in the game that duplicate a note score but doesn't have any judgement.
            rawReplayNoteData.RemoveAll(x => x[(int)NoteDataStructure.NoteJudgement] == -1);
        }

        private static List<int[]> GetDuplicatesFromDataList(float valueToFind, int dataIndex, List<int[]> dataList) => dataList.FindAll(data => data[dataIndex] == valueToFind);
        #endregion

        #region ReplayPlayer

        public void OnReplayPlayerStart()
        {
            _lastTiming = 0;
            _totalScore = 0;
            _frameIndex = 0;
            _tootIndex = 0;
            _replayNoteTally = new int[5];
            _isTooting = false;
        }

        public void OnReplayRewind(float noteHolderNewPosX, GameController __instance)
        {
            float currentMapPosition = (noteHolderNewPosX / 45f) * GetNoteHolderPrecisionMultiplier(); //45f is a rough estimate of the ratio between localPos and worldPos

            _frameIndex = Mathf.Clamp(_frameData.FindIndex(frame => frame[(int)FrameDataStructure.NoteHolder] <= currentMapPosition) - 1, 0, _frameData.Count - 1);
            _lastTiming = _frameIndex > 0 ? _frameData[_frameIndex - 1][(int)FrameDataStructure.NoteHolder] : 0;
            _tootIndex = Mathf.Clamp(_tootData.FindIndex(frame => frame[(int)TootDataStructure.NoteHolder] <= currentMapPosition) - 1, 0, _tootData.Count - 1);
            _isTooting = _tootIndex % 2 == 1; //if the count is odd then its tooting.

            if (__instance.currentnoteindex != 0)
                __instance.currentscore = _noteData.Find(note => note[(int)NoteDataStructure.NoteIndex] == __instance.currentnoteindex - 1)[(int)NoteDataStructure.TotalScore];
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

            var replayJson = JsonConvert.DeserializeObject<SerializableClass.ReplayData>(jsonFileFromZip);
            if (incompatibleReplayPluginBuildDate.Contains(replayJson.pluginbuilddate.ToString()))
            {
                PopUpNotifManager.DisplayNotif($"Replay incompatible:\nReplay Build Date is {replayJson.pluginbuilddate}\nCurrent Build Date is {Plugin.BUILDDATE}", GameTheme.themeColors.notification.errorText);
                TootTallyLogger.LogError("Cannot load replay:");
                TootTallyLogger.LogError("   Replay Build Date is " + replayJson.pluginbuilddate);
                TootTallyLogger.LogError("   Current Plugin Build Date " + Plugin.BUILDDATE);
                return ReplayState.ReplayLoadErrorIncompatible;
            }
            GlobalVariables.gamescrollspeed = replayJson.scrollspeed;
            _frameData = replayJson.framedata;
            _noteData = replayJson.notedata;
            _tootData = replayJson.tootdata;
            _finalNoteTally = replayJson.finalnotetallies;
            _finalTotalScore = replayJson.finalscore;
            _replayUsername = replayJson.username;
            _replaySong = replayJson.song;
            _replaySpeed = replayJson.gamespeedmultiplier != 0 ? replayJson.gamespeedmultiplier : 1f;
            GameModifierManager.LoadModifiersFromReplayString(replayJson.gamemodifiers ?? "");
            _replaybuilddate = replayJson.pluginbuilddate;

            return ReplayState.ReplayLoadSuccess;
        }

        public void PlaybackReplay(GameController __instance)
        {
            Cursor.visible = true;
            if (!__instance.controllermode) __instance.controllermode = true; //Still required to not make the mouse position update

            //Backward compatibility with old replays
            var currentMapPosition = (_replaybuilddate < 20230705 ?
                 __instance.noteholder.transform.position.x : __instance.noteholderr.anchoredPosition.x) * GetNoteHolderPrecisionMultiplier();

            __instance.totalscore = _totalScore;
            if (_frameData.Count > _frameIndex && _lastPosition != 0)
                InterpolateCursorPosition(currentMapPosition, __instance);

            PlaybackFrameData(currentMapPosition, __instance);
            PlaybackTootData(currentMapPosition, __instance);
        }

        private void InterpolateCursorPosition(float currentMapPosition, GameController __instance)
        {
            var newCursorPosition = EasingHelper.Lerp(_lastPosition, _nextPositionTarget, (_lastTiming - currentMapPosition) / (_lastTiming - _nextTimingTarget));
            SetCursorPosition(__instance, newCursorPosition);
            __instance.puppet_humanc.doPuppetControl(-newCursorPosition / 225); //225 is half of the Gameplay area:450
        }

        private void PlaybackFrameData(float currentMapPosition, GameController __instance)
        {
            if (_frameData.Count > _frameIndex && currentMapPosition <= _frameData[_frameIndex][(int)FrameDataStructure.NoteHolder])
            {
                _lastTiming = _frameData[_frameIndex][(int)FrameDataStructure.NoteHolder];
                _lastPosition = _frameData[_frameIndex][(int)FrameDataStructure.PointerPosition] / 100f;
            }

            while (_frameData.Count > _frameIndex && currentMapPosition <= _frameData[_frameIndex][(int)FrameDataStructure.NoteHolder]) //smaller or equal to because noteholder goes toward negative
            {

                if (_frameIndex < _frameData.Count - 1)
                {
                    _nextTimingTarget = _frameData[_frameIndex + 1][(int)FrameDataStructure.NoteHolder];
                    _nextPositionTarget = _frameData[_frameIndex + 1][(int)FrameDataStructure.PointerPosition] / 100f;
                }
                else
                {
                    _nextTimingTarget = _lastTiming;
                    _nextPositionTarget = _lastPosition;
                }

                SetCursorPosition(__instance, _frameData[_frameIndex][(int)FrameDataStructure.PointerPosition] / 100f);

                _frameIndex++;
            }
        }
        private void PlaybackTootData(float currentMapPosition, GameController __instance)
        {
            if (_tootData.Count > _tootIndex && currentMapPosition <= _tootData[_tootIndex][(int)TootDataStructure.NoteHolder])
            {
                _isTooting = !_isTooting;
                _tootIndex++;
            }
        }

        public void SetNoteScore(GameController __instance)
        {
            var note = _noteData.Find(x => x[(int)NoteDataStructure.NoteIndex] == __instance.currentnoteindex);

            if (note != null)
            {
                __instance.totalscore = _totalScore = note[(int)NoteDataStructure.TotalScore]; //total score has to be set postfix as well because notes SOMEHOW still give more points than they should during replay...
                __instance.multiplier = note[(int)NoteDataStructure.Multiplier];
                __instance.currenthealth = note[(int)NoteDataStructure.CurrentHealth];
                int tallyIndex = Mathf.Clamp(note[(int)NoteDataStructure.NoteJudgement], 0, 4); //Temporary fix for note tally being -1 sometimes?
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
            _frameData.Clear();
            _noteData.Clear();
            _tootData.Clear();
        }

        public void SetUsernameAndSongName(string username, string songname)
        {
            _replayUsername = username;
            _replaySong = songname;
        }

        public static float GetNoteHolderPrecisionMultiplier() => 10 / (GlobalVariables.gamescrollspeed <= 1 ? GlobalVariables.gamescrollspeed : 1);

        private enum FrameDataStructure
        {
            NoteHolder = 0,
            PointerPosition = 1,
            MousePositionX = 2,
            MousePositionY = 3,
        }

        private enum TootDataStructure
        {
            NoteHolder = 0,
        }

        private enum NoteDataStructure
        {
            NoteIndex = 0,
            TotalScore = 1,
            Multiplier = 2,
            CurrentHealth = 3,
            NoteJudgement = 4,
            NoteScore = 5,
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
