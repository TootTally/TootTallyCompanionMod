using BepInEx;
using HarmonyLib;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TootTally
{
    public static class ReplaySystemJson
    {
        private static int _targetFramerate;
        private static List<int[]> _frameData = new List<int[]>(), _noteData = new List<int[]>();
        private static float _elapsedTime;
        private static bool _isTooting, _isReplayPlaying, _isReplayRecording;
        private static int _replayIndex;
        private static float _nextPositionTarget, _lastPosition;
        private static float _nextTimingTarget, _lastTiming;
        private static int _scores_A, _scores_B, _scores_C, _scores_D;

        /*
        #region ReplayRecorder
        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void StartReplayRecorder(GameController __instance)
        {
            _frameData.Clear();
            _isReplayRecording = true;
            _targetFramerate = Application.targetFrameRate > 60 || Application.targetFrameRate < 1 ? 60 : Application.targetFrameRate;
            _elapsedTime = 0;
            _scores_A = _scores_B = _scores_C = _scores_D = 0;
            Plugin.LogInfo("Started recording replay");
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
        [HarmonyPostfix]
        public static void StopReplayRecorder(PointSceneController __instance)
        {
            SaveReplayToFile(__instance);
            _isReplayRecording = false;
            Plugin.LogInfo("Replay recording finished");
        }


        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        [HarmonyPrefix]
        public static void RecordFrameDataV2(GameController __instance)
        {
            float deltaTime = Time.deltaTime;
            _elapsedTime += deltaTime;
            if (_isReplayRecording && _elapsedTime >= 1f / _targetFramerate)
            {
                _elapsedTime = 0;
                float noteHolderPosition = __instance.noteholder.transform.localPosition.x * 10; // 1 decimal precision
                float pointerPos = __instance.pointer.transform.localPosition.y * 100; //times 100 and convert to int for 2 decimal precision
                bool isTooting = __instance.noteplaying;
                _frameData.Add(new int[] { (int)noteHolderPosition, (int)pointerPos, isTooting ? 1 : 0 });
            }
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
        [HarmonyPrefix]
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

        [HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
        [HarmonyPostfix]
        public static void AddNoteJudgementToNoteData(GameController __instance)
        {
            var noteLetter = _scores_A != __instance.scores_A ? 1 :
               _scores_B != __instance.scores_B ? 2 :
               _scores_C != __instance.scores_C ? 3 :
               _scores_D != __instance.scores_D ? 4 : 5;
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
                if (rawReplayNoteData[i][1] == 0)
                    rawReplayNoteData.Remove(rawReplayNoteData[i]);
            }
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.pauseRetryLevel))]
        [HarmonyPostfix]
        static void OnPauseStopRecording(GameController __instance)
        {
            _isReplayRecording = false;
            Plugin.LogInfo("Replay Stopped");
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.pauseQuitLevel))]
        [HarmonyPostfix]
        static void OnQuitDeleteReplay(GameController __instance)
        {
            _frameData.Clear();
            Plugin.LogInfo("Replay Deleted");
        }

        #endregion
        */


        
        #region ReplayPlayer
        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void StartReplayPlayer(GameController __instance)
        {
            _frameData.Clear();
            _noteData.Clear();
            _lastTiming = 0;
            _isTooting = false;
            _isReplayPlaying = true;
            LoadReplay("TestUser - Koi wa Chaos - 1671315489");
            Plugin.LogInfo("Started replay");
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
        [HarmonyPostfix]
        public static void StopReplayPlayer(PointSceneController __instance)
        {
            _isReplayPlaying = false;
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
            foreach (JSONArray jsonArray in replayJson["framedata"])
                _frameData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2] });
            foreach (JSONArray jsonArray in replayJson["notedata"])
                _noteData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2], jsonArray[3], jsonArray[4], jsonArray[5], jsonArray[6] });
            _replayIndex = 0;

        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        [HarmonyPrefix]
        public static void PlaybackReplayV2(GameController __instance)
        {
            if (!__instance.controllermode) __instance.controllermode = true;

            var currentMapPosition = __instance.noteholder.transform.localPosition.x * 10;



            if (_frameData.Count >= _replayIndex && _lastTiming != 0)
            {
                var newCursorPosition = Lerp(_lastPosition, _nextPositionTarget, (_lastTiming - currentMapPosition) / (_lastTiming - _nextTimingTarget));
                SetCursorPosition(__instance, newCursorPosition);
            }


            while (_frameData.Count >= _replayIndex && _isReplayPlaying && currentMapPosition <= _frameData[_replayIndex][0]) //smaller or equal to because noteholder goes toward negative
            {
                _lastTiming = _frameData[_replayIndex][0];
                _nextTimingTarget = _frameData[_replayIndex][1];
                _lastPosition = _frameData[_replayIndex][1] / 100f;
                _nextPositionTarget = _frameData[_replayIndex + 1][1] / 100f;

                SetCursorPosition(__instance, _frameData[_replayIndex][1] / 100f);
                if ((_frameData[_replayIndex][2] == 1 && !_isTooting) || (_frameData[_replayIndex][2] == 0 && _isTooting)) //if tooting state changes
                    ToggleTooting(__instance);
                _replayIndex++;
            }



        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
        [HarmonyPrefix]
        public static void SetNoteScore(GameController __instance)
        {
            var note = _noteData.Find(x => x[0] == __instance.currentnoteindex);

            if (note != null)
            {
                __instance.totalscore = note[1];
                __instance.multiplier = note[2];
                __instance.currenthealth = note[3];
                switch (note[4])
                {
                    case 1:
                        __instance.scores_A++;
                        break;
                    case 2:
                        __instance.scores_B++;
                        break;
                    case 3:
                        __instance.scores_C++;
                        break;
                    case 4:
                        __instance.scores_D++;
                        break;
                    default:
                        __instance.scores_F++;
                        break;
                }
            }
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

        [HarmonyPatch(typeof(GameController), nameof(GameController.pauseRetryLevel))]
        [HarmonyPostfix]
        static void OnPauseStopReplay(GameController __instance)
        {
            __instance.controllermode = false;
            _isReplayPlaying = false;
            Plugin.LogInfo("Replay finished");
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
    }
}
