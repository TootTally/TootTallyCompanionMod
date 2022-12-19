using BepInEx;
using HarmonyLib;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TootTally.Graphics;
using UnityEngine;

namespace TootTally
{
    public static class ReplaySystemJson
    {
        private static int _targetFramerate;
        private static int _scores_A, _scores_B, _scores_C, _scores_D, _scores_F, _totalScore;
        private static int[] _noteTally; // [nasties, mehs, okays, nices, perfects]
        private static int _replayIndex;
        private static List<int[]> _frameData = new List<int[]>(), _noteData = new List<int[]>();

        public static bool wasPlayingReplay;
        private static bool _isReplayPlaying, _isReplayRecording;
        private static bool _isTooting;

        private static float _nextPositionTarget, _lastPosition;
        private static float _nextTimingTarget, _lastTiming;
        private static float _elapsedTime;

        private static string _replayFileName;

        #region LevelSelectControllerPatches

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void AddTestButtonToLevelSelect(LevelSelectController __instance)
        {
            Transform mainCanvasTransform = GameObject.Find("MainCanvas/FullScreenPanel").transform;
            CustomButton mybutton = InteractableGameObjectFactory.CreateCustomButton(mainCanvasTransform, -new Vector2(400, 150), new Vector2(200, 50), "WATCH REPLAY", "ReplayButton", delegate { _replayFileName = "TestUser - Densmore - 1671466769"; __instance.playbtn.onClick?.Invoke(); });
        }

        #endregion

        #region GameControllerPatches

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void GameControllerPostfixPatch(GameController __instance)
        {
            _frameData.Clear();
            _noteData.Clear();
            if (_replayFileName == null)
                StartReplayRecorder(__instance);
            else
                StartReplayPlayer(__instance);
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
        [HarmonyPostfix]
        public static void PointSceneControllerPostfixPatch(PointSceneController __instance)
        {
            if (_isReplayRecording)
                StopReplayRecorder(__instance);
            else if (_isReplayPlaying)
                StopReplayPlayer(__instance);

        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        [HarmonyPrefix]
        public static void GameControllerUpdatePrefixPatch(GameController __instance)
        {
            if (_isReplayRecording)
                RecordFrameDataV2(__instance);
            else if (_isReplayPlaying)
                PlaybackReplayV2(__instance);
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
        [HarmonyPrefix]
        public static void GameControllerGetScoreAveragePrefixPatch(GameController __instance)
        {
            if (_isReplayRecording)
                RecordNote(__instance);
            else if (_isReplayPlaying)
                SetNoteScore(__instance);

        }


        [HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
        [HarmonyPostfix]
        public static void GameControllerGetScoreAveragePostfixPatch(GameController __instance)
        {
            if (_isReplayRecording)
                AddNoteJudgementToNoteData(__instance);
            else if (_isReplayPlaying)
                UpdateInstanceTotalScore(__instance);
        }

        [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.showPausePanel))]
        [HarmonyPostfix]
        static void PauseCanvasControllerShowPausePanelPostfixPatch(PauseCanvasController __instance)
        {
            _isReplayPlaying = _isReplayRecording = false;
            Plugin.LogInfo("Level paused, stopped " + (_isReplayPlaying ? "replay" : "recording"));
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.pauseQuitLevel))]
        [HarmonyPostfix]
        static void GameControllerPauseQuitLevelPostfixPatch(GameController __instance)
        {
            ClearData();
            _isReplayPlaying = _isReplayRecording = false;
            _replayFileName = null;
            Plugin.LogInfo("Level quit, clearing replay data");
        }
        #endregion


        #region ReplayRecorder
        public static void StartReplayRecorder(GameController __instance)
        {
            _isReplayRecording = true;
            wasPlayingReplay = false;
            _targetFramerate = Application.targetFrameRate > 60 || Application.targetFrameRate < 1 ? 60 : Application.targetFrameRate;
            _elapsedTime = 0;
            _scores_A = _scores_B = _scores_C = _scores_D = 0;
            Plugin.LogInfo("Started recording replay");
        }

        public static void StopReplayRecorder(PointSceneController __instance)
        {
            SaveReplayToFile(__instance);
            _isReplayRecording = false;
            Plugin.LogInfo("Replay recording finished");
        }

        public static void RecordFrameDataV2(GameController __instance)
        {
            float deltaTime = Time.deltaTime;
            _elapsedTime += deltaTime;
            if (_isReplayRecording && _elapsedTime >= 1f / _targetFramerate)
            {
                _elapsedTime = 0;
                float noteHolderPosition = __instance.noteholder.transform.position.x * 10; // 1 decimal precision
                float pointerPos = __instance.pointer.transform.localPosition.y * 100; //times 100 and convert to int for 2 decimal precision
                bool isTooting = __instance.noteplaying;
                _frameData.Add(new int[] { (int)noteHolderPosition, (int)pointerPos, isTooting ? 1 : 0 });
            }
        }

        public static void RecordNote(GameController __instance)
        {
            var noteIndex = __instance.currentnoteindex;
            var totalScore = __instance.totalscore;
            var multiplier = __instance.multiplier;
            var currentHealth = __instance.currenthealth;
            _scores_A = __instance.scores_A;
            _scores_B = __instance.scores_B;
            _scores_C = __instance.scores_C;
            _scores_D = __instance.scores_D;


            _noteData.Add(new int[] { noteIndex, totalScore, multiplier, (int)currentHealth, -1 }); // has to do the note judgement on postfix
        }

        public static void AddNoteJudgementToNoteData(GameController __instance)
        {
            var noteLetter = _scores_A != __instance.scores_A ? 0 :
               _scores_B != __instance.scores_B ? 1 :
               _scores_C != __instance.scores_C ? 2 :
               _scores_D != __instance.scores_D ? 3 : 4;
            _noteData[_noteData.Count - 1][4] = noteLetter;
        }

        public static void SaveReplayToFile(PointSceneController __instance)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            // Create Replays directory in case it doesn't exist
            if (!Directory.Exists(replayDir)) Directory.CreateDirectory(replayDir);

            string username = "TestUser";
            string songName = GlobalVariables.chosen_track_data.trackname_short;
            DateTimeOffset currentDateTime = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            string currentDateTimeUnix = currentDateTime.ToUnixTimeSeconds().ToString();
            string replayFilename = $"{username} - {songName} - {currentDateTimeUnix}";

            var replayJson = new JSONObject();
            replayJson["username"] = username;
            replayJson["date"] = currentDateTimeUnix;
            replayJson["song"] = songName;
            replayJson["samplerate"] = _targetFramerate;
            replayJson["scrollspeed"] = GlobalVariables.gamescrollspeed;
            var replayFrameData = new JSONArray();
            OptimizeFrameDataV2(ref _frameData);
            _frameData.ForEach(frame =>
            {
                var frameDataJsonArray = new JSONArray();
                foreach (var frameData in frame)
                    frameDataJsonArray.Add(frameData);
                replayFrameData.Add(frameDataJsonArray);
            });
            replayJson["framedata"] = replayFrameData;

            var replayNoteData = new JSONArray();
            _noteData.ForEach(notes =>
            {
                var noteDataJsonArray = new JSONArray();
                foreach (var noteData in notes)
                    noteDataJsonArray.Add(noteData);
                replayNoteData.Add(noteDataJsonArray);
            });
            replayJson["notedata"] = replayNoteData;

            File.WriteAllText(replayDir + replayFilename, replayJson.ToString());

        }


        public static void OptimizeFrameDataV2(ref List<int[]> rawReplayFrameData)
        {
            Plugin.LogInfo("Optimizing ReplayV2...");

            //Look for matching position && tooting values and remove same frames with the same positions
            for (int i = 0; i < rawReplayFrameData.Count - 1; i++)
            {
                for (int j = i + 1; j < rawReplayFrameData.Count && rawReplayFrameData[i][1] == rawReplayFrameData[j][1] && rawReplayFrameData[i][2] == rawReplayFrameData[j][2];)
                {
                    rawReplayFrameData.Remove(rawReplayFrameData[j]);
                }
            }
        }

        public static void OptimizeNoteData(ref List<int[]> rawReplayNoteData)
        {
            for (int i = 0; i < rawReplayNoteData.Count; i++)
            {
                //todo
            }
        }
        #endregion


        #region ReplayPlayer
        public static void StartReplayPlayer(GameController __instance)
        {
            _lastTiming = 0;
            _isTooting = false;
            _totalScore = 0;
            _noteTally = new int[5];
            _isReplayPlaying = wasPlayingReplay = true;
            LoadReplay(_replayFileName);
            Plugin.LogInfo("Started replay");
        }

        public static void StopReplayPlayer(PointSceneController __instance)
        {
            _isReplayPlaying = false;
            GlobalVariables.gameplay_notescores = _noteTally;
            Plugin.LogInfo("Replay finished");
        }

        public static void LoadReplay(string replayFileName)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            if (!Directory.Exists(replayDir))
            {
                Plugin.LogInfo("Replay folder not found");
                return;
            }

            if (!File.Exists(replayDir + replayFileName))
            {
                Plugin.LogInfo("Replay File not found");
                return;
            }

            string jsonFile = File.ReadAllText(replayDir + replayFileName);
            var replayJson = JSONObject.Parse(jsonFile);
            GlobalVariables.gamescrollspeed = replayJson["scrollspeed"];
            foreach (JSONArray jsonArray in replayJson["framedata"])
                _frameData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2] });
            foreach (JSONArray jsonArray in replayJson["notedata"])
                _noteData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2], jsonArray[3], jsonArray[4] });
            _replayIndex = 0;

        }

        public static void PlaybackReplayV2(GameController __instance)
        {
            if (!__instance.controllermode) __instance.controllermode = true;

            var currentMapPosition = __instance.noteholder.transform.position.x * 10;

            if (_frameData.Count >= _replayIndex && _lastPosition != 0)
            {
                var newCursorPosition = Lerp(_lastPosition, _nextPositionTarget, (_lastTiming - currentMapPosition) / (_lastTiming - _nextTimingTarget));
                SetCursorPosition(__instance, newCursorPosition);
            }
            else
            {
                __instance.totalscore = _totalScore;
            }


            while (_frameData.Count >= _replayIndex && _isReplayPlaying && currentMapPosition <= _frameData[_replayIndex][0]) //smaller or equal to because noteholder goes toward negative
            {
                _lastTiming = _frameData[_replayIndex][0];
                _nextTimingTarget = _frameData[_replayIndex + 1][0];
                _lastPosition = _frameData[_replayIndex][1] / 100f;
                _nextPositionTarget = _frameData[_replayIndex + 1][1] / 100f;

                SetCursorPosition(__instance, _frameData[_replayIndex][1] / 100f);
                if ((_frameData[_replayIndex][2] == 1 && !_isTooting) || (_frameData[_replayIndex][2] == 0 && _isTooting)) //if tooting state changes
                    ToggleTooting(__instance);
                _replayIndex++;
            }

        }

        public static void SetNoteScore(GameController __instance)
        {
            var note = _noteData.Find(x => x[0] == __instance.currentnoteindex);

            if (note != null)
            {
                __instance.totalscore = _totalScore = note[1]; //total score has to be set postfix because notes SOMEHOW still give more points than they should during replay...
                __instance.multiplier = note[2];
                __instance.currenthealth = note[3];
                _noteTally[note[4] - 1]++;
            }
        }

        public static void UpdateInstanceTotalScore(GameController __instance)
        {
            __instance.totalscore = _totalScore;
        }


        private static void OnTootStateChange(GameController __instance)
        {
            __instance.setPuppetShake(_isTooting);
            __instance.noteplaying = _isTooting;

            if (_isTooting) __instance.playNote();
            else __instance.stopNote();
        }

        private static void ToggleTooting(GameController __instance)
        {
            _isTooting = !_isTooting;
            OnTootStateChange(__instance);
        }
        #endregion


        private static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat + (secondFloat - firstFloat) * by;
        }

        private static void SetCursorPosition(GameController __instance, float newPosition)
        {
            Vector3 pointerPosition = __instance.pointer.transform.localPosition;
            pointerPosition.y = newPosition;
            __instance.pointer.transform.localPosition = pointerPosition;
        }
        private static void ClearData()
        {
            _frameData.Clear();
            _noteData.Clear();
        }
    }
}
