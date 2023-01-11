using BepInEx;
using HarmonyLib;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using TootTally.Graphics;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Networking;

namespace TootTally.Replays
{
    public static class ReplaySystemJson
    {
        public static SerializableClass.User userInfo; //Temporary public
        private static int _targetFramerate;
        private static int _scores_A, _scores_B, _scores_C, _scores_D, _scores_F, _totalScore;
        private static int[] _noteTally; // [nasties, mehs, okays, nices, perfects]
        private static int _frameIndex, _tootIndex;
        private static int _maxCombo;

        private static List<int[]> _frameData = new List<int[]>(), _noteData = new List<int[]>(), _tootData = new List<int[]>();
        private static DateTimeOffset _startTime, _endTime;

        public static bool wasPlayingReplay;
        private static bool _isReplayPlaying, _isReplayRecording;
        private static bool _wasTouchScreenUsed;
        private static bool _isTooting, _lastIsTooting, _hasReleaseToot;

        private static float _nextPositionTarget, _lastPosition;
        private static float _nextTimingTarget, _lastTiming;
        private static float _elapsedTime;

        private static string _replayUUID;
        private static string _replayFileName;


        #region GameControllerPatches

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void GameControllerPostfixPatch(GameController __instance)
        {
            if (_replayFileName == null)
            {
                ClearData();
                StartReplayRecorder(__instance);
            }
            __instance.notescoresamples = 0; //Temporary fix for a glitch
        }

        [HarmonyPatch(typeof(LoadController), nameof(LoadController.LoadGameplayAsync))]
        [HarmonyPrefix]
        public static void LoadControllerPrefixPatch(LoadController __instance)
        {
            if (_replayFileName != null)
                StartReplayPlayer(__instance);
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.isNoteButtonPressed))]
        [HarmonyPostfix]
        public static void GameControllerIsNoteButtonPressedPostfixPatch(GameController __instance, ref bool __result) // Take isNoteButtonPressed's return value and changed it to mine, hehe
        {

            if (_isReplayRecording && _hasReleaseToot && _lastIsTooting != __result)
            {
                RecordToot(__instance);
            }
            else if (_isReplayPlaying)
                __result = _isTooting;

            if (!__result && !_hasReleaseToot) //If joseph is holding the key before the song start
                _hasReleaseToot = true;
            _lastIsTooting = __result;
        }


        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
        [HarmonyPostfix]
        public static void PointSceneControllerPostfixPatch(PointSceneController __instance)
        {
            if (_isReplayRecording)
                StopReplayRecorder(__instance);
            else if (_isReplayPlaying)
            {
                StopReplayPlayer(__instance);
                GlobalVariables.localsave.tracks_played--;
            }

        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.doCoins))]
        [HarmonyPostfix]
        public static void ReplayIndicator(PointSceneController __instance)
        {
            if (_isReplayRecording) return; // Replay not running, an actual play happened
            // This code came from AutoToot (https://github.com/TomDotBat/AutoToot/blob/master/Patches/PointSceneControllerPatch.cs)
            GameObject tootTextObject = GameObject.Find("Canvas/buttons/coingroup/Text");
            if (tootTextObject == null)
            {
                Plugin.LogError("Could not find Toot Text object, cannot display replay indicator");
            }
            else
            {
                __instance.tootstext.text = "Replay Done";
            }
            __instance.Invoke(nameof(PointSceneController.showContinue), 0.75f);
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.updateSave))]
        [HarmonyPrefix]
        public static bool AvoidSaveChange(PointSceneController __instance)
        {
            return !wasPlayingReplay; // Don't touch the savefile if we just did a replay
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.checkScoreCheevos))]
        [HarmonyPrefix]
        public static bool AvoidAchievementCheck(PointSceneController __instance)
        {
            return !wasPlayingReplay; // Don't check for achievements if we just did a replay
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
                PrefixNoteData(__instance);
            else if (_isReplayPlaying)
                SetNoteScore(__instance);

        }


        [HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
        [HarmonyPostfix]
        public static void GameControllerGetScoreAveragePostfixPatch(GameController __instance)
        {
            if (_isReplayRecording)
                RecordNoteData(__instance);
            else if (_isReplayPlaying)
                UpdateInstanceTotalScore(__instance);
        }

        [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.showPausePanel))]
        [HarmonyPostfix]
        static void PauseCanvasControllerShowPausePanelPostfixPatch(PauseCanvasController __instance)
        {
            ClearData();
            _isReplayPlaying = _isReplayRecording = false;
            Plugin.LogInfo("Level paused, stopped " + (_isReplayPlaying ? "replay" : "recording") + " and cleared replay data");
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.pauseQuitLevel))]
        [HarmonyPostfix]
        static void GameControllerPauseQuitLevelPostfixPatch(GameController __instance)
        {
            _replayFileName = null;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void GetUserProfile(LevelSelectController __instance)
        {
            __instance.StartCoroutine(TootTallyAPIService.GetUser((user) =>
            {
                if (user != null)
                {
                    userInfo = user;
                }
            }));
        }
        #endregion

        #region ReplayRecorder
        private static void StartReplayRecorder(GameController __instance)
        {
            _isReplayRecording = _hasReleaseToot = true;
            wasPlayingReplay = false;
            _targetFramerate = Application.targetFrameRate > 60 || Application.targetFrameRate < 1 ? 60 : Application.targetFrameRate; //Could let the user choose replay framerate... but risky for when they will upload to our server
            _elapsedTime = 0;
            _scores_A = _scores_B = _scores_C = _scores_D = 0;
            _maxCombo = 0;
            _startTime = new DateTimeOffset(DateTime.Now.ToUniversalTime());

            Plugin.Instance.StartCoroutine(TootTallyAPIService.GetReplayUUID(GetChoosenSongHash(), (UUID) => _replayUUID = UUID));

            Plugin.LogInfo("Started recording replay");
        }

        private static void StopReplayRecorder(PointSceneController __instance)
        {
            _endTime = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            SaveAndUploadReplay(__instance);
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
                float noteHolderPosition = __instance.noteholder.transform.position.x * GetNoteHolderPrecisionMultiplier(); // the slower the scrollspeed , the better the precision
                float pointerPos = __instance.pointer.transform.localPosition.y * 100; // 2 decimal precision

                _frameData.Add(new int[] { (int)noteHolderPosition, (int)pointerPos });
            }
        }

        private static void PrefixNoteData(GameController __instance)
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

        private static void RecordToot(GameController __instance)
        {
            float noteHolderPosition = __instance.noteholder.transform.position.x * GetNoteHolderPrecisionMultiplier(); // the slower the scrollspeed , the better the precision

            _tootData.Add(new int[] { (int)noteHolderPosition });
        }

        private static void RecordNoteData(GameController __instance)
        {
            _maxCombo = __instance.highestcombo_level;
            var noteLetter = _scores_A != __instance.scores_A ? 4 :
               _scores_B != __instance.scores_B ? 3 :
               _scores_C != __instance.scores_C ? 2 :
               _scores_D != __instance.scores_D ? 1 : 0;

            _noteData[_noteData.Count - 1][(int)NoteDataStructure.NoteJudgement] = noteLetter;

        }

        private static void SaveAndUploadReplay(PointSceneController __instance)
        {
            if (AutoTootCompatibility.enabled && AutoTootCompatibility.WasAutoUsed) return; // Don't submit anything if AutoToot was used.
            if (HoverTootCompatibility.enabled && HoverTootCompatibility.DidToggleThisSong) return; // Don't submit anything if HoverToot was used.

            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            // Create Replays directory in case it doesn't exist
            if (!Directory.Exists(replayDir)) Directory.CreateDirectory(replayDir);

            string songNameLong = GlobalVariables.chosen_track_data.trackname_long, songNameShort = GlobalVariables.chosen_track_data.trackname_short;
            string trackRef = GlobalVariables.chosen_track_data.trackref;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            string songHash;
            if (!isCustom)
            {
                string songFilePath = Plugin.SongSelect.GetSongFilePath(false, trackRef);
                string tmb = Plugin.GenerateBaseTmb(songFilePath); 
                songHash = Plugin.Instance.CalcSHA256Hash(Encoding.UTF8.GetBytes(tmb));
            }
            else
                songHash = GetSongHash(trackRef);

            string username = userInfo.username;

            string startDateTimeUnix = _startTime.ToUnixTimeSeconds().ToString();
            string endDateTimeUnix = _endTime.ToUnixTimeSeconds().ToString();
            string replayFileName = $"{_replayUUID}";

            string inputType = _wasTouchScreenUsed ? "touch" : "mouse";
            var replayJson = new JSONObject();
            replayJson["username"] = username;
            replayJson["starttime"] = startDateTimeUnix;
            replayJson["endtime"] = endDateTimeUnix;
            replayJson["uuid"] = _replayUUID;
            replayJson["input"] = inputType;
            replayJson["song"] = songNameLong;
            replayJson["samplerate"] = _targetFramerate;
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

            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                {
                    var zipFile = zipArchive.CreateEntry(replayFileName);

                    using (var entry = zipFile.Open())
                    using (var sw = new StreamWriter(entry))
                    {
                        sw.Write(replayJson.ToString());
                    }
                }

                using (var fileStream = new FileStream(replayDir + replayFileName + ".ttr", FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }

            }

            Plugin.Instance.StartCoroutine(TootTallyAPIService.SubmitReplay(replayFileName + ".ttr", _replayUUID));

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
            Plugin.LogInfo("Optimizing Replay FrameData...");

            //Look for matching position and remove same frames with the same positions
            for (int i = 0; i < rawReplayFrameData.Count - 1; i++)
            {
                for (int j = i + 1; j < rawReplayFrameData.Count && rawReplayFrameData[i][(int)FrameDataStructure.PointerPosition] == rawReplayFrameData[j][(int)FrameDataStructure.PointerPosition];)
                {
                    rawReplayFrameData.Remove(rawReplayFrameData[j]);
                }
            }
        }

        private static void OptimizeTootData(ref List<int[]> rawReplayTootData)
        {
            Plugin.LogInfo("Optimizing Replay TootData...");

            for (int i = 0; i < rawReplayTootData.Count - 1; i++)
            {
                //if two toot happens on the same frame, probably is inputFix so unsync the frames
                if (rawReplayTootData[i][(int)TootDataStructure.NoteHolder] == rawReplayTootData[i + 1][(int)TootDataStructure.NoteHolder])
                {
                    rawReplayTootData[i][(int)TootDataStructure.NoteHolder]++;
                }

            }
        }

        private static void OptimizeNoteData(ref List<int[]> rawReplayNoteData)
        {
            Plugin.LogInfo("Optimizing Replay NoteData...");
            for (int i = 0; i < rawReplayNoteData.Count; i++)
            {
                if (rawReplayNoteData[i][(int)NoteDataStructure.NoteJudgement] == -1)
                    rawReplayNoteData.Remove(rawReplayNoteData[i]);
            }
        }

        private static bool ValidateData()
        {
            bool isValid = true;
            List<string> errorList = new List<string>();


            for (int i = 0; i < _frameData.Count; i++)
            {
                if (_frameData.FindAll(frame => frame[(int)FrameDataStructure.NoteHolder] == _frameData[i][(int)FrameDataStructure.NoteHolder]).Count > 1)
                {
                    isValid = false;
                    errorList.Add("Duplicate frames found, replay validation failed");
                    break;
                }
            }
            for (int i = 0; i < _noteData.Count; i++)
            {
                //if multiple notes has the same index
                if (_noteData.FindAll(note => note[(int)NoteDataStructure.NoteIndex] == _noteData[i][(int)NoteDataStructure.NoteIndex]).Count > 1)
                {
                    isValid = false;
                    errorList.Add("Duplicate note index found, replay validation failed");
                    break;
                }
            }

            if (_noteData.FindAll(note => note[(int)NoteDataStructure.NoteJudgement] == -1).Count > 1)
            {
                isValid = false;
                errorList.Add("Note Data is missing note judgement, replay validation failed");
            }

            if (!isValid)
                errorList.ForEach(error => Plugin.LogError(error));

            return isValid;
        }
        #endregion

        #region ReplayPlayer
        private static void StartReplayPlayer(LoadController __instance)
        {
            _lastTiming = 0;
            _totalScore = 0;
            _noteTally = new int[5];
            _isReplayPlaying = wasPlayingReplay = true;
            _isTooting = _lastIsTooting = false;
            Plugin.LogInfo("Starting replay: " + _replayFileName);
        }

        private static void StopReplayPlayer(PointSceneController __instance)
        {
            _isReplayPlaying = false;
            _replayFileName = null;
            GlobalVariables.gameplay_notescores = _noteTally;
            Plugin.LogInfo("Replay finished");
        }

        public static bool LoadReplay(string replayFileName)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            if (!Directory.Exists(replayDir))
            {
                Plugin.LogInfo("Replay folder not found");
                return false;
            }
            if (!File.Exists(replayDir + replayFileName + ".ttr"))
            {
                Plugin.LogInfo("Replay File does not exist");
                return false;
            }
            _replayFileName = replayFileName;


            string jsonFileFromZip;

            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = new FileStream(replayDir + replayFileName + ".ttr", FileMode.Open))
                {
                    fileStream.CopyTo(memoryStream);
                }

                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, true))
                {
                    var zipFile = zipArchive.GetEntry(zipArchive.Entries[0].Name);

                    using (var entry = zipFile.Open())
                    using (var sr = new StreamReader(entry))
                    {
                        jsonFileFromZip = sr.ReadToEnd();
                    }

                }
            }

            ClearData();
            var replayJson = JSONObject.Parse(jsonFileFromZip);
            GlobalVariables.gamescrollspeed = replayJson["scrollspeed"];
            foreach (JSONArray jsonArray in replayJson["framedata"])
                _frameData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2] });
            foreach (JSONArray jsonArray in replayJson["notedata"])
                _noteData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2], jsonArray[3], jsonArray[4] });
            foreach (JSONArray jsonArray in replayJson["tootdata"])
                _tootData.Add(new int[] { jsonArray[0] });
            _frameIndex = _tootIndex = 0;
            ValidateData();

            return true;
        }

        private static void PlaybackReplay(GameController __instance)
        {
            if (!__instance.controllermode) __instance.controllermode = true; //Still required to not make the mouse position update


            var currentMapPosition = __instance.noteholder.transform.position.x * GetNoteHolderPrecisionMultiplier();

            if (_frameData.Count > _frameIndex && _lastPosition != 0)
            {
                var newCursorPosition = Lerp(_lastPosition, _nextPositionTarget, (_lastTiming - currentMapPosition) / (_lastTiming - _nextTimingTarget));
                SetCursorPosition(__instance, newCursorPosition);
                __instance.puppet_humanc.doPuppetControl(-newCursorPosition / 225); //225 is half of the Gameplay area:450 
            }
            else
            {
                __instance.totalscore = _totalScore;
            }


            while (_frameData.Count > _frameIndex && _isReplayPlaying && currentMapPosition <= _frameData[_frameIndex][(int)FrameDataStructure.NoteHolder]) //smaller or equal to because noteholder goes toward negative
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

            while (_tootData.Count > _tootIndex && _isReplayPlaying && currentMapPosition <= _tootData[_tootIndex][(int)TootDataStructure.NoteHolder])
            {
                _isTooting = !_isTooting;
                _tootIndex++;
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
                int tallyIndex = Mathf.Clamp(note[(int)NoteDataStructure.NoteJudgement], 0, 4); //Temporary fix for note tally being -1 sometimes?
                _noteTally[tallyIndex]++;
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
            _tootData.Clear();
        }

        private static string GetChoosenSongHash()
        {
            string trackRef = GlobalVariables.chosen_track_data.trackref;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            return isCustom ? GetSongHash(trackRef) : trackRef;
        }
        public static string GetSongHash(string trackref) => Plugin.Instance.CalcFileHash(Plugin.SongSelect.GetSongFilePath(true, trackref));

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

        #endregion
    }
}
