using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TootTally.Compatibility;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Scripting;

namespace TootTally.Replays
{
    public static class ReplaySystemManager
    {
        public static List<string> incompatibleReplayPluginBuildDate = new List<string> { "20230106" };

        private static int _targetFramerate;
        public static bool wasPlayingReplay;
        private static bool _hasPaused;
        private static bool _hasReleaseToot, _lastIsTooting, _hasGreetedUser;

        private static float _elapsedTime;

        private static string _replayUUID;
        private static string _replayFileName;

        private static NewReplaySystem _replay;
        private static ReplayManagerState _replayManagerState;

        #region GameControllerPatches

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void GameControllerPostfixPatch(GameController __instance)
        {
            if (_replayFileName == null)
                OnRecordingStart(__instance);

            __instance.notescoresamples = 0; //Temporary fix for a glitch
            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;

        }

        [HarmonyPatch(typeof(LoadController), nameof(LoadController.LoadGameplayAsync))]
        [HarmonyPrefix]
        public static void LoadControllerPrefixPatch(LoadController __instance)
        {
            if (_replayFileName != null)
                OnReplayingStart();
            else
                OnLoadGamePlayAsyncSetupRecording(__instance);
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.isNoteButtonPressed))]
        [HarmonyPostfix]
        public static void GameControllerIsNoteButtonPressedPostfixPatch(GameController __instance, ref bool __result) // Take isNoteButtonPressed's return value and changed it to mine, hehe
        {
            switch (_replayManagerState)
            {
                case ReplayManagerState.Recording:
                    if (_hasReleaseToot && _lastIsTooting != __result)
                        _replay.RecordToot(__instance);
                    break;
                case ReplayManagerState.Replaying:
                    __result = _replay.GetIsTooting;
                    break;
            }

            if (!__result && !_hasReleaseToot) //If joseph is holding the key before the song start
                _hasReleaseToot = true;
            _lastIsTooting = __result;
        }


        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
        [HarmonyPostfix]
        public static void PointSceneControllerPostfixPatch(PointSceneController __instance)
        {
            switch (_replayManagerState)
            {
                case ReplayManagerState.Recording:
                    OnRecordingStop();
                    break;
                case ReplayManagerState.Replaying:
                    OnReplayingStop();
                    GlobalVariables.localsave.tracks_played--;
                    break;
            }


            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.doCoins))]
        [HarmonyPostfix]
        public static void ReplayIndicator(PointSceneController __instance)
        {
            if (!wasPlayingReplay) return; // Replay not running, an actual play happened
            __instance.tootstext.text = "Replay Done";
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.updateSave))]
        [HarmonyPrefix]
        public static bool AvoidSaveChange(PointSceneController __instance) => !wasPlayingReplay; // Don't touch the savefile if we just did a replay

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.checkScoreCheevos))]
        [HarmonyPrefix]
        public static bool AvoidAchievementCheck(PointSceneController __instance) => !wasPlayingReplay; // Don't check for achievements if we just did a replay

        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        [HarmonyPrefix]
        public static void GameControllerUpdatePrefixPatch(GameController __instance)
        {
            switch (_replayManagerState)
            {
                case ReplayManagerState.Recording:
                    _elapsedTime += Time.deltaTime;
                    if (_elapsedTime >= 1f / _targetFramerate)
                    {
                        _elapsedTime = 0;
                        _replay.RecordFrameData(__instance);
                    }
                    break;
                case ReplayManagerState.Replaying:
                    _replay.PlaybackReplay(__instance);
                    break;
            }
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
        [HarmonyPrefix]
        public static void GameControllerGetScoreAveragePrefixPatch(GameController __instance)
        {
            switch (_replayManagerState)
            {
                case ReplayManagerState.Recording:
                    _replay.RecordNoteDataPrefix(__instance);
                    break;
                case ReplayManagerState.Replaying:
                    _replay.SetNoteScore(__instance);
                    break;
            }
        }


        [HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
        [HarmonyPostfix]
        public static void GameControllerGetScoreAveragePostfixPatch(GameController __instance)
        {
            switch (_replayManagerState)
            {
                case ReplayManagerState.Recording:
                    _replay.RecordNoteDataPostfix(__instance);
                    break;
                case ReplayManagerState.Replaying:
                    _replay.UpdateInstanceTotalScore(__instance);
                    break;
            }

        }

        [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.showPausePanel))]
        [HarmonyPostfix]
        static void PauseCanvasControllerShowPausePanelPostfixPatch()
        {
            _replay.ClearData();
            _hasPaused = true;
            _replayManagerState = ReplayManagerState.Paused;
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            Plugin.LogInfo("Level paused, cleared replay data");
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.pauseQuitLevel))]
        [HarmonyPostfix]
        static void GameControllerPauseQuitLevelPostfixPatch(GameController __instance)
        {
            _replayManagerState = ReplayManagerState.None;
            _replayFileName = null;
            _replayUUID = null;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void OnLevelselectControllerStartInstantiateReplay(LevelSelectController __instance)
        {
            if (_replay == null)
                _replay = new NewReplaySystem();

            if (!_hasGreetedUser)
            {
                _hasGreetedUser = true;
                PopUpNotifManager.DisplayNotif($"Welcome, {Plugin.userInfo.username}!", Color.white, 9f);
            }
        }
        #endregion

        public static NewReplaySystem.ReplayState ResolveLoadReplay(string replayId, LevelSelectController levelSelectControllerInstance)
        {
            _replay.ClearData();
            NewReplaySystem.ReplayState replayState = _replay.LoadReplay(replayId);
            switch (replayState)
            {
                case NewReplaySystem.ReplayState.ReplayLoadSuccess:
                    _replayFileName = replayId;
                    levelSelectControllerInstance.playbtn.onClick?.Invoke();
                    break;

                case NewReplaySystem.ReplayState.ReplayLoadNotFound:
                    PopUpNotifManager.DisplayNotif("Downloading replay...", Color.white);
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadReplay(replayId, (uuid) =>
                    {
                        ResolveLoadReplay(uuid, levelSelectControllerInstance);
                    }));
                    break;

                case NewReplaySystem.ReplayState.ReplayLoadErrorIncompatible:
                    break;
                case NewReplaySystem.ReplayState.ReplayLoadError:
                    break;

            }
            return replayState;
        }


        public static void OnLoadGamePlayAsyncSetupRecording(LoadController __instance)
        {
            _replayUUID = null;
            string trackRef = GlobalVariables.chosen_track;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            string songFilePath = SongDataHelper.GetSongFilePath(trackRef);
            string songHash = isCustom ? SongDataHelper.CalcFileHash(songFilePath) : trackRef;

            StartAPICallCoroutine(__instance, songHash, songFilePath, isCustom);
        }

        public static void StartAPICallCoroutine(LoadController __instance, string songHash, string songFilePath, bool isCustom)
        {
            __instance.StartCoroutine(TootTallyAPIService.GetHashInDB(songHash, isCustom, (songHashInDB) =>
            {
                if (Plugin.Instance.AllowTMBUploads.Value && songHashInDB == 0)
                {
                    string tmb = File.ReadAllText(songFilePath, Encoding.UTF8);
                    SerializableClass.Chart chart = new SerializableClass.Chart { tmb = tmb };
                    __instance.StartCoroutine(TootTallyAPIService.AddChartInDB(chart, () =>
                    {
                        Plugin.Instance.StartCoroutine(TootTallyAPIService.GetReplayUUID(SongDataHelper.GetChoosenSongHash(), (UUID) => _replayUUID = UUID));
                    }));
                }
                else
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetReplayUUID(SongDataHelper.GetChoosenSongHash(), (UUID) => _replayUUID = UUID));

            }));
        }

        public static void OnRecordingStart(GameController __instance)
        {
            wasPlayingReplay = _hasPaused = _hasReleaseToot = false;
            _elapsedTime = 0;
            _targetFramerate = Application.targetFrameRate > 60 || Application.targetFrameRate < 1 ? 60 : Application.targetFrameRate; //Could let the user choose replay framerate... but risky for when they will upload to our server
            _replay.ClearData();
            _replay.SetupRecording(__instance);
            _replayManagerState = ReplayManagerState.Recording;
        }

        public static void OnReplayingStart()
        {
            _replay.OnReplayPlayerStart();
            _lastIsTooting = false;
            wasPlayingReplay = true;
            _replayManagerState = ReplayManagerState.Replaying;
            Plugin.LogInfo("Replay Started");
        }

        public static void OnRecordingStop()
        {
            _replay.FinalizedRecording();
            _replayManagerState = ReplayManagerState.None;


            if (AutoTootCompatibility.enabled && AutoTootCompatibility.WasAutoUsed) return; // Don't submit anything if AutoToot was used.
            if (HoverTootCompatibility.enabled && HoverTootCompatibility.DidToggleThisSong) return; // Don't submit anything if HoverToot was used.
            if (_hasPaused) return; //Don't submit if paused during the play

            SaveReplayToFile();
            SendReplayFileToServer();
        }

        private static void SaveReplayToFile()
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");

            // Create Replays directory in case it doesn't exist
            if (!Directory.Exists(replayDir)) Directory.CreateDirectory(replayDir);

            FileHelper.WriteJsonToFile(replayDir, _replayUUID, _replay.GetRecordedReplayJson(_replayUUID, _targetFramerate));

        }

        private static void SendReplayFileToServer()
        {
            //Using replayUUID as a name
            Plugin.Instance.StartCoroutine(TootTallyAPIService.SubmitReplay(_replayUUID + ".ttr", _replayUUID));
        }

        public static void OnReplayingStop()
        {
            _replay.OnReplayPlayerStop();
            _replayFileName = null;
            _replayManagerState = ReplayManagerState.None;
            Plugin.LogInfo("Replay finished");
        }


        public enum ReplayManagerState
        {
            None,
            Paused,
            Recording,
            Replaying
        }
    }
}
