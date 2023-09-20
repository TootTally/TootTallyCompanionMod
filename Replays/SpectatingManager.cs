using BaboonAPI.Hooks.Tracks;
using HarmonyLib;
using Microsoft.FSharp.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TootTally.CustomLeaderboard;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using UnityEngine;

namespace TootTally.Replays
{
    public class SpectatingManager : MonoBehaviour
    {
        public static JsonConverter[] _dataConverter = new JsonConverter[] { new SocketDataConverter() };
        private static List<SpectatingSystem> _spectatingSystemList;
        public static SpectatingSystem hostedSpectatingSystem;
        public static bool IsHosting => hostedSpectatingSystem != null && hostedSpectatingSystem.IsConnected && hostedSpectatingSystem.IsHost;
        public static bool IsSpectating => _spectatingSystemList != null && !IsHosting && _spectatingSystemList.Any(x => x.IsConnected);

        public void Awake()
        {
            _spectatingSystemList ??= new List<SpectatingSystem>();
            if (Plugin.Instance.AllowSpectate.Value)
                CreateUniqueSpectatingConnection(Plugin.userInfo.id);
        }

        public void Update()
        {
            _spectatingSystemList?.ForEach(s => s.UpdateStacks());

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Escape) && _spectatingSystemList.Count > 0 && !IsHosting)
                _spectatingSystemList.Last().RemoveFromManager();
        }

        public static void StopAllSpectator()
        {
            if (_spectatingSystemList != null)
            {
                for (int i = 0; i < _spectatingSystemList.Count;)
                    RemoveSpectator(_spectatingSystemList[i]);
                if (hostedSpectatingSystem != null && hostedSpectatingSystem.IsConnected)
                    hostedSpectatingSystem = null;
            }
        }

        public static SpectatingSystem CreateNewSpectatingConnection(int id)
        {
            var spec = new SpectatingSystem(id);
            _spectatingSystemList.Add(spec);
            if (id == Plugin.userInfo.id)
                hostedSpectatingSystem = spec;
            return spec;
        }

        public static void RemoveSpectator(SpectatingSystem spectator)
        {
            if (spectator == null) return;

            if (spectator.IsConnected)
                spectator.Disconnect();
            if (_spectatingSystemList.Contains(spectator))
                _spectatingSystemList.Remove(spectator);
            else
                TootTallyLogger.LogInfo($"Couldnt find websocket in list.");
        }

        public static SpectatingSystem CreateUniqueSpectatingConnection(int id)
        {
            StopAllSpectator();
            return CreateNewSpectatingConnection(id);
        }

        public static void OnAllowHostConfigChange(bool value)
        {
            if (value && hostedSpectatingSystem == null)
                CreateUniqueSpectatingConnection(Plugin.userInfo.id);
            else if (!value && hostedSpectatingSystem != null)
            {
                RemoveSpectator(hostedSpectatingSystem);
                hostedSpectatingSystem = null;
            }
        }

        public static bool IsAnyConnectionPending() => _spectatingSystemList.Any(x => x.ConnectionPending);

        public enum DataType
        {
            UserState,
            SongInfo,
            FrameData,
            TootData,
        }

        public enum UserState
        {
            SelectingSong,
            Paused,
            Playing,
            Restarting,
            Quitting,
            PointScene,
        }

        public class SocketMessage
        {
            public string dataType { get; set; }
        }

        public class SocketUserState : SocketMessage
        {
            public int userState { get; set; }
        }

        public class SocketFrameData : SocketMessage
        {
            public float time { get; set; }
            public float noteHolder { get; set; }
            public float pointerPosition { get; set; }
            public int totalScore { get; set; }
            public int highestCombo { get; set; }
            public int currentCombo { get; set; }
            public float health { get; set; }
        }

        public class SocketTootData : SocketMessage
        {
            public float noteHolder { get; set; }
            public bool isTooting { get; set; }
        }

        public class SocketSongInfo : SocketMessage
        {
            public string trackRef { get; set; }
            public int songID { get; set; }
            public float gameSpeed { get; set; }
            public float scrollSpeed { get; set; }
        }

        public class SocketDataConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(SocketMessage);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jo = JObject.Load(reader);
                if (jo["dataType"].Value<string>() == DataType.UserState.ToString())
                    return jo.ToObject<SocketUserState>(serializer);

                if (jo["dataType"].Value<string>() == DataType.FrameData.ToString())
                    return jo.ToObject<SocketFrameData>(serializer);

                if (jo["dataType"].Value<string>() == DataType.TootData.ToString())
                    return jo.ToObject<SocketTootData>(serializer);

                if (jo["dataType"].Value<string>() == DataType.SongInfo.ToString())
                    return jo.ToObject<SocketSongInfo>(serializer);

                return null;
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        #region patches
        public static class SpectatorManagerPatches
        {
            private static LevelSelectController _levelSelectControllerInstance;
            private static GameController _gameControllerInstance;
            private static PointSceneController _pointSceneControllerInstance;

            private static UserState _lastSpecState, _currentSpecState;

            private static List<SocketFrameData> _frameData = new List<SocketFrameData>();
            private static List<SocketTootData> _tootData = new List<SocketTootData>();
            private static SocketFrameData _lastFrame, _currentFrame;

            private static SocketSongInfo _lastSongInfo;

            private static bool _isTooting;
            private static int _frameIndex;
            private static int _tootIndex;
            private static bool _waitingToSync;
            private static bool _spectatingStarting;
            private static bool _leftPointSelectScreen;

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
            [HarmonyPostfix]
            public static void SetLevelSelectUserStatusOnAdvanceSongs(LevelSelectController __instance)
            {
                _gameControllerInstance = null;
                _pointSceneControllerInstance = null;
                _levelSelectControllerInstance = __instance;
                _spectatingStarting = false;
                _lastSongInfo = null;
                if (IsHosting)
                    SetCurrentUserState(UserState.SelectingSong);
            }


            [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
            [HarmonyPostfix]
            public static void OnGameControllerStart(GameController __instance)
            {
                _pointSceneControllerInstance = null;
                _levelSelectControllerInstance = null;
                _gameControllerInstance = __instance;
                _waitingToSync = IsSpectating;
                if (IsSpectating)
                {
                    _frameIndex = 0;
                    _tootIndex = 0;
                    _lastFrame = null;
                    _currentFrame = null;
                        /*new SocketFrameData()
                    {
                        currentCombo = 0,
                        health = 0,
                        highestCombo = 0,
                        pointerPosition = 0,
                        time = 0,
                        noteHolder = 0,
                        totalScore = 0,
                    };*/
                    _elapsedTime = 0;
                }
            }

            private const float SYNC_BUFFER = 1.5f;

            [HarmonyPatch(typeof(GameController), nameof(GameController.startSong))]
            [HarmonyPrefix]
            public static bool OverwriteStartSongIfSyncRequired(GameController __instance)
            {
                if (!IsSpectating) return true;

                if (ShouldWaitForSync(out _waitingToSync))
                    PopUpNotifManager.DisplayNotif("Waiting to sync with host...");

                return !_waitingToSync;
            }

            private static bool ShouldWaitForSync(out bool waitForSync)
            {
                waitForSync = true;

                if (_frameData != null && _frameData.Count > 0 && _frameData.Last().time >= SYNC_BUFFER)
                    waitForSync = false;
                return waitForSync;
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.startSong))]
            [HarmonyPostfix]
            public static void SetPlayingUserStatus(GameController __instance)
            {
                if (IsSpectating)
                {
                    if (_frameData != null && _frameData.Count > 0 && _frameData[_frameIndex].time - __instance.musictrack.time >= SYNC_BUFFER)
                    {
                        TootTallyLogger.LogInfo("Syncing track with replay data...");
                        __instance.musictrack.time = _frameData[_frameIndex].time;
                        __instance.noteholderr.anchoredPosition = new Vector2(_frameData[_frameIndex].noteHolder, __instance.noteholderr.anchoredPosition.y);
                    }
                }
                if (IsHosting)
                    SetCurrentUserState(UserState.Playing);
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.isNoteButtonPressed))]
            [HarmonyPostfix]
            public static void GameControllerIsNoteButtonPressedPostfixPatch(ref bool __result) // Take isNoteButtonPressed's return value and changed it to mine, hehe
            {
                if (IsSpectating)
                    __result = _isTooting;
            }

            [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
            [HarmonyPostfix]
            public static void OnPointSceneControllerStartSetInstance(PointSceneController __instance) // Take isNoteButtonPressed's return value and changed it to mine, hehe
            {
                _levelSelectControllerInstance = null;
                _gameControllerInstance = null;
                _pointSceneControllerInstance = __instance;
                if (IsHosting)
                    SetCurrentUserState(UserState.PointScene);
            }

            [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Update))]
            [HarmonyPostfix]
            public static void GetOutOfPointSceneIfHostLeft(PointSceneController __instance)
            {
                if (IsSpectating && _lastSpecState == UserState.PointScene && !__instance.clickedleave)
                    if (_currentSpecState == UserState.SelectingSong)
                        __instance.clickCont();
                    else if (_currentSpecState == UserState.Playing)
                        __instance.clickRetry();
            }

            public static void PlaybackSpectatingData(GameController __instance)
            {
                Cursor.visible = true;
                if (_frameData == null || _tootData == null) return;

                if (!__instance.controllermode) __instance.controllermode = true; //Still required to not make the mouse position update

                var currentMapPosition = __instance.noteholderr.anchoredPosition.x;

                if (_frameData.Count - 2 > _frameIndex && _lastFrame != null && _currentFrame != null)
                    InterpolateCursorPosition(currentMapPosition, __instance);
                if (_frameData.Count > 0)
                    PlaybackFrameData(currentMapPosition, __instance);
                if (_tootData.Count > 0)
                    PlaybackTootData(currentMapPosition, __instance);
            }

            private static void InterpolateCursorPosition(float currentMapPosition, GameController __instance)
            {
                var newCursorPosition = EasingHelper.Lerp(_lastFrame.pointerPosition, _currentFrame.pointerPosition, (_lastFrame.noteHolder - currentMapPosition) / (_lastFrame.noteHolder - _currentFrame.noteHolder));
                SetCursorPosition(__instance, newCursorPosition);
                __instance.puppet_humanc.doPuppetControl(-newCursorPosition / 225); //225 is half of the Gameplay area:450
            }

            private static void PlaybackFrameData(float currentMapPosition, GameController __instance)
            {

                if (_currentFrame != null && currentMapPosition <= _currentFrame.noteHolder)
                {
                    _lastFrame = _currentFrame;
                }

                while (_currentFrame == null || (_frameData.Count - 2 > _frameIndex && currentMapPosition <= _currentFrame.noteHolder)) //smaller or equal to because noteholder goes toward negative
                {
                    _currentFrame = _frameData[++_frameIndex];
                    if (_currentFrame != null)
                    {
                        SetCursorPosition(__instance, _currentFrame.pointerPosition);
                        __instance.totalscore = _currentFrame.totalScore;
                        __instance.currenthealth = _currentFrame.health;
                        __instance.highestcombo_level = _currentFrame.highestCombo;
                        __instance.highestcombocounter = _currentFrame.currentCombo;
                    }
                       
                }
            }

            private static void SetCursorPosition(GameController __instance, float newPosition)
            {
                Vector3 pointerPosition = __instance.pointer.transform.localPosition;
                pointerPosition.y = newPosition;
                __instance.pointer.transform.localPosition = pointerPosition;
            }

            public static void OnFrameDataReceived(int id, SocketFrameData frameData)
            {
                _frameData?.Add(frameData);
            }

            public static void OnTootDataReceived(int id, SocketTootData tootData)
            {
                _tootData?.Add(tootData);
            }

            public static void PlaybackTootData(float currentMapPosition, GameController __instance)
            {
                if (_tootData.Count - 2 > _tootIndex && currentMapPosition <= _tootData[_tootIndex].noteHolder) //smaller or equal to because noteholder goes toward negative
                {
                    _isTooting = _tootData[++_tootIndex].isTooting;
                }
            }

            public static void OnSongInfoReceived(int id, SocketSongInfo info)
            {
                if (info == null || info.trackRef == null || info.gameSpeed <= 0f)
                {
                    TootTallyLogger.LogInfo("SongInfo went wrong.");
                    return;
                }
                _lastSongInfo = info;
            }

            public static void OnUserStateReceived(int id, SocketUserState userState)
            {
                if (IsSpectating)
                    UserStateHandler((UserState)userState.userState);
            }

            private static void UserStateHandler(UserState state)
            {
                _lastSpecState = _currentSpecState;
                _currentSpecState = state;
                switch (state)
                {
                    case UserState.SelectingSong:
                        if (_pointSceneControllerInstance != null)
                            BackToLevelSelect();
                        break;
                    case UserState.Playing:
                        if (_levelSelectControllerInstance != null)
                            TryStartSong();
                        else if (_gameControllerInstance != null && _lastSpecState == UserState.Paused)
                            ResumeSong();
                        else if (_pointSceneControllerInstance != null)
                            RetryFromPointScene();
                        break;
                    case UserState.Paused:
                        if (_gameControllerInstance != null)
                            PauseSong();
                        break;
                    case UserState.Quitting:
                        if (_gameControllerInstance != null)
                            QuitSong();
                        break;
                    case UserState.Restarting:
                        if (_gameControllerInstance != null)
                            RestartSong();
                        break;
                }
            }


            private static void BackToLevelSelect()
            {
                _lastSongInfo = null;
                _pointSceneControllerInstance.clickCont();
            }

            private static void RetryFromPointScene()
            {
                _frameData.Clear();
                _tootData.Clear();
                _frameIndex = 0;
                _tootIndex = 0;
                _isTooting = false;
                ReplaySystemManager.gameSpeedMultiplier = _lastSongInfo.gameSpeed;
                GlobalVariables.gamescrollspeed = _lastSongInfo.scrollSpeed;
                TootTallyLogger.LogInfo("ScrollSpeed Set: " + _lastSongInfo.scrollSpeed);
                _pointSceneControllerInstance.clickRetry();
            }

            private static void TryStartSong()
            {
                if (_lastSongInfo != null)
                    if (!FSharpOption<TromboneTrack>.get_IsNone(TrackLookup.tryLookup(_lastSongInfo.trackRef)))
                    {
                        _frameData.Clear();
                        _tootData.Clear();
                        _frameIndex = 0;
                        _tootIndex = 0;
                        _isTooting = false;

                        GlobalLeaderboardManager.SetGameSpeedSlider((_lastSongInfo.gameSpeed - 0.5f) / .05f);

                        GlobalVariables.gamescrollspeed = _lastSongInfo.scrollSpeed;
                        TootTallyLogger.LogInfo("ScrollSpeed Set: " + _lastSongInfo.scrollSpeed);

                        SetTrackToSpectatingTrackref(_lastSongInfo.trackRef);
                        if (_levelSelectControllerInstance.alltrackslist[_levelSelectControllerInstance.songindex].trackref == _lastSongInfo.trackRef)
                        {
                            _spectatingStarting = true;
                            ReplaySystemManager.SetSpectatingMode();
                            _levelSelectControllerInstance.clickPlay();
                            _levelSelectControllerInstance = null;
                        }
                        else
                        {
                            PopUpNotifManager.DisplayNotif($"Clear song organizer filters for auto start to work properly.");
                            TootTallyLogger.LogWarning("Clear song organizer filters for auto start to work properly.");
                        }
                    }
                    else
                    {

                        TootTallyLogger.LogInfo("Do not own the song " + _lastSongInfo.trackRef);
                        PopUpNotifManager.DisplayNotif($"Do not own the song #{_lastSongInfo.songID}");
                    }
                else
                    PopUpNotifManager.DisplayNotif($"No SongInfo from host.");

            }

            private static void ResumeSong()
            {
                _gameControllerInstance.pausecontroller.clickResume();
            }

            //Yoinked from DNSpy Token: 0x06000276 RID: 630 RVA: 0x000270A8 File Offset: 0x000252A8
            private static void PauseSong()
            {
                if (!_gameControllerInstance.quitting && !_gameControllerInstance.level_finished && _gameControllerInstance.pausecontroller.done_animating && !_gameControllerInstance.freeplay)
                {
                    _isTooting = false;
                    _gameControllerInstance.notebuttonpressed = false;
                    _gameControllerInstance.musictrack.Pause();
                    _gameControllerInstance.sfxrefs.backfromfreeplay.Play();
                    _gameControllerInstance.puppet_humanc.shaking = false;
                    _gameControllerInstance.puppet_humanc.stopParticleEffects();
                    _gameControllerInstance.puppet_humanc.playCameraRotationTween(false);
                    _gameControllerInstance.paused = true;
                    _gameControllerInstance.quitting = true;
                    _gameControllerInstance.pausecanvas.SetActive(true);
                    _gameControllerInstance.pausecontroller.showPausePanel();
                    Cursor.visible = true;
                    if (!_gameControllerInstance.track_is_pausable)
                    {
                        _gameControllerInstance.curtainc.closeCurtain(false);
                    }
                }
            }

            private static void QuitSong()
            {
                _gameControllerInstance.pausecontroller.clickButton2();
                _gameControllerInstance = null;
            }

            private static void RestartSong()
            {
                _frameData.Clear();
                _tootData.Clear();
                _frameIndex = 0;
                _tootIndex = 0;
                _isTooting = false;
                _waitingToSync = IsSpectating;
                _gameControllerInstance.pauseRetryLevel();
            }

            private static void SetTrackToSpectatingTrackref(string trackref)
            {
                if (_levelSelectControllerInstance == null) return;
                for (int i = 0; i < _levelSelectControllerInstance.alltrackslist.Count; i++)
                {
                    if (_levelSelectControllerInstance.alltrackslist[i].trackref == trackref)
                    {
                        if (i - _levelSelectControllerInstance.songindex != 0)
                        {
                            _levelSelectControllerInstance.advanceSongs(i - _levelSelectControllerInstance.songindex, true);
                            return;
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.showPausePanel))]
            [HarmonyPrefix]
            public static void OnResumeSetUserStatus(PauseCanvasController __instance)
            {
                if (IsHosting)
                    SetCurrentUserState(UserState.Paused);

                if (Input.GetKeyDown(KeyCode.Escape) && IsSpectating)
                {
                    StopAllSpectator();
                    __instance.gc.quitting = true;
                    __instance.gc.pauseQuitLevel();
                    PopUpNotifManager.DisplayNotif("Stopped spectating.");
                }
            }

            [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.resumeFromPause))]
            [HarmonyPostfix]
            public static void OnPauseSetUserStatus()
            {
                if (IsHosting)
                    SetCurrentUserState(UserState.Playing);
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.pauseQuitLevel))]
            [HarmonyPostfix]
            public static void OnGameControllerUpdate()
            {
                if (IsHosting)
                    SetCurrentUserState(UserState.Quitting);
            }

            private static float _elapsedTime;
            private static readonly float _targetFramerate = Application.targetFrameRate > 60 || Application.targetFrameRate < 1 ? 60 : Application.targetFrameRate;

            [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
            [HarmonyPostfix]
            public static void OnUpdatePlaybackSpectatingData(GameController __instance)
            {
                if (!__instance.paused && !__instance.quitting && !__instance.retrying)
                {
                    if (IsSpectating && !_waitingToSync)
                        PlaybackSpectatingData(__instance);
                    else if (_waitingToSync && __instance.curtainc.doneanimating && !ShouldWaitForSync(out _waitingToSync))
                    {
                        PopUpNotifManager.DisplayNotif("Finished syncing with host.");
                        __instance.startSong(false);
                    }

                    _elapsedTime += Time.deltaTime;
                    if (IsHosting && _elapsedTime >= 1f / _targetFramerate)
                    {
                        _elapsedTime = 0f;
                        hostedSpectatingSystem.SendFrameData(__instance.musictrack.time, __instance.noteholderr.anchoredPosition.x, __instance.pointer.transform.localPosition.y, __instance.totalscore, __instance.highestcombo_level, __instance.highestcombocounter, __instance.currenthealth);
                    }
                }

            }

            private static bool _lastIsTooting;

            [HarmonyPatch(typeof(GameController), nameof(GameController.isNoteButtonPressed))]
            [HarmonyPostfix]
            public static void GameControllerIsNoteButtonPressedPostfixPatch(GameController __instance, ref bool __result)
            {
                if (IsHosting && _lastIsTooting != __result && !__instance.paused && !__instance.retrying && !__instance.quitting)
                    hostedSpectatingSystem.SendTootData(__instance.noteholderr.anchoredPosition.x, __instance.notebuttonpressed);
                _lastIsTooting = __result;
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.pauseRetryLevel))]
            [HarmonyPostfix]
            public static void OnRetryingSetUserStatus()
            {
                if (IsHosting)
                    SetCurrentUserState(UserState.Restarting);
            }

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPlay))]
            [HarmonyPostfix]
            public static void OnLevelSelectControllerClickPlaySendToSocket(LevelSelectController __instance)
            {
                if (!IsHosting && !IsSpectating && Plugin.Instance.AllowSpectate.Value)
                    CreateUniqueSpectatingConnection(Plugin.userInfo.id); //Remake Hosting connection just in case it wasnt reopened correctly

                if (IsHosting)
                    hostedSpectatingSystem.SendSongInfoToSocket(__instance.alltrackslist[__instance.songindex].trackref, 0, ReplaySystemManager.gameSpeedMultiplier, GlobalVariables.gamescrollspeed);
                _levelSelectControllerInstance = null;
            }

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPlay))]
            [HarmonyPrefix]
            public static bool OnLevelSelectControllerClickPlayOverwriteIfSpectating()
            {
                if (IsSpectating)
                {
                    if (_spectatingStarting) return true;

                    TryStartSong();
                    return false;
                }
                return true;
            }


            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickBack))]
            [HarmonyPostfix]
            public static void OnBackButtonClick()
            {
                _levelSelectControllerInstance = null;
            }

            private static UserState _currentHostState;
            private static UserState _lastHostState;

            private static void SetCurrentUserState(UserState userState)
            {
                _lastHostState = _currentHostState;
                _currentHostState = userState;
                hostedSpectatingSystem.SendUserStateToSocket(userState);
            }


            [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.doCoins))]
            [HarmonyPostfix]
            public static void ReplayIndicator(PointSceneController __instance)
            {
                if (!IsSpectating) return; // Replay not running, an actual play happened
                __instance.tootstext.text = "Spectating Done";
            }

            public static void SendCurrentUserState() => hostedSpectatingSystem?.SendUserStateToSocket(_currentHostState);
        }
        #endregion
    }
}