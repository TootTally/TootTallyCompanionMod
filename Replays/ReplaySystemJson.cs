using BepInEx;
using HarmonyLib;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.IO.Compression;
using System.Text;
using TootTally.Graphics;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Networking;

namespace TootTally.Replays
{
    public static class ReplaySystemJson
    {
        private static SerializableClass.User _user;
        private static int _targetFramerate;
        private static int _scores_A, _scores_B, _scores_C, _scores_D, _scores_F, _totalScore;
        private static int[] _noteTally; // [nasties, mehs, okays, nices, perfects]
        private static int _replayIndex;
        private static int _lastTootingKeyPressed, _tootingKeyPressed;

        private static List<int[]> _frameData = new List<int[]>(), _noteData = new List<int[]>();
        private static DateTimeOffset _startTime, _endTime;

        public static bool wasPlayingReplay;
        private static bool _isReplayPlaying, _isReplayRecording;
        private static bool _isReplayBtnInitialized;
        private static bool _wasTouchScreenUsed;

        private static float _nextPositionTarget, _lastPosition;
        private static float _nextTimingTarget, _lastTiming;
        private static float _elapsedTime;

        public  static string replayFileName;

        #region GameControllerPatches

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void GameControllerPostfixPatch(GameController __instance)
        {
            if (replayFileName == null)
                StartReplayRecorder(__instance);
        }

        [HarmonyPatch(typeof(LoadController), nameof(LoadController.LoadGameplayAsync))]
        [HarmonyPrefix]
        public static void LoadControllerPrefixPatch(LoadController __instance)
        {
            ClearData();
            if (replayFileName != null)
                StartReplayPlayer(__instance);
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.isNoteButtonPressed))]
        [HarmonyPostfix]
        public static void GameControllerIsNoteButtonPressedPostfixPatch(ref bool __result) // Take isNoteButtonPressed's return value and changed it to mine, hehe
        {
            if (_isReplayPlaying)
            {
                __result = _tootingKeyPressed > 0 && _tootingKeyPressed == _lastTootingKeyPressed;
                _lastTootingKeyPressed = _tootingKeyPressed;
            }
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
                PlaybackReplay(__instance);
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
            replayFileName = null;
            Plugin.LogInfo("Level quit, clearing replay data");
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void GetUserProfile(LevelSelectController __instance)
        {
            __instance.StartCoroutine(TootTallyAPIService.GetUser((user) => {
                if (user != null) {
                    _user = user;
                }
            }));
        }
        #endregion

        #region Config

        public static string GetSongHash(string trackref) => Plugin.Instance.CalcFileHash(Plugin.SongSelect.GetSongFilePath(true, trackref));

        public static void ReadReplayConfig(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            SingleTrackData trackData = ___alltrackslist[__instance.songindex];
            string trackref = trackData.trackref;
            bool isCustom = Globals.IsCustomTrack(trackref);
            if (isCustom)
            {
                string songName = trackData.trackname_short;
                string songHash = GetSongHash(trackref);
                if (songHash != null)
                    ReplayConfig.ReadConfig($"{songName} - {songHash}");
            }
            else
            {
                string songName = trackData.trackname_short;
                ReplayConfig.ReadConfig($"{songName}");
            }
        }
        public static void ReadReplayConfig()
        {
            string trackref = GlobalVariables.chosen_track_data.trackref;
            bool isCustom = Globals.IsCustomTrack(trackref);
            if (isCustom)
            {
                string songName = GlobalVariables.chosen_track_data.trackname_short;
                string songHash = GetSongHash(trackref);
                if (songHash != null)
                    ReplayConfig.ReadConfig($"{songName} - {songHash}");
            }
        }
        #endregion

        #region ReplayRecorder
        private static void StartReplayRecorder(GameController __instance)
        {
            _isReplayRecording = true;
            wasPlayingReplay = false;
            _targetFramerate = Application.targetFrameRate > 60 || Application.targetFrameRate < 1 ? 60 : Application.targetFrameRate; //Could let the user choose replay framerate... but risky for when they will upload to our server
            _elapsedTime = 0;
            _scores_A = _scores_B = _scores_C = _scores_D = 0;
            _startTime = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            Plugin.LogInfo("Started recording replay");
        }

        private static void StopReplayRecorder(PointSceneController __instance)
        {
            _endTime = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            SaveReplayToFile(__instance);
            _isReplayRecording = false;
            Plugin.LogInfo("Replay recording finished");
        }

        private static void RecordFrameDataV2(GameController __instance)
        {
            if (Input.touchCount > 0) _wasTouchScreenUsed = true;
            float deltaTime = Time.deltaTime;
            _elapsedTime += deltaTime;
            if (_isReplayRecording && _elapsedTime >= 1f / _targetFramerate)
            {
                _elapsedTime = 0;
                float noteHolderPosition = __instance.noteholder.transform.position.x * 10; // 1 decimal precision
                float pointerPos = __instance.pointer.transform.localPosition.y * 100; // 2 decimal precision
                int isTootingV2 = __instance.toot_keys.FindAll(Input.GetKey).Count;
                _frameData.Add(new int[] { (int)noteHolderPosition, (int)pointerPos, isTootingV2 });
            }
        }

        private static void RecordNote(GameController __instance)
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


            _noteData.Add(new int[] { noteIndex, totalScore, multiplier, (int)currentHealth, -1 }); // has to do the note judgement on postfix
        }

        private static void AddNoteJudgementToNoteData(GameController __instance)
        {
            var noteLetter = _scores_A != __instance.scores_A ? 4 :
               _scores_B != __instance.scores_B ? 3 :
               _scores_C != __instance.scores_C ? 2 :
               _scores_D != __instance.scores_D ? 1 : 0;
            _noteData[_noteData.Count - 1][(int)NoteDataStructure.NoteJudgement] = noteLetter;
        }

        private static void SaveReplayToFile(PointSceneController __instance)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            // Create Replays directory in case it doesn't exist
            if (!Directory.Exists(replayDir)) Directory.CreateDirectory(replayDir);

            string songNameLong = GlobalVariables.chosen_track_data.trackname_long, songNameShort = GlobalVariables.chosen_track_data.trackname_short;
            string trackRef = GlobalVariables.chosen_track_data.trackref;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            string songHash = isCustom ? GetSongHash(trackRef) : "ost";

            string username = _user.username;

            string startDateTimeUnix = _startTime.ToUnixTimeSeconds().ToString();
            string endDateTimeUnix = _endTime.ToUnixTimeSeconds().ToString();
            string replayFileName = $"{username} - {songNameShort} - {endDateTimeUnix}";

            string inputType = _wasTouchScreenUsed ? "touch" : "mouse";
            var replayJson = new JSONObject();
            replayJson["username"] = username;
            replayJson["starttime"] = startDateTimeUnix;
            replayJson["endtime"] = endDateTimeUnix;
            replayJson["input"] = inputType;
            replayJson["song"] = songNameLong;
            replayJson["samplerate"] = _targetFramerate;
            replayJson["scrollspeed"] = GlobalVariables.gamescrollspeed;
            replayJson["pluginbuilddate"] = Plugin.BUILDDATE;
            replayJson["gameversion"] = GlobalVariables.version;
            replayJson["songhash"] = isCustom ? songHash : "ost";
            replayJson["finalscore"] = GlobalVariables.gameplay_scoretotal;

            var noteJudgmentData = new JSONArray();
            noteJudgmentData.Add(_scores_A); noteJudgmentData.Add(_scores_B); noteJudgmentData.Add(_scores_C); noteJudgmentData.Add(_scores_D); noteJudgmentData.Add(_scores_F);
            replayJson["finalnotetallies"] = noteJudgmentData;
            var replayFrameData = new JSONArray();
            OptimizeFrameData(ref _frameData);
            _frameData.ForEach(frame =>
            {
                var frameDataJsonArray = new JSONArray();
                foreach (var frameData in frame)
                    frameDataJsonArray.Add(frameData);
                replayFrameData.Add(frameDataJsonArray);
            });
            replayJson["framedata"] = replayFrameData;


            _noteData[_noteData.Count - 1][1] = GlobalVariables.gameplay_scoretotal; // Manually set the last note's totalscore to the actual totalscore because game is weird...
            var replayNoteData = new JSONArray();
            _noteData.ForEach(notes =>
            {
                var noteDataJsonArray = new JSONArray();
                foreach (var noteData in notes)
                    noteDataJsonArray.Add(noteData);
                replayNoteData.Add(noteDataJsonArray);
            });
            replayJson["notedata"] = replayNoteData;

            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var zipFile = zipArchive.CreateEntry(replayFileName);

                    using (var entry = zipFile.Open())
                    using (var sw = new StreamWriter(entry))
                    {
                        sw.Write(replayJson.ToString());
                    }
                }

                using (var fileStream = new FileStream(replayDir + replayFileName, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }

            //Send Replay to server async here

            ReadReplayConfig();
            ReplayConfig.SaveToConfig(replayFileName);

        }


        private static void OptimizeFrameData(ref List<int[]> rawReplayFrameData)
        {
            Plugin.LogInfo("Optimizing Replay...");

            //Look for matching position && tooting values and remove same frames with the same positions
            for (int i = 0; i < rawReplayFrameData.Count - 1; i++)
            {
                for (int j = i + 1; j < rawReplayFrameData.Count && rawReplayFrameData[i][1] == rawReplayFrameData[j][1] && rawReplayFrameData[i][2] == rawReplayFrameData[j][2];)
                {
                    rawReplayFrameData.Remove(rawReplayFrameData[j]);
                }
            }
        }

        private static void OptimizeNoteData(ref List<int[]> rawReplayNoteData)
        {
            for (int i = 0; i < rawReplayNoteData.Count; i++)
            {
                //todo
            }
        }
        #endregion

        #region ReplayPlayer
        private static void StartReplayPlayer(LoadController __instance)
        {
            _lastTiming = 0;
            _totalScore = 0;
            _noteTally = new int[5];
            _isReplayPlaying = wasPlayingReplay = true;
            _lastTootingKeyPressed = _tootingKeyPressed = 0;
            Plugin.LogInfo("Loading replay: " + replayFileName);
            if (LoadReplay(replayFileName))
                Plugin.LogInfo("Started replay.");
            else
            {
                Plugin.LogError("Replay failed to start.");
                replayFileName = null;
            }
        }

        private static void StopReplayPlayer(PointSceneController __instance)
        {
            _isReplayPlaying = false;
            GlobalVariables.gameplay_notescores = _noteTally;
            Plugin.LogInfo("Replay finished");
        }

        private static bool LoadReplay(string replayFileName)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            if (!Directory.Exists(replayDir))
            {
                Plugin.LogInfo("Replay folder not found");
                return false;
            }

            if (!File.Exists(replayDir + replayFileName))
            {
                Plugin.LogInfo("Replay File not found");
                return false;
            }

            string jsonFileFromZip;

            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = new FileStream(replayDir + replayFileName, FileMode.Open))
                {
                    fileStream.CopyTo(memoryStream);
                }

                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, true))
                {
                    var zipFile = zipArchive.GetEntry(replayFileName);

                    using (var entry = zipFile.Open())
                    using (var sr = new StreamReader(entry))
                    {
                        jsonFileFromZip = sr.ReadToEnd();
                    }

                }
            }

            var replayJson = JSONObject.Parse(jsonFileFromZip);
            GlobalVariables.gamescrollspeed = replayJson["scrollspeed"];
            foreach (JSONArray jsonArray in replayJson["framedata"])
                _frameData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2] });
            foreach (JSONArray jsonArray in replayJson["notedata"])
                _noteData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2], jsonArray[3], jsonArray[4] });
            _replayIndex = 0;
            return true;
        }

        private static void PlaybackReplay(GameController __instance)
        {
            if (!__instance.controllermode) __instance.controllermode = true; //Still required to not make the mouse position update

            var currentMapPosition = __instance.noteholder.transform.position.x * 10;

            if (_frameData.Count > _replayIndex && _lastPosition != 0)
            {
                var newCursorPosition = Lerp(_lastPosition, _nextPositionTarget, (_lastTiming - currentMapPosition) / (_lastTiming - _nextTimingTarget));
                SetCursorPosition(__instance, newCursorPosition);
                __instance.puppet_humanc.doPuppetControl(-newCursorPosition / 225); //225 is half of the Gameplay area:450 
            }
            else
            {
                __instance.totalscore = _totalScore;
            }


            while (_frameData.Count > _replayIndex && _isReplayPlaying && currentMapPosition <= _frameData[_replayIndex][(int)FrameDataStructure.NoteHolder]) //smaller or equal to because noteholder goes toward negative
            {
                _lastTiming = _frameData[_replayIndex][(int)FrameDataStructure.NoteHolder];
                _lastPosition = _frameData[_replayIndex][(int)FrameDataStructure.PointerPosition] / 100f;
                if (_replayIndex < _frameData.Count - 1)
                {
                    _nextTimingTarget = _frameData[_replayIndex + 1][(int)FrameDataStructure.NoteHolder];
                    _nextPositionTarget = _frameData[_replayIndex + 1][(int)FrameDataStructure.PointerPosition] / 100f;
                }
                else
                {
                    _nextTimingTarget = _lastTiming;
                    _nextPositionTarget = _lastPosition;
                }


                SetCursorPosition(__instance, _frameData[_replayIndex][(int)FrameDataStructure.PointerPosition] / 100f);

                _tootingKeyPressed = _frameData[_replayIndex][(int)FrameDataStructure.IsTooting];
                _replayIndex++;
            }

        }

        private static void SetNoteScore(GameController __instance)
        {
            var note = _noteData.Find(x => x[(int)NoteDataStructure.NoteIndex] == __instance.currentnoteindex);

            if (note != null)
            {
                __instance.totalscore = _totalScore = note[(int)NoteDataStructure.TotalScore]; //total score has to be set postfix as well because notes SOMEHOW still give more points than they should during replay...
                __instance.multiplier = note[(int)NoteDataStructure.Multiplier];
                __instance.currenthealth = note[(int)NoteDataStructure.CurrentHealth];
                _noteTally[note[(int)NoteDataStructure.NoteJudgement]]++;
            }
        }

        private static void UpdateInstanceTotalScore(GameController __instance)
        {
            __instance.totalscore = _totalScore;
        }

        #endregion

        #region Utils
        private static float Lerp(float firstFloat, float secondFloat, float by) //Linear easing
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

        private enum FrameDataStructure
        {
            NoteHolder = 0,
            PointerPosition = 1,
            IsTooting = 2,
        }

        private enum NoteDataStructure
        {
            NoteIndex = 0,
            TotalScore = 1,
            Multiplier = 2,
            CurrentHealth = 3,
            NoteJudgement = 4,
        }

        #endregion
    }
}
