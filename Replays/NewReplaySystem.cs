﻿using BepInEx;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using TrombLoader.Helpers;
using UnityEngine;

namespace TootTally.Replays
{
    public class NewReplaySystem
    {
        public static List<string> incompatibleReplayPluginBuildDate = new List<string> { "20230106" };

        private int _scores_A, _scores_B, _scores_C, _scores_D, _scores_F, _totalScore;
        private int[] _noteTally; // [nasties, mehs, okays, nices, perfects]
        private int _frameIndex, _tootIndex;
        private int _maxCombo;

        private List<int[]> _frameData = new List<int[]>(), _noteData = new List<int[]>(), _tootData = new List<int[]>();
        private DateTimeOffset _startTime, _endTime;

        private bool _wasTouchScreenUsed;
        private bool _wasTabletUsed;
        private bool _isTooting;
        public bool GetIsTooting { get => _isTooting; }

        private float _nextPositionTarget, _lastPosition;
        private float _nextTimingTarget, _lastTiming;



        #region ReplayRecorder
        public NewReplaySystem()
        {
        }

        public void SetupRecording(GameController __instance)
        {
            _scores_A = _scores_B = _scores_C = _scores_D = 0;
            _maxCombo = 0;
            _startTime = new DateTimeOffset(DateTime.Now.ToUniversalTime());

            Plugin.LogInfo("Started recording replay");
        }

        public void FinalizedRecording()
        {
            _endTime = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            Plugin.LogInfo("Replay recording finished");
        }

        public void RecordFrameData(GameController __instance)
        {
            if (Input.touchCount > 0) _wasTouchScreenUsed = true;
            float noteHolderPosition = __instance.noteholder.transform.position.x * GetNoteHolderPrecisionMultiplier(); // the slower the scrollspeed , the better the precision
            float pointerPos = __instance.pointer.transform.localPosition.y * 100; // 2 decimal precision
            _frameData.Add(new int[] { (int)noteHolderPosition, (int)pointerPos });
        }

        public void RecordNoteDataPrefix(GameController __instance)
        {
            var noteIndex = __instance.currentnoteindex;
            var totalScore = __instance.totalscore;
            var multiplier = __instance.multiplier;
            var currentHealth = __instance.currenthealth;

            _scores_A = __instance.scores_A;
            _scores_B = __instance.scores_B;
            _scores_C = __instance.scores_C;
            _scores_D = __instance.scores_D;
            _scores_F = __instance.scores_F;

            _noteData.Add(new int[] { noteIndex, totalScore, multiplier, (int)currentHealth, -1 });
        }

        public void RecordToot(GameController __instance)
        {
            float noteHolderPosition = __instance.noteholder.transform.position.x * GetNoteHolderPrecisionMultiplier(); // the slower the scrollspeed , the better the precision

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

        }

        public JSONObject GetRecordedReplayJson(string uuid, float targetFramerate)
        {
            string songNameLong = GlobalVariables.chosen_track_data.trackname_long;
            string trackRef = GlobalVariables.chosen_track_data.trackref;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            string songHash;
            if (!isCustom)
            {
                string songFilePath = SongDataHelper.GetSongFilePath(trackRef);
                string tmb = SongDataHelper.GenerateBaseTmb(songFilePath);
                songHash = SongDataHelper.CalcSHA256Hash(Encoding.UTF8.GetBytes(tmb));
            }
            else
                songHash = SongDataHelper.GetSongHash(trackRef);

            string username = Plugin.userInfo.username;

            string startDateTimeUnix = _startTime.ToUnixTimeSeconds().ToString();
            string endDateTimeUnix = _endTime.ToUnixTimeSeconds().ToString();

            string inputType = _wasTouchScreenUsed ? "touch" : "mouse";
            var replayJson = new JSONObject();
            replayJson["username"] = username;
            replayJson["starttime"] = startDateTimeUnix;
            replayJson["endtime"] = endDateTimeUnix;
            replayJson["uuid"] = uuid;
            replayJson["input"] = inputType;
            replayJson["song"] = songNameLong;
            replayJson["samplerate"] = targetFramerate;
            replayJson["scrollspeed"] = GlobalVariables.gamescrollspeed;
            replayJson["pluginbuilddate"] = Plugin.BUILDDATE;
            replayJson["gameversion"] = GlobalVariables.version;
            replayJson["songhash"] = songHash;
            replayJson["finalscore"] = GlobalVariables.gameplay_scoretotal;
            replayJson["maxcombo"] = _maxCombo;

            var noteJudgmentData = new JSONArray();
            noteJudgmentData.Add(_scores_A); noteJudgmentData.Add(_scores_B); noteJudgmentData.Add(_scores_C); noteJudgmentData.Add(_scores_D); noteJudgmentData.Add(_scores_F);
            replayJson["finalnotetallies"] = noteJudgmentData;
            OptimizeFrameData(ref _frameData);
            OptimizeNoteData(ref _noteData);
            OptimizeTootData(ref _tootData);
            ValidateData();
            _noteData[_noteData.Count - 1][1] = GlobalVariables.gameplay_scoretotal; // Manually set the last note's totalscore to the actual totalscore because game is weird...

            replayJson["framedata"] = DataListToJson(_frameData);
            replayJson["notedata"] = DataListToJson(_noteData);
            replayJson["tootdata"] = DataListToJson(_tootData);

            return replayJson;
        }

        private static JSONArray DataListToJson(List<int[]> dataList)
        {
            var jsonArrayData = new JSONArray();
            dataList.ForEach(item =>
            {
                var dataJsonArray = new JSONArray();
                foreach (var itemData in item)
                    dataJsonArray.Add(itemData);
                jsonArrayData.Add(dataJsonArray);
            });
            return jsonArrayData;
        }


        private static void OptimizeFrameData(ref List<int[]> rawReplayFrameData)
        {
            //Look for matching position and remove same frames with the same positions
            for (int i = 0; i < rawReplayFrameData.Count - 1; i++)
            {
                for (int j = i + 1; j < rawReplayFrameData.Count && (CheckIfSameValue(i, j, (int)FrameDataStructure.PointerPosition, rawReplayFrameData) || CheckIfSameValue(i, j, (int)FrameDataStructure.NoteHolder, rawReplayFrameData));)
                {
                    rawReplayFrameData.Remove(rawReplayFrameData[j]);
                }
            }
        }

        private static bool CheckIfSameValue(int index1, int index2, int dataIndex, List<int[]> dataList) => dataList[index1][dataIndex] == dataList[index2][dataIndex];

        private static void OptimizeTootData(ref List<int[]> rawReplayTootData)
        {
            for (int i = 0; i < rawReplayTootData.Count - 1; i++)
            {
                //if two toot happens on the same frame, probably is inputFix so unsync the frames
                if (CheckIfSameValue(i, i+1, (int)TootDataStructure.NoteHolder, rawReplayTootData))
                {
                    rawReplayTootData[i][(int)TootDataStructure.NoteHolder]++;
                }

            }
        }

        private static void OptimizeNoteData(ref List<int[]> rawReplayNoteData)
        {
            //Current glitch in the game that duplicate a note score but doesn't have any judgement.
            for (int i = 0; i < rawReplayNoteData.Count; i++)
            {
                if (rawReplayNoteData[i][(int)NoteDataStructure.NoteJudgement] == -1)
                    rawReplayNoteData.Remove(rawReplayNoteData[i]);
            }
        }

        public bool ValidateData()
        {
            bool isValid = true;
            List<string> errorList = new List<string>();

            //Turning it off until fixed FrameDataOptimization
            for (int i = 0; i < _frameData.Count; i++)
            {
                if (GetDuplicatesFromDataList(_frameData[i][(int)FrameDataStructure.NoteHolder], (int)FrameDataStructure.NoteHolder, _frameData).Count > 1)
                {
                    isValid = false;
                    errorList.Add("Duplicate frames found, replay validation failed");
                    break;
                }
            }
            for (int i = 0; i < _noteData.Count; i++)
            {
                //if multiple notes has the same index
                if (GetDuplicatesFromDataList(_noteData[i][(int)NoteDataStructure.NoteIndex], (int)NoteDataStructure.NoteIndex, _noteData).Count > 1)
                {
                    isValid = false;
                    errorList.Add("Duplicate note index found, replay validation failed");
                    break;
                }
            }

            if (GetDuplicatesFromDataList(-1, (int)NoteDataStructure.NoteJudgement, _noteData).Count > 1)
            {
                isValid = false;
                errorList.Add("Note Data is missing note judgement, replay validation failed");
            }

            if (!isValid)
                errorList.ForEach(error => Plugin.LogError(error));

            return isValid;
        }

        private static List<int[]> GetDuplicatesFromDataList(float valueToFind, int dataIndex, List<int[]> dataList) => dataList.FindAll(data => data[dataIndex] == valueToFind);
        #endregion

        #region ReplayPlayer
        public void OnReplayPlayerStart()
        {
            _lastTiming = 0;
            _totalScore = 0;
            _noteTally = new int[5];
            _isTooting  = false;
        }

        public void OnReplayPlayerStop()
        {
            GlobalVariables.gameplay_notescores = _noteTally;
        }

        public ReplayState LoadReplay(string replayFileName)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            if (!Directory.Exists(replayDir))
            {
                Plugin.LogInfo("Replay folder not found");
                return ReplayState.ReplayLoadError;
            }
            if (!File.Exists(replayDir + replayFileName + ".ttr"))
            {
                Plugin.LogInfo("Replay File does not exist");
                return ReplayState.ReplayLoadNotFound;
            }

            string jsonFileFromZip = FileHelper.ReadJsonFromFile(replayDir, replayFileName + ".ttr");

            var replayJson = JSONObject.Parse(jsonFileFromZip);
            if (incompatibleReplayPluginBuildDate.Contains(replayJson["pluginbuilddate"]))
            {
                PopUpNotifManager.DisplayNotif($"Replay incompatible:\nReplay Build Date is {replayJson["pluginbuilddate"]}\nCurrent Build Date is {Plugin.BUILDDATE}", Color.red);
                Plugin.LogError("Cannot load replay:");
                Plugin.LogError("   Replay Build Date is " + replayJson["pluginbuilddate"]);
                Plugin.LogError("   Current Plugin Build Date " + Plugin.BUILDDATE);
                return ReplayState.ReplayLoadErrorIncompatible;
            }
            GlobalVariables.gamescrollspeed = replayJson["scrollspeed"];
            foreach (JSONArray jsonArray in replayJson["framedata"])
                _frameData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2] });
            foreach (JSONArray jsonArray in replayJson["notedata"])
                _noteData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2], jsonArray[3], jsonArray[4] });
            foreach (JSONArray jsonArray in replayJson["tootdata"])
                _tootData.Add(new int[] { jsonArray[0] });
            _frameIndex = _tootIndex = 0;
            ValidateData();//Validation Doesnt do anything except return a log, but we can use this to change the ReplayState to something like "ReplayValidationFailed" and use ReplaySystemManager to handle it.

            return ReplayState.ReplayLoadSuccess;
        }

        public void PlaybackReplay(GameController __instance)
        {
            if (!__instance.controllermode) __instance.controllermode = true; //Still required to not make the mouse position update

            var currentMapPosition = __instance.noteholder.transform.position.x * GetNoteHolderPrecisionMultiplier();

            if (_frameData.Count > _frameIndex && _lastPosition != 0)
                InterpolateCursorPosition(currentMapPosition, __instance);

            PlaybackFrameData(currentMapPosition, __instance);
            PlaybackTootData(currentMapPosition);
        }

        private void InterpolateCursorPosition(float currentMapPosition, GameController __instance)
        {
            var newCursorPosition = EasingHelper.Lerp(_lastPosition, _nextPositionTarget, (_lastTiming - currentMapPosition) / (_lastTiming - _nextTimingTarget));
            SetCursorPosition(__instance, newCursorPosition);
            __instance.puppet_humanc.doPuppetControl(-newCursorPosition / 225); //225 is half of the Gameplay area:450 
        }

        private void PlaybackFrameData(float currentMapPosition, GameController __instance)
        {
            while (_frameData.Count > _frameIndex && currentMapPosition <= _frameData[_frameIndex][(int)FrameDataStructure.NoteHolder]) //smaller or equal to because noteholder goes toward negative
            {
                _lastTiming = _frameData[_frameIndex][(int)FrameDataStructure.NoteHolder];
                _lastPosition = _frameData[_frameIndex][(int)FrameDataStructure.PointerPosition] / 100f;
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
        private void PlaybackTootData(float currentMapPosition)
        {
            while (_tootData.Count > _tootIndex && currentMapPosition <= _tootData[_tootIndex][(int)TootDataStructure.NoteHolder])
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
                _noteTally[tallyIndex]++;
            }
        }

        public void UpdateInstanceTotalScore(GameController __instance)
        {
            __instance.totalscore = _totalScore;
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
            _frameData.Clear();
            _noteData.Clear();
            _tootData.Clear();
        }

        public static float GetNoteHolderPrecisionMultiplier() => 10 / (GlobalVariables.gamescrollspeed <= 1 ? GlobalVariables.gamescrollspeed : 1);

        private enum FrameDataStructure
        {
            NoteHolder = 0,
            PointerPosition = 1,
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