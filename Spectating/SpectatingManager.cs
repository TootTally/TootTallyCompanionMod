using BaboonAPI.Hooks.Tracks;
using HarmonyLib;
using Microsoft.FSharp.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TootTally.CustomLeaderboard;
using TootTally.GameplayModifier;
using TootTally.Replays;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using UnityEngine;

namespace TootTally.Spectating
{
    public class SpectatingManager : MonoBehaviour
    {
        public static JsonConverter[] _dataConverter = new JsonConverter[] { new SocketDataConverter() };
        private static List<SpectatingSystem> _spectatingSystemList;
        public static SpectatingSystem hostedSpectatingSystem;
        public static int[] currentSpectatorIDList;
        public static bool IsHosting => hostedSpectatingSystem != null && hostedSpectatingSystem.IsConnected && hostedSpectatingSystem.IsHost;
        public static bool IsSpectating => _spectatingSystemList != null && !IsHosting && _spectatingSystemList.Any(x => x.IsConnected);

        public void Awake()
        {
            _spectatingSystemList ??= new List<SpectatingSystem>();
            if (Plugin.Instance.AllowSpectate.Value && Plugin.userInfo != null && Plugin.userInfo.id != 0)
                CreateUniqueSpectatingConnection(Plugin.userInfo.id, Plugin.userInfo.username);
            Plugin.Instance.StartCoroutine(TootTallyAPIService.GetSpectatorIDList(idList => currentSpectatorIDList = idList));
        }

        public void Update()
        {
            _spectatingSystemList?.ForEach(s => s.UpdateStacks());

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Escape) && _spectatingSystemList.Count > 0 && !IsHosting)
            {
                SpectatingOverlay.UpdateViewerList(null);
                SpectatingOverlay.HideStopSpectatingButton();
                SpectatingOverlay.HideViewerIcon();
                _spectatingSystemList.Last().RemoveFromManager();
            }

            if (IsAnyConnectionPending() && !SpectatingOverlay.IsLoadingIconVisible())
                SpectatingOverlay.ShowLoadingIcon();
            else if (!IsAnyConnectionPending() && SpectatingOverlay.IsLoadingIconVisible())
                SpectatingOverlay.HideLoadingIcon();
        }

        public static void StopAllSpectator()
        {
            if (_spectatingSystemList != null && _spectatingSystemList.Count > 0)
            {
                SpectatingOverlay.UpdateViewerList(null);
                SpectatingOverlay.SetCurrentUserState(UserState.None);
                for (int i = 0; i < _spectatingSystemList.Count;)
                    RemoveSpectator(_spectatingSystemList[i]);
                if (hostedSpectatingSystem != null && hostedSpectatingSystem.IsConnected)
                    hostedSpectatingSystem = null;
            }
        }

        public static SpectatingSystem CreateNewSpectatingConnection(int id, string name)
        {
            var spec = new SpectatingSystem(id, name);
            spec.OnWebSocketOpenCallback = OnSpectatingConnect;
            _spectatingSystemList.Add(spec);
            if (id == Plugin.userInfo.id)
                hostedSpectatingSystem = spec;
            return spec;
        }

        public static void OnSpectatingConnect(SpectatingSystem sender)
        {
            if (!sender.IsHost)
            {
                sender.OnSocketSongInfoReceived = SpectatingManagerPatches.OnSongInfoReceived;
                sender.OnSocketUserStateReceived = SpectatingManagerPatches.OnUserStateReceived;
                sender.OnSocketFrameDataReceived = SpectatingManagerPatches.OnFrameDataReceived;
                sender.OnSocketTootDataReceived = SpectatingManagerPatches.OnTootDataReceived;
                sender.OnSocketNoteDataReceived = SpectatingManagerPatches.OnNoteDataReceived;
                PopUpNotifManager.DisplayNotif($"Waiting for host to pick a song...");
            }
            else
            {
                OnHostConnection();
                SpectatingManagerPatches.SendCurrentUserState();
            }
            sender.OnSocketSpecInfoReceived = SpectatingManagerPatches.OnSpectatorDataReceived;
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

        public static SpectatingSystem CreateUniqueSpectatingConnection(int id, string name)
        {
            StopAllSpectator();
            return CreateNewSpectatingConnection(id, name);
        }

        public static void OnAllowHostConfigChange(bool value)
        {
            if (value && hostedSpectatingSystem == null && Plugin.userInfo.id != 0)
                CreateUniqueSpectatingConnection(Plugin.userInfo.id, Plugin.userInfo.username);
            else if (!value && hostedSpectatingSystem != null)
            {
                RemoveSpectator(hostedSpectatingSystem);
                hostedSpectatingSystem = null;
            }
        }

        public static void UpdateSpectatorIDList()
        {
            Plugin.Instance.StartCoroutine(TootTallyAPIService.GetSpectatorIDList(idList => currentSpectatorIDList = idList));
        }

        public static void OnHostConnection()
        {
        }

        public static bool IsAnyConnectionPending() => _spectatingSystemList.Any(x => x.ConnectionPending);

        public enum DataType
        {
            UserState,
            SongInfo,
            FrameData,
            TootData,
            NoteData,
            SpectatorInfo,
        }

        public enum UserState
        {
            None,
            SelectingSong,
            Paused,
            Playing,
            Restarting,
            Quitting,
            PointScene,
            GettingReady,
        }
        public enum SceneType
        {
            None,
            LevelSelect,
            GameController,
            HomeController
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
            public double time { get; set; }
            public double noteHolder { get; set; }
            public float pointerPosition { get; set; }
        }

        public class SocketTootData : SocketMessage
        {
            public double time { get; set; }
            public double noteHolder { get; set; }
            public bool isTooting { get; set; }
        }

        public class SocketSongInfo : SocketMessage
        {
            public string trackRef { get; set; }
            public int songID { get; set; }
            public float gameSpeed { get; set; }
            public float scrollSpeed { get; set; }
            public string gamemodifiers { get; set; }
        }

        public class SocketNoteData : SocketMessage
        {
            public int noteID { get; set; }
            public double noteScoreAverage { get; set; }
            public bool champMode { get; set; }
            public int multiplier { get; set; }
            public int totalScore { get; set; }
            public bool releasedButtonBetweenNotes { get; set; }
            public float health { get; set; }
            public int highestCombo { get; set; }
        }

        public class SocketSpectatorInfo : SocketMessage
        {
            public string hostName { get; set; }
            public int count { get; set; }
            public List<string> spectators { get; set; }
        }

        public class SocketDataConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(SocketMessage);

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

                if (jo["dataType"].Value<string>() == DataType.NoteData.ToString())
                    return jo.ToObject<SocketNoteData>(serializer);

                if (jo["dataType"].Value<string>() == DataType.SpectatorInfo.ToString())
                    return jo.ToObject<SocketSpectatorInfo>(serializer);

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
        public static class SpectatingManagerPatches
        {
            private static LevelSelectController _levelSelectControllerInstance;
            private static GameController _gameControllerInstance;
            private static PointSceneController _pointSceneControllerInstance;

            private static UserState _lastSpecState = UserState.None, _currentSpecState = UserState.None;

            private static List<SocketFrameData> _frameData = new List<SocketFrameData>();
            private static List<SocketTootData> _tootData = new List<SocketTootData>();
            private static List<SocketNoteData> _noteData = new List<SocketNoteData>();

            private static SocketFrameData _lastFrame, _currentFrame;
            private static SocketTootData _currentTootData;
            private static SocketNoteData _currentNoteData;

            private static SocketSongInfo _lastSongInfo;
            private static SocketSongInfo _currentSongInfo;

            private static TromboneTrack _lastTrackData;

            private static bool _isTooting;
            private static int _frameIndex;
            private static int _tootIndex;
            private static bool _waitingToSync;
            private static bool _spectatingStarting;
            private static bool _wasSpectating;

            [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
            [HarmonyPostfix]

            public static void InitOverlay() { SpectatingOverlay.Initialize(); }

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
            [HarmonyPostfix]
            public static void SetLevelSelectUserStatusOnAdvanceSongs(LevelSelectController __instance)
            {
                _gameControllerInstance = null;
                _pointSceneControllerInstance = null;
                _levelSelectControllerInstance = __instance;
                _spectatingStarting = false;
                _lastSongInfo = null;
                _wasSpectating = false;
                if (IsHosting)
                    SetCurrentUserState(UserState.SelectingSong);
                else if (IsSpectating)
                    SpectatingOverlay.SetCurrentUserState(UserState.SelectingSong);
                else if (Plugin.Instance.AllowSpectate.Value && Plugin.userInfo.id != 0)
                {
                    CreateUniqueSpectatingConnection(Plugin.userInfo.id, Plugin.userInfo.username); //Remake Hosting connection just in case it wasnt reopened correctly
                    SpectatingOverlay.HideViewerIcon();
                }
                SpectatingOverlay.HidePauseText();
                SpectatingOverlay.HideMarquee();
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
            [HarmonyPostfix]
            public static void OnGameControllerStart(GameController __instance)
            {
                if (IsHosting && _currentHostState != UserState.GettingReady)
                {
                    if (_lastHostSongInfo != null)
                        hostedSpectatingSystem.SendSongInfoToSocket(_lastHostSongInfo);
                    SetCurrentUserState(UserState.GettingReady);
                }
                _pointSceneControllerInstance = null;
                _levelSelectControllerInstance = null;
                _gameControllerInstance = __instance;
                _waitingToSync = _wasSpectating = IsSpectating;
                if (IsSpectating)
                {
                    _frameIndex = 0;
                    _tootIndex = 0;
                    _lastFrame = null;
                    _currentFrame = new SocketFrameData() { time = 0, noteHolder = 0, pointerPosition = 0 };
                    _currentTootData = new SocketTootData() { time = 0, isTooting = false, noteHolder = 0 };
                    _isTooting = false;
                    _elapsedTime = 0;
                    if (_lastTrackData != null)
                        SpectatingOverlay.ShowMarquee(_spectatingSystemList.Last().spectatorName, _lastTrackData.trackname_short, _lastSongInfo.gameSpeed, _lastSongInfo.gamemodifiers);
                }
            }

            private const float SYNC_BUFFER = 1f;

            [HarmonyPatch(typeof(GameController), nameof(GameController.playsong))]
            [HarmonyPrefix]
            public static bool OverwriteStartSongIfSyncRequired(GameController __instance)
            {
                if (!IsSpectating || IsHosting) return true;

                if (ShouldWaitForSync(out _waitingToSync))
                    PopUpNotifManager.DisplayNotif("Waiting to sync with host...");

                return !_waitingToSync;
            }

            private static bool ShouldWaitForSync(out bool waitForSync)
            {
                waitForSync = true;

                if (_frameData != null && _frameData.Count > 0 && _frameData.Last().time >= SYNC_BUFFER && _currentSpecState != UserState.GettingReady && _currentSpecState != UserState.Restarting)
                {
                    SpectatingOverlay.SetCurrentUserState(UserState.Playing);
                    waitForSync = false;
                }
                return waitForSync;
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.playsong))]
            [HarmonyPostfix]
            public static void SetPlayingUserStatus(GameController __instance)
            {
                if (IsHosting)
                    SetCurrentUserState(UserState.Playing);

                if (IsSpectating)
                {
                    if (_frameData != null && _frameData.Count > 0 && _frameData[_frameIndex].time - __instance.musictrack.time >= SYNC_BUFFER)
                    {
                        TootTallyLogger.LogInfo("Syncing track with replay data...");
                        __instance.musictrack.time = (float)_frameData[_frameIndex].time;
                        __instance.noteholderr.anchoredPosition = new Vector2((float)_frameData[_frameIndex].noteHolder, __instance.noteholderr.anchoredPosition.y);
                    }
                }
            }

            private static bool _lastIsTooting;

            [HarmonyPatch(typeof(GameController), nameof(GameController.isNoteButtonPressed))]
            [HarmonyPostfix]
            public static void GameControllerIsNoteButtonPressedPostfixPatch(GameController __instance, ref bool __result)
            {
                if (IsSpectating)
                    __result = _isTooting;
                else if (IsHosting && _lastIsTooting != __result && !__instance.paused && !__instance.retrying && !__instance.quitting)
                    hostedSpectatingSystem.SendTootData(__instance.musictrack.time, __instance.noteholderr.anchoredPosition.x, __result);
                _lastIsTooting = __result;
            }

            [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
            [HarmonyPostfix]
            public static void OnPointSceneControllerStartSetInstance(PointSceneController __instance)
            {
                _levelSelectControllerInstance = null;
                _gameControllerInstance = null;
                _pointSceneControllerInstance = __instance;
                if (IsHosting)
                    SetCurrentUserState(UserState.PointScene);
                SpectatingOverlay.HideMarquee();
            }

            [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Update))]
            [HarmonyPostfix]
            public static void GetOutOfPointSceneIfHostLeft(PointSceneController __instance)
            {
                if (IsSpectating && _lastSpecState == UserState.PointScene && !__instance.clickedleave)
                    if (_currentSpecState == UserState.SelectingSong)
                        BackToLevelSelect();
                    else if (_currentSpecState == UserState.Playing)
                        RetryFromPointScene();
            }

            public static void PlaybackSpectatingData(GameController __instance)
            {
                Cursor.visible = true;
                if (_frameData == null || _tootData == null) return;

                if (!__instance.controllermode) __instance.controllermode = true; //Still required to not make the mouse position update

                var currentMapPosition = __instance.noteholderr.anchoredPosition.x;



                if (_frameData.Count > 0)
                    PlaybackFrameData(currentMapPosition, __instance);

                if (_tootData.Count > 0)
                    PlaybackTootData(currentMapPosition);

                if (_frameData.Count > _frameIndex && _lastFrame != null && _currentFrame != null)
                    InterpolateCursorPosition(currentMapPosition, __instance);


            }

            private static void InterpolateCursorPosition(float currentMapPosition, GameController __instance)
            {
                var newCursorPosition = EasingHelper.Lerp(_lastFrame.pointerPosition, _currentFrame.pointerPosition, (float)((_lastFrame.noteHolder - currentMapPosition) / (_lastFrame.noteHolder - _currentFrame.noteHolder)));
                SetCursorPosition(__instance, newCursorPosition);
                __instance.puppet_humanc.doPuppetControl(-newCursorPosition / 225); //225 is half of the Gameplay area:450
            }

            private static void PlaybackFrameData(float currentMapPosition, GameController __instance)
            {
                if (_lastFrame != _currentFrame && currentMapPosition <= _currentFrame.noteHolder)
                    _lastFrame = _currentFrame;

                if (_frameData.Count > _frameIndex && (_currentFrame == null || currentMapPosition <= _currentFrame.noteHolder))
                {
                    _frameIndex = _frameData.FindIndex(_frameIndex > 1 ? _frameIndex - 1 : 0, x => currentMapPosition > x.noteHolder);
                    if (_frameData.Count > _frameIndex && _frameIndex != -1)
                        _currentFrame = _frameData[_frameIndex];
                }
            }

            private static void SetCursorPosition(GameController __instance, float newPosition)
            {
                Vector3 pointerPosition = __instance.pointer.transform.localPosition;
                pointerPosition.y = newPosition;
                __instance.pointer.transform.localPosition = pointerPosition;
            }

            public static void OnFrameDataReceived(SocketFrameData frameData)
            {
                _frameData?.Add(frameData);
            }

            public static void OnTootDataReceived(SocketTootData tootData)
            {
                _tootData?.Add(tootData);
            }

            public static void OnNoteDataReceived(SocketNoteData noteData)
            {
                _noteData?.Add(noteData);
            }

            public static void OnSpectatorDataReceived(SocketSpectatorInfo specData)
            {
                SpectatingOverlay.UpdateViewerList(specData);
            }

            public static void PlaybackTootData(float currentMapPosition)
            {
                if (currentMapPosition <= _currentTootData.noteHolder && _isTooting != _currentTootData.isTooting)
                    _isTooting = _currentTootData.isTooting;

                if (_tootData.Count > _tootIndex && currentMapPosition <= _currentTootData.noteHolder) //smaller or equal to because noteholder goes toward negative
                    _currentTootData = _tootData[_tootIndex++];
            }

            public static void OnSongInfoReceived(SocketSongInfo info)
            {
                if (info == null || info.trackRef == null || info.gameSpeed <= 0f)
                {
                    TootTallyLogger.LogInfo("SongInfo went wrong.");
                    return;
                }
                _lastSongInfo = info;
            }

            public static void OnUserStateReceived(SocketUserState userState)
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
                        if (_gameControllerInstance != null && _waitingToSync)
                            ClearSpectatingData();
                        if (_gameControllerInstance != null)
                            QuitSong();
                        break;

                    case UserState.Restarting:
                        if (_gameControllerInstance != null && _waitingToSync)
                            ClearSpectatingData();
                        if (_gameControllerInstance != null)
                            RestartSong();
                        break;

                    case UserState.GettingReady:
                        if (_levelSelectControllerInstance != null)
                            TryStartSong();
                        else if (_gameControllerInstance != null)
                            _waitingToSync = true;
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
                ClearSpectatingData();
                ReplaySystemManager.gameSpeedMultiplier = _lastSongInfo.gameSpeed;
                GlobalVariables.gamescrollspeed = _lastSongInfo.scrollSpeed;
                TootTallyLogger.LogInfo("ScrollSpeed Set: " + _lastSongInfo.scrollSpeed);
                _pointSceneControllerInstance.clickRetry();
            }

            private static void ClearSpectatingData()
            {
                _frameData.Clear();
                _tootData.Clear();
                _noteData.Clear();
                _frameIndex = 0;
                _tootIndex = 0;
                _isTooting = false;
            }

            private static void TryStartSong()
            {
                if (_currentSpecState != UserState.SelectingSong)
                    if (_lastSongInfo != null && _lastSongInfo.trackRef != null)
                        if (!FSharpOption<TromboneTrack>.get_IsNone(TrackLookup.tryLookup(_lastSongInfo.trackRef)))
                        {
                            _lastTrackData = TrackLookup.lookup(_lastSongInfo.trackRef);
                            SetTrackToSpectatingTrackref(_lastSongInfo.trackRef);
                            if (_levelSelectControllerInstance.alltrackslist[_levelSelectControllerInstance.songindex].trackref == _lastSongInfo.trackRef)
                            {
                                _currentSongInfo = _lastSongInfo;
                                _spectatingStarting = true;
                                ClearSpectatingData();
                                ReplaySystemManager.SetSpectatingMode();
                                GlobalLeaderboardManager.SetGameSpeedSlider((_lastSongInfo.gameSpeed - 0.5f) / .05f);
                                GlobalVariables.gamescrollspeed = _lastSongInfo.scrollSpeed;
                                TootTallyLogger.LogInfo("ScrollSpeed Set: " + _lastSongInfo.scrollSpeed);
                                GameModifierManager.LoadModifiersFromString(_lastSongInfo.gamemodifiers);
                                _levelSelectControllerInstance.clickPlay();
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
                            PopUpNotifManager.DisplayNotif($"Do not own the song #{_lastSongInfo.trackRef}");
                        }
                    else
                        PopUpNotifManager.DisplayNotif($"No SongInfo from host.");
                else
                    PopUpNotifManager.DisplayNotif($"Waiting for host to start a song.");

            }

            private static void ResumeSong()
            {
                _gameControllerInstance.pausecontroller.clickResume();
                SpectatingOverlay.HidePauseText();
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

            public static void QuitSong()
            {
                _gameControllerInstance.paused = true;
                _gameControllerInstance.quitting = true;
                ClearSpectatingData();
                _gameControllerInstance.pauseQuitLevel();
                SpectatingOverlay.HidePauseText();
                SpectatingOverlay.HideMarquee();
            }

            private static void RestartSong()
            {
                ClearSpectatingData();
                _waitingToSync = IsSpectating;
                _gameControllerInstance.pauseRetryLevel();
                SpectatingOverlay.HidePauseText();
                SpectatingOverlay.HideMarquee();
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

                    __instance.gc.quitting = true;
                    __instance.gc.pauseQuitLevel();
                    StopAllSpectator();
                    PopUpNotifManager.DisplayNotif("Stopped spectating.");
                }
                else if (IsSpectating)
                {
                    __instance.panelobj.SetActive(false);
                    SpectatingOverlay.ShowPauseText();
                }
            }

            [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.clickResume))]
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
                else
                    SpectatingOverlay.SetCurrentUserState(UserState.Quitting);
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
            [HarmonyPrefix]
            public static void OnGetScoreAveragePrefixSetCurrentNote(GameController __instance)
            {
                if (IsHosting)
                {
                    hostedSpectatingSystem.SendNoteData(__instance.rainbowcontroller.champmode, __instance.multiplier, __instance.currentnoteindex,
                        __instance.notescoreaverage, __instance.released_button_between_notes, __instance.totalscore, __instance.currenthealth, __instance.highestcombo_level);
                }
                else if (IsSpectating)
                {
                    if (_noteData != null && _noteData.Count > 0 && _noteData.Last().noteID > __instance.currentnoteindex)
                        _currentNoteData = _noteData.Find(x => x.noteID == __instance.currentnoteindex);
                    if (_currentNoteData != null)
                    {
                        __instance.rainbowcontroller.champmode = _currentNoteData.champMode;
                        __instance.multiplier = _currentNoteData.multiplier;
                        __instance.notescoreaverage = (float)_currentNoteData.noteScoreAverage;
                        __instance.released_button_between_notes = _currentNoteData.releasedButtonBetweenNotes;
                        if (__instance.currentscore < 0)
                            __instance.currentscore = _currentNoteData.totalScore;
                        __instance.totalscore = _currentNoteData.totalScore;
                        __instance.currenthealth = _currentNoteData.health;
                        __instance.highestcombo_level = _currentNoteData.highestCombo;
                        _currentNoteData = null;
                    }
                }
            }

            private static float _elapsedTime;
            private static readonly float _targetFramerate = Application.targetFrameRate > 60 || Application.targetFrameRate < 1 ? 60 : Application.targetFrameRate;

            [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
            [HarmonyPostfix]
            public static void OnUpdatePlaybackSpectatingData(GameController __instance)
            {
                if (IsSpectating)
                {
                    if (!__instance.quitting && !__instance.retrying && (_currentSpecState == UserState.SelectingSong || _lastSongInfo == null || _currentSongInfo == null || _lastSongInfo.trackRef != _currentSongInfo.trackRef))
                        QuitSong();
                    if (!_waitingToSync && !__instance.paused && !__instance.quitting && !__instance.retrying)
                        PlaybackSpectatingData(__instance);
                    else if (_waitingToSync && __instance.curtainc.doneanimating && !ShouldWaitForSync(out _waitingToSync))
                    {
                        PopUpNotifManager.DisplayNotif("Finished syncing with host.");
                        __instance.startSong(false);
                    }
                }
                else if (IsHosting && !__instance.paused && !__instance.quitting && !__instance.retrying)
                {
                    _elapsedTime += Time.deltaTime;
                    if (_elapsedTime >= 1f / _targetFramerate)
                    {
                        _elapsedTime = 0f;
                        hostedSpectatingSystem.SendFrameData(__instance.musictrack.time + (__instance.latency_offset / 1000f), __instance.noteholderr.anchoredPosition.x, __instance.pointer.transform.localPosition.y);
                    }
                }

            }


            [HarmonyPatch(typeof(GameController), nameof(GameController.pauseRetryLevel))]
            [HarmonyPostfix]
            public static void OnRetryingSetUserStatus()
            {
                if (IsHosting)
                    SetCurrentUserState(UserState.Restarting);
            }

            private static SocketSongInfo _lastHostSongInfo;

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPlay))]
            [HarmonyPostfix]
            public static void OnLevelSelectControllerClickPlaySendToSocket(LevelSelectController __instance)
            {
                _lastHostSongInfo = new SocketSongInfo
                {
                    trackRef = __instance.alltrackslist[__instance.songindex].trackref,
                    songID = 0,
                    gameSpeed = ReplaySystemManager.gameSpeedMultiplier,
                    scrollSpeed = GlobalVariables.gamescrollspeed,
                    gamemodifiers = GameModifierManager.GetModifiersString()
                };

                if (!IsHosting && !IsSpectating && Plugin.Instance.AllowSpectate.Value && Plugin.userInfo.id != 0)
                    CreateUniqueSpectatingConnection(Plugin.userInfo.id, Plugin.userInfo.username); //Remake Hosting connection just in case it wasnt reopened correctly

                if (IsHosting)
                {
                    hostedSpectatingSystem.SendSongInfoToSocket(_lastHostSongInfo);
                    hostedSpectatingSystem.SendUserStateToSocket(UserState.GettingReady);
                }

                SpectatingOverlay.HideViewerIcon();
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
                SpectatingOverlay.HideStopSpectatingButton();
                SpectatingOverlay.HideViewerIcon();
                _levelSelectControllerInstance = null;
                if (IsHosting)
                    SetCurrentUserState(UserState.None);
            }

            private static UserState _currentHostState;
            private static UserState _lastHostState;

            private static void SetCurrentUserState(UserState userState)
            {
                _lastHostState = _currentHostState;
                _currentHostState = userState;
                hostedSpectatingSystem.SendUserStateToSocket(userState);
                TootTallyLogger.LogInfo($"Current state changed from {_lastHostState} to {_currentHostState}");
                SpectatingOverlay.SetCurrentUserState(userState);
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