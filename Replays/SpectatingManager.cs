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
            spectator.Disconnect();
            if (_spectatingSystemList.Contains(spectator))
                _spectatingSystemList.Remove(spectator);
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
        }

        public enum UserState
        {
            SelectingSong,
            Paused,
            Playing,
            Restarting,
            Quitting,
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
            private static List<SocketFrameData> _frameData;
            private static LevelSelectController _levelSelectControllerInstance;
            private static SocketFrameData _lastFrame;
            private static bool _isTooting;
            private static int _frameIndex;
            private static int _totalScore;

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
            [HarmonyPostfix]
            public static void SetLevelSelectUserStatusOnAdvanceSongs(LevelSelectController __instance)
            {
                _levelSelectControllerInstance = __instance;
                if (IsHosting)
                    hostedSpectatingSystem.SendUserStateToSocket(UserState.SelectingSong);
            }


            [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
            [HarmonyPostfix]
            public static void OnGameControllerStart()
            {
                if (IsSpectating)
                    _frameIndex = 0;
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.startSong))]
            [HarmonyPostfix]
            public static void SetPlayingUserStatus(GameController __instance)
            {
                if (IsSpectating)
                {
                    if (_frameData[_frameIndex].time - __instance.musictrack.time >= 4)
                    {
                        TootTallyLogger.LogInfo("Syncing track with replay data...");
                        __instance.musictrack.time = _frameData[_frameIndex].time;
                        __instance.noteholderr.anchoredPosition = new Vector2(_frameData[_frameIndex].noteHolder, __instance.noteholderr.anchoredPosition.y);
                    }
                }
                if (IsHosting)
                    hostedSpectatingSystem.SendUserStateToSocket(UserState.Playing);

            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.isNoteButtonPressed))]
            [HarmonyPostfix]
            public static void GameControllerIsNoteButtonPressedPostfixPatch(ref bool __result) // Take isNoteButtonPressed's return value and changed it to mine, hehe
            {
                if (IsSpectating)
                    __result = _isTooting;
            }

            public static void PlaybackSpectatingData(GameController __instance)
            {
                Cursor.visible = true;
                if (_frameData == null) return;
                if (!__instance.controllermode) __instance.controllermode = true; //Still required to not make the mouse position update

                var currentMapPosition = __instance.noteholderr.anchoredPosition.x;

                __instance.totalscore = _totalScore;
                if (_frameData.Count > _frameIndex && _lastFrame != null)
                    InterpolateCursorPosition(currentMapPosition, __instance);

                PlaybackFrameData(currentMapPosition, __instance);
                _isTooting = _frameData[_frameIndex].isTooting;
            }

            private static void InterpolateCursorPosition(float currentMapPosition, GameController __instance)
            {
                var newCursorPosition = EasingHelper.Lerp(_lastFrame.pointerPosition, _frameData[_frameIndex].pointerPosition, (_lastFrame.noteHolder - currentMapPosition) / (_lastFrame.noteHolder - _frameData[_frameIndex].noteHolder));
                SetCursorPosition(__instance, newCursorPosition);
                __instance.puppet_humanc.doPuppetControl(-newCursorPosition / 225); //225 is half of the Gameplay area:450
            }

            private static void PlaybackFrameData(float currentMapPosition, GameController __instance)
            {
                if (_frameData.Count > _frameIndex && currentMapPosition <= _frameData[_frameIndex].noteHolder)
                {
                    _lastFrame = _frameData[_frameIndex];
                }

                while (_frameData.Count > _frameIndex && currentMapPosition <= _frameData[_frameIndex].noteHolder) //smaller or equal to because noteholder goes toward negative
                {
                    SetCursorPosition(__instance, _frameData[_frameIndex].pointerPosition);
                    if (_frameIndex < _frameData.Count - 2)
                        _frameIndex++;
                    else if (!__instance.level_finished)
                        __instance.musictrack.time = __instance.levelendtime;
                    __instance.totalscore = _frameData[_frameIndex].totalScore;
                    __instance.currenthealth = _frameData[_frameIndex].health;
                    __instance.highestcombo_level = _frameData[_frameIndex].highestCombo;
                    __instance.highestcombocounter = _frameData[_frameIndex].currentCombo;
                }
            }

            public static void SetCursorPosition(GameController __instance, float newPosition)
            {
                Vector3 pointerPosition = __instance.pointer.transform.localPosition;
                pointerPosition.y = newPosition;
                __instance.pointer.transform.localPosition = pointerPosition;
            }

            public static void OnFrameDataReceived(int id, SocketFrameData frameData)
            {
                _frameData?.Add(frameData);
            }

            public static void OnSongInfoReceived(int id, SocketSongInfo info)
            {
                if (info == null || info.trackRef == null || info.gameSpeed <= 0f)
                {
                    TootTallyLogger.LogInfo("SongInfo went wrong.");
                    return;
                }
                _frameData = new List<SocketFrameData>();
                _frameIndex = 0;
                GlobalLeaderboardManager.SetGameSpeedSlider((info.gameSpeed - 0.5f) / .05f);
                TootTallyLogger.LogInfo("GameSpeed Set: " + info.gameSpeed);
                GlobalVariables.gamescrollspeed = info.scrollSpeed;
                TootTallyLogger.LogInfo("ScrollSpeed Set: " + info.scrollSpeed);
                if (_levelSelectControllerInstance != null)
                {
                    if (!FSharpOption<TromboneTrack>.get_IsNone(TrackLookup.tryLookup(info.trackRef)))
                    {
                        SetTrackToSpectatingTrackref(info.trackRef);
                        ReplaySystemManager.SetSpectatingMode();
                        _levelSelectControllerInstance.clickPlay();
                    }
                    else
                        TootTallyLogger.LogInfo("Do not own the song " + info.trackRef);
                }

            }

            public static void SetTrackToSpectatingTrackref(string trackref)
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
            [HarmonyPostfix]
            public static void OnResumeSetUserStatus()
            {
                if (IsHosting)
                    hostedSpectatingSystem.SendUserStateToSocket(UserState.Paused);
            }

            [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.resumeFromPause))]
            [HarmonyPostfix]
            public static void OnPauseSetUserStatus()
            {
                if (IsHosting)
                    hostedSpectatingSystem.SendUserStateToSocket(UserState.Playing);
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.pauseQuitLevel))]
            [HarmonyPostfix]
            public static void OnGameControllerUpdate()
            {
                if (IsHosting)
                    hostedSpectatingSystem.SendUserStateToSocket(UserState.Quitting);
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
            [HarmonyPostfix]
            public static void OnQuitSetUserStatus(GameController __instance)
            {
                if (IsSpectating)
                    PlaybackSpectatingData(__instance);
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.pauseRetryLevel))]
            [HarmonyPostfix]
            public static void OnRetryingSetUserStatus()
            {
                if (IsHosting)
                    hostedSpectatingSystem.SendUserStateToSocket(UserState.Restarting);
            }

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPlay))]
            [HarmonyPostfix]
            public static void OnLevelSelectControllerClickPlaySendToSocket(LevelSelectController __instance)
            {
                if (IsHosting)
                    hostedSpectatingSystem.SendSongInfoToSocket(__instance.alltrackslist[__instance.songindex].trackref, 0, ReplaySystemManager.gameSpeedMultiplier, GlobalVariables.gamescrollspeed);
                _levelSelectControllerInstance = null;
            }

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickBack))]
            [HarmonyPostfix]
            public static void OnBackButtonClick()
            {
                _levelSelectControllerInstance = null;
            }

        }
        #endregion
    }
}