using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TootTally.Compatibility;
using TootTally.Graphics;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using UnityEngine.UI;
using UnityEngine.Video;

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
        private static Slider _replaySpeedSlider, _replayTimestampSlider;
        private static VideoPlayer _videoPlayer;
        private static Text _replayIndicatorMarquee;
        private static Vector3 _marqueeScroll = new Vector3(60, 0, 0);
        private static Vector3 _marqueeStartingPosition = new Vector3(500, -100, 100);
        private static GameController _currentGCInstance;
        private static EasingHelper.SecondOrderDynamics _pausePointerAnimation;
        private static GameObject _pauseArrow;
        private static Vector2 _pauseArrowDestination;

        #region GameControllerPatches

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void GameControllerPostfixPatch(GameController __instance)
        {
            _currentGCInstance = __instance;
            if (_replayFileName == null)
                OnRecordingStart(__instance);
            else
            {
                OnReplayingStart();
                SetReplayUI(__instance);
            }

            __instance.notescoresamples = 0; //Temporary fix for a glitch
            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            _pausePointerAnimation = new EasingHelper.SecondOrderDynamics(2.5f, 1f, 0.85f);

        }

        //This is when the video player is created.
        [HarmonyPatch(typeof(BGController), nameof(BGController.setUpBGControllerRefsDelayed))]
        [HarmonyPostfix]
        public static void OnSetUpBGControllerRefsDelayedPostFix(BGController __instance)
        {
            if (_replayManagerState == ReplayManagerState.Replaying)
            {
                if (__instance.bgobjects != null)
                {
                    try
                    {
                        GameObject canBG = GameObject.Find("can-bg-1").gameObject;
                        _videoPlayer = canBG.GetComponent<VideoPlayer>();
                    } catch (Exception e)
                    {
                        Plugin.LogInfo("Couldn't find VideoPlayer in background");
                    }
                    
                    if (_videoPlayer != null)
                    {
                        _replaySpeedSlider.onValueChanged.AddListener((float value) =>
                        {
                            _videoPlayer.playbackSpeed = value;
                        });
                        /*_replayTimestampSlider.onValueChanged.AddListener((float value) =>
                        {
                            _videoPlayer.time = _videoPlayer.length * value;
                        });*/
                    }
                        
                }
            }
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
                    Time.timeScale = 1f;
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
        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        [HarmonyPostfix]
        public static void GameControllerUpdatePostfixPatch(GameController __instance)
        {
            switch (_replayManagerState)
            {
                case ReplayManagerState.Replaying:
                    _replayTimestampSlider.value = __instance.musictrack.time / __instance.musictrack.clip.length;
                    __instance.currentnotesound.pitch = Mathf.Clamp(__instance.currentnotesound.pitch * __instance.musictrack.pitch, 0.5f * __instance.musictrack.pitch, 2f * __instance.musictrack.pitch);
                    if (_replayIndicatorMarquee.text.Equals(""))
                    {
                        _replayIndicatorMarquee.text = $"Watching {_replay.GetUsername} play {_replay.GetSongName}";
                    }
                    _replayIndicatorMarquee.transform.localPosition -= _marqueeScroll * Time.deltaTime;
                    if (_replayIndicatorMarquee.transform.localPosition.x <= -1000)
                    {
                        _replayIndicatorMarquee.transform.localPosition = _marqueeStartingPosition;
                    }
                    break;
                case ReplayManagerState.Paused:
                    if (_pauseArrowDestination != null)
                    _pauseArrow.GetComponent<RectTransform>().anchoredPosition = _pausePointerAnimation.GetNewPosition(_pauseArrowDestination, Time.deltaTime);
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
            }

        }

        [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.showPausePanel))]
        [HarmonyPostfix]
        static void PauseCanvasControllerShowPausePanelPostfixPatch(PauseCanvasController __instance)
        {
            switch (_replayManagerState)
            {
                case ReplayManagerState.Recording:
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.OnReplayStopUUID(SongDataHelper.GetChoosenSongHash(), _replayUUID));
                    Plugin.LogInfo($"UUID deleted: {_replayUUID}");
                    _replayUUID = null;
                    if (_replayFileName == null)
                        OnPauseAddReplayButton(__instance);
                    break;
                case ReplayManagerState.Replaying:
                    _replaySpeedSlider.onValueChanged.RemoveAllListeners();
                    Time.timeScale = 1f;
                    OnPauseChangeButtonText(__instance);
                    break;

            }
            _pauseArrow = __instance.pausearrow;
            _hasPaused = true;
            _replayManagerState = ReplayManagerState.Paused;
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.pauseQuitLevel))]
        [HarmonyPostfix]
        static void GameControllerPauseQuitLevelPostfixPatch(GameController __instance)
        {
            _replay.ClearData();
            _replayManagerState = ReplayManagerState.None;
            _replayFileName = null;
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.pauseRetryLevel))]
        [HarmonyPostfix]
        static void GameControllerPauseRetryLevelPostfixPatch(GameController __instance)
        {
            if (_replayFileName == null)
                _replay.ClearData();
        }


        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void OnLevelselectControllerStartInstantiateReplay(LevelSelectController __instance)
        {
            if (_replay == null)
            {
                _replayManagerState = ReplayManagerState.None;
                _replay = new NewReplaySystem();
            }

            if (Plugin.userInfo != null && !_hasGreetedUser)
            {
                _hasGreetedUser = true;
                if (Plugin.userInfo.username != "Guest")
                    PopUpNotifManager.DisplayNotif($"Welcome, {Plugin.userInfo.username}!", GameTheme.themeColors.notification.defaultText, 9f);
                else
                    PopUpNotifManager.DisplayNotif($"Login on TootTally\n<size=16>Put the APIKey in your config file\nto be able to submit scores</size>", GameTheme.themeColors.notification.warningText, 9f);
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
                    PopUpNotifManager.DisplayNotif("Downloading replay...", GameTheme.themeColors.notification.defaultText);
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


        public static void SetReplayUUID()
        {
            string trackRef = GlobalVariables.chosen_track;
            bool isCustom = Globals.IsCustomTrack(trackRef);
            string songFilePath = SongDataHelper.GetSongFilePath(trackRef);
            string songHash = isCustom ? SongDataHelper.CalcFileHash(songFilePath) : trackRef;

            StartAPICallCoroutine(songHash, songFilePath, isCustom);
        }

        public static void StartAPICallCoroutine(string songHash, string songFilePath, bool isCustom)
        {
            Plugin.LogInfo($"Requesting UUID for {songHash}");
            Plugin.Instance.StartCoroutine(TootTallyAPIService.GetHashInDB(songHash, isCustom, (songHashInDB) =>
            {
                if (Plugin.Instance.AllowTMBUploads.Value && songHashInDB == 0)
                {
                    string tmb = isCustom ? File.ReadAllText(songFilePath, Encoding.UTF8) : SongDataHelper.GenerateBaseTmb(songFilePath);
                    SerializableClass.TMBFile chart = new SerializableClass.TMBFile { tmb = tmb };
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.AddChartInDB(chart, () =>
                    {
                        Plugin.Instance.StartCoroutine(TootTallyAPIService.GetReplayUUID(SongDataHelper.GetChoosenSongHash(), (UUID) => _replayUUID = UUID));
                    }));
                }
                else if (songHashInDB != 0)
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetReplayUUID(SongDataHelper.GetChoosenSongHash(), (UUID) => _replayUUID = UUID));


            }));
        }

        public static void OnRecordingStart(GameController __instance)
        {
            SetReplayUUID();
            wasPlayingReplay = _hasPaused = _hasReleaseToot = false;
            _elapsedTime = 0;
            _targetFramerate = Application.targetFrameRate > 120 || Application.targetFrameRate < 1 ? 120 : Application.targetFrameRate; //Could let the user choose replay framerate... but risky for when they will upload to our server
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
            if (_replayUUID == null) return;//Dont save or upload if no UUID

            SaveReplayToFile();
            if (Plugin.userInfo.username != "Guest") //Don't upload if logged in as a Guest
                SendReplayFileToServer();
        }

        private static void SaveReplayToFile()
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");

            // Create Replays directory in case it doesn't exist
            if (!Directory.Exists(replayDir)) Directory.CreateDirectory(replayDir);


            FileHelper.WriteJsonToFile(replayDir, _replayUUID + ".ttr", _replay.GetRecordedReplayJson(_replayUUID, _targetFramerate));
        }

        private static void SetReplayUI(GameController __instance)
        {
            GameObject GameplayCanvas = GameObject.Find("GameplayCanvas").gameObject;
            GameObject UIHolder = GameplayCanvas.transform.Find("UIHolder").gameObject;
            SetReplaySpeedSlider(UIHolder.transform, __instance);
            SetReplayTimestampSlider(UIHolder.transform, __instance);
            SetReplayMarquees(UIHolder.transform);
        }


        private static void SetReplaySpeedSlider(Transform canvasTransform, GameController __instance)
        {
            _replaySpeedSlider = GameObjectFactory.CreateSliderFromPrefab(canvasTransform, "SpeedSlider");
            _replaySpeedSlider.gameObject.AddComponent<GraphicRaycaster>();
            _replaySpeedSlider.transform.SetSiblingIndex(0);
            GameObject sliderHandle = _replaySpeedSlider.transform.Find("Handle Slide Area/Handle").gameObject;
            sliderHandle.GetComponent<Image>().color = GameTheme.themeColors.scrollSpeedSlider.handle;

            //Text above the slider
            Text floatingSpeedText = GameObjectFactory.CreateSingleText(_replaySpeedSlider.transform, "SpeedSliderFloatingText", "SPEED", new Color(1, 1, 1, 1));
            floatingSpeedText.fontSize = 14;
            floatingSpeedText.alignment = TextAnchor.MiddleCenter;
            floatingSpeedText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 22);
            floatingSpeedText.GetComponent<RectTransform>().sizeDelta = _replaySpeedSlider.GetComponent<RectTransform>().sizeDelta;
            floatingSpeedText.GetComponent<Outline>().effectDistance = Vector2.one / 3f;
            floatingSpeedText.gameObject.SetActive(true);

            //Text inside the slider
            Text replaySpeedSliderText = GameObjectFactory.CreateSingleText(sliderHandle.transform, "replaySliderText", "100", Color.black);
            replaySpeedSliderText.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            replaySpeedSliderText.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 21);
            GameObject.Destroy(replaySpeedSliderText.GetComponent<Outline>());
            replaySpeedSliderText.alignment = TextAnchor.MiddleCenter;
            replaySpeedSliderText.horizontalOverflow = HorizontalWrapMode.Overflow;
            replaySpeedSliderText.verticalOverflow = VerticalWrapMode.Overflow;
            replaySpeedSliderText.fontSize = 8;
            replaySpeedSliderText.text = BetterScrollSpeedSliderPatcher.SliderValueToText(_replaySpeedSlider.value);
            replaySpeedSliderText.color = GameTheme.themeColors.scrollSpeedSlider.text;
            replaySpeedSliderText.gameObject.SetActive(true);
            _replaySpeedSlider.fillRect.gameObject.GetComponent<Image>().color = GameTheme.themeColors.scrollSpeedSlider.fill;
            _replaySpeedSlider.transform.Find("Background").GetComponent<Image>().color = GameTheme.themeColors.scrollSpeedSlider.background;

            _replaySpeedSlider.onValueChanged.AddListener((float value) =>
            {
                __instance.musictrack.pitch = _replaySpeedSlider.value;
                Time.timeScale = _replaySpeedSlider.value;
                replaySpeedSliderText.text = BetterScrollSpeedSliderPatcher.SliderValueToText(_replaySpeedSlider.value);
            });

            _replaySpeedSlider.gameObject.SetActive(true);
            _replaySpeedSlider.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, 190);
        }

        //This is absolutely the worst and most scuffed thing in the world, the game hate it when you change the musictrack time
        private static void SetReplayTimestampSlider(Transform canvasTransform, GameController __instance)
        {
            _replayTimestampSlider = GameObjectFactory.CreateSliderFromPrefab(canvasTransform, "TimestampSlider");
            _replayTimestampSlider.gameObject.AddComponent<GraphicRaycaster>();
            _replayTimestampSlider.transform.SetSiblingIndex(0);
            _replayTimestampSlider.value = 0f;
            _replayTimestampSlider.maxValue = 1f;
            _replayTimestampSlider.minValue = 0f;
            RectTransform rectTransform = _replayTimestampSlider.gameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(800, 20);
            rectTransform.anchoredPosition = new Vector2(-0, -195);

            /*_replayTimestampSlider.onValueChanged.AddListener((float value) =>
            {
                __instance.musictrack.time = __instance.musictrack.clip.length * value;
            });*/
           //_replayTimestampSlider.gameObject.SetActive(true); //Hidding until we figure out 
        }

        private static void SetReplayMarquees(Transform canvasTransform)
        {
            _replayIndicatorMarquee = GameObjectFactory.CreateSingleText(canvasTransform, "ReplayMarquee", "", new Color(1f, 1f, 1f, 0.75f));
            Outline textOutline = _replayIndicatorMarquee.GetComponent<Outline>();
            textOutline.effectDistance = Vector2.one / 2;
            _replayIndicatorMarquee.fontSize = 14;
            _replayIndicatorMarquee.transform.localPosition = _marqueeStartingPosition;
            _replayIndicatorMarquee.gameObject.SetActive(true);
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

        public static void OnPauseAddReplayButton(PauseCanvasController __instance)
        {
            __instance.panelrect.sizeDelta = new Vector2(290, 198);
            GameObject exitbtn = __instance.panelobj.transform.Find("ButtonRetry").gameObject;
            GameObject replayBtn = GameObject.Instantiate(exitbtn, __instance.panelobj.transform);

            replayBtn.name = "ButtonReplay";
            replayBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, -121);
            replayBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 40);
            replayBtn.GetComponent<Button>().onClick.m_PersistentCalls.Clear();
            replayBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                _replayFileName = "TempReplay";
                _replay.SetUsernameAndSongName(Plugin.userInfo.username, GlobalVariables.chosen_track_data.trackname_long);
                _currentGCInstance.pauseRetryLevel();
            });
            GameObject replayText = GameObject.Instantiate(__instance.panelobj.transform.Find("REST").gameObject, replayBtn.transform);
            replayText.name = "ReplayText";
            replayText.GetComponent<Text>().text = "View Replay";
            replayText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            replayText.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            replayText.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 44);

            EventTrigger replayBtnEvent = replayBtn.AddComponent<EventTrigger>();
            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) => OnPauseMenuButtonOver(__instance, new object[] { 3 }));
            replayBtnEvent.triggers.Add(pointerEnterEvent);
            _pauseArrowDestination = new Vector2(28, -37);
        }

        [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.mouseOverPauseBtn))]
        [HarmonyPrefix]
        public static bool OnPauseMenuButtonOver(PauseCanvasController __instance, object[] __args)
        {
            _pausePointerAnimation.SetStartPosition(__instance.pausearrowr.anchoredPosition);
            _pauseArrowDestination = new Vector2(28, -44 * ((int)__args[0]-1) - 37);

            return false;
        }

        public static void OnPauseChangeButtonText(PauseCanvasController __instance)
        {

            __instance.panelobj.transform.Find("ButtonExit").gameObject.GetComponent<RectTransform>().sizeDelta += new Vector2(8, 0);
            __instance.panelobj.transform.Find("REST").gameObject.GetComponent<Text>().text = "Exit Replay";

            __instance.panelobj.transform.Find("ButtonRetry").gameObject.GetComponent<RectTransform>().sizeDelta += new Vector2(38, 0); ;
            __instance.panelobj.transform.Find("CONT").gameObject.GetComponent<Text>().text = "Restart Replay";
            _pauseArrowDestination = new Vector2(28, -37);
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
