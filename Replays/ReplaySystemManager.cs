using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using HarmonyLib;
using TMPro;
using TootTally.Compatibility;
using TootTally.Graphics;
using TootTally.Utils;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using TrombLoader.CustomTracks;
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

        private const float SWIRLY_SPEED = 0.5f;

        private static int _targetFramerate;
        public static bool wasPlayingReplay;
        private static bool _hasPaused, _hasRewindReplay;
        private static bool _hasReleaseToot, _lastIsTooting, _hasGreetedUser;

        private static float _elapsedTime;
        public static float gameSpeedMultiplier = 1f;

        private static string _replayUUID;
        private static string _replayFileName;

        private static NewReplaySystem _replay;
        private static ReplayManagerState _replayManagerState;
        private static Slider _replaySpeedSlider, _replayTimestampSlider;
        private static VideoPlayer _videoPlayer;
        private static TMP_Text _replayIndicatorMarquee;
        private static readonly Vector3 _marqueeScroll = new Vector3(60, 0, 0);
        private static readonly Vector3 _marqueeStartingPosition = new Vector3(500, -100, 100);
        private static GameController _currentGCInstance;
        private static EasingHelper.SecondOrderDynamics _pausePointerAnimation;
        private static GameObject _pauseArrow;
        private static Vector2 _pauseArrowDestination;

        private static GameObject _loadingSwirly, _tootTallyScorePanel;
        #region GameControllerPatches

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void GameControllerPostfixPatch(GameController __instance)
        {
            _currentGCInstance = __instance;
            if (__instance.freeplay)
            {
                gameSpeedMultiplier = __instance.smooth_scrolling_move_mult = 1f;
                return;
            }

            if (_replayFileName == null)
                OnRecordingStart(__instance);
            else
            {
                OnReplayingStart();
                SetReplayUI(__instance);
            }

            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            _pausePointerAnimation = new EasingHelper.SecondOrderDynamics(2.5f, 1f, 0.85f);

        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.playsong))]
        [HarmonyPostfix]
        public static void OnGameControllerPlaySongSetReplayStartTime()
        {
            if (_replay != null)
            {
                SetReplayUUID();
                _replay.SetStartTime();
            }

        }

        [HarmonyPatch(typeof(CurtainController), nameof(CurtainController.closeCurtain))]
        [HarmonyPostfix]
        public static void OnCurtainControllerCloseCurtainSetReplayEndTime()
        {
            if (_replay != null)
                _replay.SetEndTime();

        }

        //This is when the video player is created.
        [HarmonyPatch(typeof(BGController), nameof(BGController.setUpBGControllerRefsDelayed))]
        [HarmonyPostfix]
        public static void OnSetUpBGControllerRefsDelayedPostFix(BGController __instance)
        {
            if (_replayManagerState == ReplayManagerState.Replaying)
            {
                try
                {
                    GameObject bgObj = GameObject.Find("BGCameraObj").gameObject;
                    _videoPlayer = bgObj.GetComponentInChildren<VideoPlayer>();
                    if (_videoPlayer != null)
                    {
                        _replaySpeedSlider.onValueChanged.AddListener((float value) =>
                        {
                            _videoPlayer.playbackSpeed = value;
                        });
                        _replayTimestampSlider.onValueChanged.AddListener((float value) =>
                        {
                            _videoPlayer.time = _videoPlayer.length * value;
                        });
                    }
                }
                catch (Exception e)
                {
                    TootTallyLogger.LogWarning(e.ToString());
                    TootTallyLogger.LogInfo("Couldn't find VideoPlayer in background");
                }
            }
            else
            {
                //Have to set the speed here because the pitch is changed in 2 different places? one time during GC.Start and one during GC.loadAssetBundleResources... Derp
                _currentGCInstance.smooth_scrolling_move_mult = gameSpeedMultiplier;
                _currentGCInstance.musictrack.pitch = gameSpeedMultiplier; // SPEEEEEEEEEEEED
                TootTallyLogger.LogInfo("GameSpeed set to " + gameSpeedMultiplier);
            }
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.fixAudioMixerStuff))]
        [HarmonyPostfix]
        public static void OnFixAudioMixerStuffPostFix(GameController __instance)
        {
            if (gameSpeedMultiplier != 1f)
            {
                __instance.smooth_scrolling_move_mult = gameSpeedMultiplier;
                __instance.musictrack.outputAudioMixerGroup = __instance.audmix_bgmus_pitchshifted;
                _currentGCInstance.audmix.SetFloat("pitchShifterMult", 1f / gameSpeedMultiplier);
            }
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.startDance))]
        [HarmonyPostfix]
        public static void OnGameControllerStartDanceFixSpeedBackup(GameController __instance)
        {
            if (gameSpeedMultiplier != 1f && _currentGCInstance.musictrack.pitch != gameSpeedMultiplier)
            {
                __instance.smooth_scrolling_move_mult = gameSpeedMultiplier;
                __instance.breathscale = gameSpeedMultiplier;
                _currentGCInstance.musictrack.pitch = gameSpeedMultiplier;
                TootTallyLogger.LogInfo("BACKUP: GameSpeed set to " + gameSpeedMultiplier);
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
        [HarmonyPrefix]
        public static void PointSceneControllerPostfixPatch()
        {
            switch (_replayManagerState)
            {
                case ReplayManagerState.Recording:
                    OnRecordingStop();
                    break;
                case ReplayManagerState.Replaying:
                    OnReplayingStop();
                    break;
            }


            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
        [HarmonyPostfix]
        public static void SetPointSceneControllerCustomUI(PointSceneController __instance)
        {
            if (gameSpeedMultiplier != 1f)
            {
                Color color = Color.Lerp(new Color(.1f, .1f, .85f), Color.red, (gameSpeedMultiplier - .5f) / 1.5f);
                string colorStringHeader = $"<Color='#{ColorUtility.ToHtmlStringRGBA(color)}'>";
                string colorStringFoot = $"</Color>";
                __instance.txt_trackname.supportRichText = true;
                __instance.txt_trackname.text += $" {colorStringHeader}({gameSpeedMultiplier:0.00}x){colorStringFoot}";
            }

            GameObject lowerRightPanel = __instance.yellowwave.transform.parent.gameObject;
            try
            {
                GameObject.DestroyImmediate(lowerRightPanel.transform.Find("rule-b").gameObject);
                GameObject.DestroyImmediate(lowerRightPanel.transform.Find("rule-r").gameObject);
            }
            catch (Exception)
            {
                TootTallyLogger.LogWarning("Objects rule-b or rule-r couldn't be found.");
            }

            GameObject UICanvas = lowerRightPanel.transform.parent.gameObject;

            GameObject ttHitbox = GameObjectFactory.CreateDefaultPanel(UICanvas.transform, new Vector2(365, -23), new Vector2(56, 112), "ScorePanelHitbox");
            GameObjectFactory.CreateSingleText(ttHitbox.transform, "ScorePanelHitboxText", "<", GameTheme.themeColors.leaderboard.text, GameObjectFactory.TextFont.Multicolore);
            GameObject.DestroyImmediate(ttHitbox.transform.Find("loadingspinner_parent").gameObject);

            GameObject panelBody = GameObjectFactory.CreateDefaultPanel(UICanvas.transform, new Vector2(750, 0), new Vector2(600, 780), "TootTallyScorePanel");
            _tootTallyScorePanel = panelBody.transform.Find("scoresbody").gameObject;
            VerticalLayoutGroup vertLayout = _tootTallyScorePanel.AddComponent<VerticalLayoutGroup>();
            vertLayout.padding = new RectOffset(2, 2, 2, 2);
            vertLayout.childAlignment = TextAnchor.MiddleCenter;
            vertLayout.childForceExpandHeight = vertLayout.childForceExpandWidth = true;
            _loadingSwirly = panelBody.transform.Find("loadingspinner_parent").gameObject;



            new SlideTooltip(ttHitbox, panelBody, new Vector2(750, 0), new Vector2(225, 0));

        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.doneWithCountUp))]
        [HarmonyPostfix]
        public static void OnDoneWIthCountUpUpdateFCLogo(PointSceneController __instance)
        {
            if (_replay.IsFullCombo)
                DisplayFCLogo(__instance);
        }

        //This is just in case they have InstantScore Plugin
        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.doCoins))]
        [HarmonyPostfix]
        public static void OnDoCoinsUpdateFCLogo(PointSceneController __instance)
        {
            if (_replay.IsFullCombo)
                DisplayFCLogo(__instance);
        }

        private static void DisplayFCLogo(PointSceneController __instance)
        {
            __instance.giantscoretext.text = "You suck";
            __instance.giantscoretext.fontSize = 20;
            __instance.giantscoretext.horizontalOverflow = HorizontalWrapMode.Overflow;
            __instance.giantscoretextshad.text = "You suck";
            __instance.giantscoretextshad.fontSize = 20;
            __instance.giantscoretextshad.horizontalOverflow = HorizontalWrapMode.Overflow;
            if (_replay.IsTripleS)
                __instance.giantscorediamond.transform.Find("cool-s").GetComponent<Image>().sprite = AssetManager.GetSprite("Cool-sss.png");
            __instance.giantscorediamond.transform.Find("cool-s").gameObject.SetActive(true);
        }


        public static void ShowLoadingSwirly() => _loadingSwirly.SetActive(true); public static void HideLoadingSwirly() => _loadingSwirly.SetActive(false);

        public static void UpdateLoadingSwirlyAnimation()
        {
            _loadingSwirly?.GetComponent<RectTransform>().Rotate(0, 0, 1000 * Time.deltaTime * SWIRLY_SPEED);
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Update))]
        [HarmonyPostfix]
        public static void OnPointSceneControllerUpdateDoSwirlyAnimation()
        {
            UpdateLoadingSwirlyAnimation();
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
        public static bool AvoidSaveChange() => !wasPlayingReplay; // Don't touch the savefile if we just did a replay

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.checkScoreCheevos))]
        [HarmonyPrefix]
        public static bool AvoidAchievementCheck() => !wasPlayingReplay; // Don't check for achievements if we just did a replay

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
                    if (!_hasRewindReplay) //have to skip a frame when rewinding because dev is using LeanTween to move the play area... and it only updates on the second frame after rewinding :|
                        _replay.PlaybackReplay(__instance);
                    _hasRewindReplay = false;
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
                    _replayTimestampSlider.SetValueWithoutNotify(__instance.musictrack.time / __instance.musictrack.clip.length);
                    if (_replayIndicatorMarquee.text.Equals(""))
                    {
                        _replayIndicatorMarquee.text = $"Watching {_replay.GetUsername} play {_replay.GetSongName}" + (gameSpeedMultiplier != 1f ? $" [{gameSpeedMultiplier:0.00}x]" : "");
                    }
                    _replayIndicatorMarquee.transform.localPosition -= _marqueeScroll * Time.deltaTime;
                    if (_replayIndicatorMarquee.transform.localPosition.x <= -1000)
                    {
                        _replayIndicatorMarquee.transform.localPosition = _marqueeStartingPosition;
                    }
                    break;
                case ReplayManagerState.Paused:
                    if (_pauseArrowDestination != null)
                        _pauseArrow.GetComponent<RectTransform>().anchoredPosition = _pausePointerAnimation.GetNewVector(_pauseArrowDestination, Time.deltaTime);
                    break;
            }
            float value = 0;
            if (__instance.noteplaying && __instance.breathcounter < 1f)
            {
                value = Time.deltaTime * (1 - gameSpeedMultiplier) * -0.22f;
            }
            else if (!__instance.noteplaying && __instance.breathcounter > 0f)
            {
                if (!__instance.outofbreath)
                    value = Time.deltaTime * (1 - gameSpeedMultiplier) * 8.5f;
                else
                    value = Time.deltaTime * (1 - gameSpeedMultiplier) * .29f;
            }
            __instance.breathcounter += value;
            if (__instance.breathcounter >= 1f) { __instance.breathcounter = .99f; }
            /*
             * Refer to this function where the outOfBreath detection happens... no idea why there's a breathcount < 1f if statement
             * Token: 0x06000268 RID: 616 RVA: 0x000260D8 File Offset: 0x000242D8
            */

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
                    TootTallyLogger.LogInfo($"UUID deleted: {_replayUUID}");
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
                gameSpeedMultiplier = 1f;
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
                    gameSpeedMultiplier = _replay.GetReplaySpeed;
                    levelSelectControllerInstance.playbtn.onClick?.Invoke();
                    break;

                case NewReplaySystem.ReplayState.ReplayLoadNotFound:
                    PopUpNotifManager.DisplayNotif("Downloading replay...", GameTheme.themeColors.notification.defaultText);
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadReplay(replayId, uuid =>
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
            var trackRef = GlobalVariables.chosen_track;
            var track = TrackLookup.lookup(trackRef);

            StartAPICallCoroutine(track);
        }

        public static void StartAPICallCoroutine(TromboneTrack track)
        {
            var songHash = SongDataHelper.GetSongHash(track);
            var songFilePath = SongDataHelper.GetSongFilePath(track);
            var isCustom = track is CustomTrack;

            TootTallyLogger.LogInfo($"Requesting UUID for {songHash}");
            Plugin.Instance.StartCoroutine(TootTallyAPIService.GetHashInDB(songHash, isCustom, songHashInDB =>
            {
                if (songHashInDB == 0)
                    _replayUUID = null;
                else
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetReplayUUID(SongDataHelper.GetChoosenSongHash(), UUID => _replayUUID = UUID));
            }));
        }

        public static void OnRecordingStart(GameController __instance)
        {
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
            _lastIsTooting = _hasRewindReplay = false;
            wasPlayingReplay = true;
            _replayManagerState = ReplayManagerState.Replaying;
            TootTallyLogger.LogInfo("Replay Started");
        }

        public static void OnRecordingStop()
        {
            _replayManagerState = ReplayManagerState.None;

            if (AutoTootCompatibility.enabled && AutoTootCompatibility.WasAutoUsed)
            {
                TootTallyLogger.LogInfo("AutoToot used, skipping replay submission.");
                return; // Don't submit anything if AutoToot was used.
            }
            if (HoverTootCompatibility.enabled && HoverTootCompatibility.DidToggleThisSong)
            {
                TootTallyLogger.LogInfo("HoverToot used, skipping replay submission.");
                return; // Don't submit anything if HoverToot was used.
            }
            if (CircularBreathingCompatibility.enabled && CircularBreathingCompatibility.IsActivated)
            {
                PopUpNotifManager.DisplayNotif("Circular Breathing enabled, Score submission disabled.", GameTheme.themeColors.notification.warningText);
                TootTallyLogger.LogInfo("CircularBreathing used, skipping replay submission.");
                return; // Don't submit anything if Circular Breathing is enabled
            }
            if (_hasPaused)
            {
                PopUpNotifManager.DisplayNotif("Pausing not allowed, Score submission disabled.", GameTheme.themeColors.notification.warningText);
                TootTallyLogger.LogInfo("Paused during gameplay, skipping replay submission.");
                return; //Don't submit if paused during the play
            }

            if (_replayUUID == null)
            {
                TootTallyLogger.LogInfo("Replay UUID was null, skipping replay submission.");
                return; //Dont save or upload if no UUID
            }


            SaveReplayToFile();
            if (Plugin.userInfo.username != "Guest" && Plugin.userInfo.allowSubmit) //Don't upload if logged in as a Guest or doesn't allowSubmit
                SendReplayFileToServer();
        }

        private static void SaveReplayToFile()
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays");
            TootTallyLogger.DebugModeLog("Replay directory: " + replayDir);

            // Create Replays directory in case it doesn't exist
            if (!Directory.Exists(replayDir))
            {
                TootTallyLogger.DebugModeLog("Replay directory not found. Creating new Replay folder directory.");
                Directory.CreateDirectory(replayDir);
            }

            try
            {
                FileHelper.WriteJsonToFile(replayDir + "\\", _replayUUID + ".ttr", _replay.GetRecordedReplayJson(_replayUUID, _targetFramerate));
            }
            catch (Exception e)
            {
                TootTallyLogger.LogError(e.Message);
            }
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
            _replaySpeedSlider.value = 1;
            GameObject sliderHandle = _replaySpeedSlider.transform.Find("Handle Slide Area/Handle").gameObject;
            sliderHandle.GetComponent<Image>().color = GameTheme.themeColors.scrollSpeedSlider.handle;

            //Text above the slider
            TMP_Text floatingSpeedText = GameObjectFactory.CreateSingleText(_replaySpeedSlider.transform, "SpeedSliderFloatingText", "SPEED", new Color(1, 1, 1, 1));
            floatingSpeedText.fontSize = 14;
            floatingSpeedText.alignment = TextAlignmentOptions.Center;
            floatingSpeedText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2, 30);

            //Text inside the slider
            TMP_Text replaySpeedSliderText = GameObjectFactory.CreateSingleText(sliderHandle.transform, "replaySliderText", "100", Color.black);
            replaySpeedSliderText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 5);
            replaySpeedSliderText.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 21);
            replaySpeedSliderText.alignment = TextAlignmentOptions.Top;
            replaySpeedSliderText.fontSize = 8;
            replaySpeedSliderText.text = BetterScrollSpeedSliderPatcher.SliderValueToText(_replaySpeedSlider.value);
            replaySpeedSliderText.color = GameTheme.themeColors.scrollSpeedSlider.text;
            _replaySpeedSlider.fillRect.gameObject.GetComponent<Image>().color = GameTheme.themeColors.scrollSpeedSlider.fill;
            _replaySpeedSlider.transform.Find("Background").GetComponent<Image>().color = GameTheme.themeColors.scrollSpeedSlider.background;
            _replaySpeedSlider.onValueChanged.AddListener((float value) =>
            {
                __instance.musictrack.pitch = _replaySpeedSlider.value * gameSpeedMultiplier;
                Time.timeScale = _replaySpeedSlider.value;
                replaySpeedSliderText.text = BetterScrollSpeedSliderPatcher.SliderValueToText(_replaySpeedSlider.value);
                __instance.musictrack.outputAudioMixerGroup = __instance.audmix_bgmus_pitchshifted;
                _currentGCInstance.audmix.SetFloat("pitchShifterMult", 1f / (_replaySpeedSlider.value * gameSpeedMultiplier));
                EventSystem.current.SetSelectedGameObject(null);
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

            _replayTimestampSlider.onValueChanged.AddListener((float value) =>
            {
                __instance.musictrack.time = __instance.musictrack.clip.length * value;
                _currentGCInstance.syncTrackPositions((float)__instance.musictrack.time); //SyncTrack in case smooth scrolling is on
                var oldIndex = __instance.currentnoteindex;
                var noteHolderNewLocalPosX = __instance.zeroxpos + (__instance.musictrack.time - __instance.latency_offset - __instance.noteoffset) * -__instance.trackmovemult;
                __instance.currentnoteindex = Mathf.Clamp(__instance.allnotevals.FindIndex(note => note[0] >= Mathf.Abs(noteHolderNewLocalPosX)), 1, __instance.allnotevals.Count - 1) - 1;
                __instance.grabNoteRefs(0); //the parameter is the note increment. Putting 0 just gets the noteData for currentnoteindex's value
                __instance.beatstoshow = __instance.currentnoteindex + 64; // hardcoded 64 for now but ultimately depends on what people use in Trombloader's config
                for (int i = __instance.currentnoteindex; i <= oldIndex + 64; i++)
                {
                    __instance.allnotes[i].GetComponent<RectTransform>().localScale = Vector3.one;
                    __instance.allnotes[i].SetActive(i <= __instance.beatstoshow);
                }

                for (int i = __instance.currentnoteindex; i <= __instance.beatstoshow && i < __instance.allnotes.Count - 1; i++)
                    __instance.allnotes[i].SetActive(true);

                _replay.OnReplayRewind(noteHolderNewLocalPosX, __instance);
                _hasRewindReplay = true;
                EventSystem.current.SetSelectedGameObject(null);
            });
            _replayTimestampSlider.gameObject.SetActive(true);
        }

        private static void SetReplayMarquees(Transform canvasTransform)
        {
            _replayIndicatorMarquee = GameObjectFactory.CreateSingleText(canvasTransform, "ReplayMarquee", "", new Color(1f, 1f, 1f, 0.75f));
            _replayIndicatorMarquee.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 60);
            //_replayIndicatorMarquee.outlineWidth = 0.2f //doesn't work... don't know why.
            _replayIndicatorMarquee.fontSize = 14;
            _replayIndicatorMarquee.transform.localPosition = _marqueeStartingPosition;
            _replayIndicatorMarquee.enableWordWrapping = true;
        }

        private static void SendReplayFileToServer()
        {
            //Using replayUUID as a name
            Plugin.Instance.StartCoroutine(TootTallyAPIService.SubmitReplay(_replayUUID + ".ttr", _replayUUID, (replaySubmissionReply) =>
            {
                if (replaySubmissionReply == null)
                {
                    HideLoadingSwirly();
                    GameObjectFactory.CreateSingleText(_tootTallyScorePanel.transform, "TextMain", "Score submission disabled for that map.", GameTheme.themeColors.leaderboard.text);
                    return;
                }
                if (replaySubmissionReply.tt != 0)
                {
                    HideLoadingSwirly();
                    var rankDiff = Math.Abs(replaySubmissionReply.ranking - Plugin.userInfo.rank);
                    var displayMessage = "Replay submitted." + (replaySubmissionReply.isBestPlay ? " New Personal best!\n" : "\n");
                    displayMessage += $"#{replaySubmissionReply.position} {replaySubmissionReply.tt:0.00}tt\n";
                    if (replaySubmissionReply.isBestPlay)
                        displayMessage += $"Rank: {Plugin.userInfo.rank} -> {replaySubmissionReply.ranking} (+{rankDiff})";
                    PopUpNotifManager.DisplayNotif(displayMessage, GameTheme.themeColors.notification.defaultText);
                    GameObjectFactory.CreateSingleText(_tootTallyScorePanel.transform, "TextSubmit", "Replay submitted." + (replaySubmissionReply.isBestPlay ? " New Personal best!" : ""), GameTheme.themeColors.leaderboard.text);
                    GameObjectFactory.CreateSingleText(_tootTallyScorePanel.transform, "TextPosition", $"#{replaySubmissionReply.position} {replaySubmissionReply.tt:0.00}tt", GameTheme.themeColors.leaderboard.text);
                    if (replaySubmissionReply.isBestPlay)
                        GameObjectFactory.CreateSingleText(_tootTallyScorePanel.transform, "TextRank", $"Rank: {Plugin.userInfo.rank} -> {replaySubmissionReply.ranking} (+{rankDiff})", GameTheme.themeColors.leaderboard.text);
                }
                else
                {
                    HideLoadingSwirly();
                    GameObjectFactory.CreateSingleText(_tootTallyScorePanel.transform, "TextMain", "Map not rated, no data found.", GameTheme.themeColors.leaderboard.text);
                    GameObjectFactory.CreateSingleText(_tootTallyScorePanel.transform, "TextPosition", $"Score position: #{replaySubmissionReply.position}", GameTheme.themeColors.leaderboard.text);
                }
            }));
        }

        public static void OnReplayingStop()
        {
            _replay.OnReplayPlayerStop();
            _replayFileName = null;
            GlobalVariables.localsave.tracks_played--;
            Time.timeScale = 1f;

            _replayManagerState = ReplayManagerState.None;
            TootTallyLogger.LogInfo("Replay finished");
        }

        public static void OnPauseAddReplayButton(PauseCanvasController __instance)
        {
            __instance.panelrect.sizeDelta = new Vector2(290, 220);
            GameObject exitbtn = __instance.panelobj.transform.Find("buttons/ButtonRetry").gameObject;
            GameObject replayBtn = GameObject.Instantiate(exitbtn, __instance.panelobj.transform.Find("buttons"));

            replayBtn.name = "ButtonReplay";
            replayBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, -121);
            //replayBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 40);
            replayBtn.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            replayBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                PopUpNotifManager.DisplayNotif("Temp Replays currently under maintenance.", GameTheme.themeColors.notification.warningText);
                /*_replayFileName = "TempReplay";
                _replay.SetUsernameAndSongName(Plugin.userInfo.username, GlobalVariables.chosen_track_data.trackname_long);
                TootTallyLogger.DebugModeLog("TempReplay Loaded");
                _currentGCInstance.pauseRetryLevel();*/
            });
            Text replayText = replayBtn.transform.Find("RETRY").GetComponent<Text>();
            replayText.name = "ReplayText";
            replayText.supportRichText = true;
            replayText.text = "View Replay";
            replayText.alignment = TextAnchor.MiddleCenter;
            replayText.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            replayText.GetComponent<RectTransform>().sizeDelta = new Vector2(205, 44);

            EventTrigger replayBtnEvent = replayBtn.AddComponent<EventTrigger>();
            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) => OnPauseMenuButtonOver(__instance, new object[] { 3 }));
            replayBtnEvent.triggers.Add(pointerEnterEvent);
            _pauseArrowDestination = new Vector2(28, -37);
        }

        [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.mouseOverButton))]
        [HarmonyPrefix]
        public static bool OnPauseMenuButtonOver(PauseCanvasController __instance, object[] __args)
        {
            _pausePointerAnimation.SetStartVector(__instance.pausearrowr.anchoredPosition);
            _pauseArrowDestination = new Vector2(25, -46 * ((int)__args[0] - 1) - 73);
            return false;
        }

        public static void OnPauseChangeButtonText(PauseCanvasController __instance)
        {
            GameObject resumebtn = __instance.panelobj.transform.Find("buttons/ButtonResume").gameObject;
            resumebtn.transform.Find("RESUME").GetComponent<Text>().text = "Resume Replay";

            GameObject exitbtn = __instance.panelobj.transform.Find("buttons/ButtonExit").gameObject;
            exitbtn.transform.Find("EXIT").GetComponent<Text>().text = "Exit Replay";

            GameObject retrybtn = __instance.panelobj.transform.Find("buttons/ButtonRetry").gameObject;
            retrybtn.transform.Find("RETRY").GetComponent<Text>().text = "Restart Replay";
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
