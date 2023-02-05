using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public static class GameThemeManager
    {
        private const string CONFIG_FIELD = "Themes";
        private const ThemeTypes DEFAULT_THEME = ThemeTypes.Default;
        public static Text songyear, songgenre, songcomposer, songtempo, songduration, songdesctext;
        public static Options option;
        private static ThemeTypes _currentTheme;
        private static bool _isInitialized;

        //can very well be optimized but fuck it xd
        public static void SetTheme(ThemeTypes themeType)
        {
            _currentTheme = themeType;
            GameTheme.isDefault = false;
            switch (themeType)
            {
                case ThemeTypes.Day:
                    GameTheme.SetDayTheme();
                    break;
                case ThemeTypes.Night:
                    GameTheme.SetNightTheme();
                    break;
                case ThemeTypes.Aradigm:
                    GameTheme.SetCustomTheme("Aradigm");
                    break;
                case ThemeTypes.Citrus:
                    GameTheme.SetCustomTheme("Citrus");
                    break;
                case ThemeTypes.Custom:
                    GameTheme.SetCustomTheme("Custom");
                    break;
                case ThemeTypes.Electro:
                    GameTheme.SetCustomTheme("Electro");
                    break;
                case ThemeTypes.Gloomhonk:
                    GameTheme.SetCustomTheme("Gloomhonk");
                    break;
                case ThemeTypes.Guardie:
                    GameTheme.SetCustomTheme("Guardie");
                    break;
                case ThemeTypes.Jeff:
                    GameTheme.SetCustomTheme("Jeff");
                    break;
                case ThemeTypes.JoeSickPack:
                    GameTheme.SetCustomTheme("Joes Sick pack");
                    break;
                case ThemeTypes.Katiny:
                    GameTheme.SetCustomTheme("Katiny");
                    break;
                case ThemeTypes.Lavender:
                    GameTheme.SetCustomTheme("Lavender");
                    break;
                case ThemeTypes.NightRider:
                    GameTheme.SetCustomTheme("Night Rider");
                    break;
                case ThemeTypes.Perandus:
                    GameTheme.SetCustomTheme("Perandus");
                    break;
                case ThemeTypes.Samuran:
                    GameTheme.SetCustomTheme("Samuran");
                    break;
                case ThemeTypes.Random:
                    GameTheme.SetRandomTheme();
                    break;
                default:
                    GameTheme.SetDefaultTheme();
                    break;
            }
            GameObjectFactory.UpdatePrefabTheme();
        }

        public static void Initialize()
        {
            if (_isInitialized) return;

            string configPath = Path.Combine(Paths.BepInExRootPath, "config/");
            ConfigFile config = new ConfigFile(configPath + Plugin.CONFIG_NAME, true);

            option = new Options()
            {
                Theme = config.Bind(CONFIG_FIELD, nameof(option.Theme), DEFAULT_THEME),
                CustomTrombColor = config.Bind(CONFIG_FIELD, "Custom Tromb Color", false),
                TrombRed = config.Bind(CONFIG_FIELD, "Tromb Red", 1f),
                TrombGreen = config.Bind(CONFIG_FIELD, "Tromb Green", 1f),
                TrombBlue = config.Bind(CONFIG_FIELD, "Tromb Blue", 1f),
            };
            config.SettingChanged += Config_SettingChanged;

            object settings = OptionalTrombSettings.GetConfigPage("TootTally");
            if (settings != null)
            {
                OptionalTrombSettings.Add(settings, option.Theme);
                OptionalTrombSettings.Add(settings, option.CustomTrombColor);
                OptionalTrombSettings.AddSlider(settings, 0, 1, .001f, false, option.TrombRed);
                OptionalTrombSettings.AddSlider(settings, 0, 1, .001f, false, option.TrombGreen);
                OptionalTrombSettings.AddSlider(settings, 0, 1, .001f, false, option.TrombBlue);
            }

            SetTheme(option.Theme.Value);
            _isInitialized = true;
        }

        private static void Config_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            SetTheme(option.Theme.Value);
            PopUpNotifManager.DisplayNotif("New Theme Loaded!", GameTheme.themeColors.notification.defaultText);
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void ChangeThemeOnLevelSelectControllerStartPostFix(LevelSelectController __instance)
        {
            if (GameTheme.isDefault) return;

            foreach (GameObject btn in __instance.btns)
            {
                btn.transform.Find("ScoreText").gameObject.GetComponent<Text>().color = GameTheme.themeColors.leaderboard.text;
            }

            #region SongButton
            GameObject btnBGPrefab = GameObject.Instantiate(__instance.btnbgs[0].gameObject);
            GameObject.DestroyImmediate(btnBGPrefab.transform.Find("Image").gameObject);

            for (int i = 0; i < 7; i++) //songbuttons only, not the arrow ones
            {
                Image img = __instance.btnbgs[i];
                img.sprite = AssetManager.GetSprite("SongButtonBackground.png");
                img.transform.parent.Find("Text").GetComponent<Text>().color = i == 0 ? GameTheme.themeColors.songButton.textOver : GameTheme.themeColors.songButton.text;

                GameObject btnBGShadow = GameObject.Instantiate(btnBGPrefab, img.gameObject.transform.parent);
                btnBGShadow.name = "Shadow";
                OverwriteGameObjectSpriteAndColor(btnBGShadow, "SongButtonShadow.png", GameTheme.themeColors.songButton.shadow);

                GameObject btnBGOutline = GameObject.Instantiate(btnBGPrefab, img.gameObject.transform);
                btnBGOutline.name = "Outline";
                OverwriteGameObjectSpriteAndColor(btnBGOutline, "SongButtonOutline.png", i == 0 ? GameTheme.themeColors.songButton.outlineOver : GameTheme.themeColors.songButton.outline);

                img.transform.Find("Image").GetComponent<Image>().color = GameTheme.themeColors.songButton.square;
                img.color = GameTheme.themeColors.songButton.background;
            }

            for (int i = 7; i < __instance.btnbgs.Length; i++) //these are the arrow ones :}
                __instance.btnbgs[i].color = GameTheme.themeColors.songButton.background;
            GameObject.DestroyImmediate(btnBGPrefab);
            #endregion

            #region SongTitle
            __instance.songtitlebar.GetComponent<Image>().color = GameTheme.themeColors.title.titleBar;
            __instance.scenetitle.GetComponent<Text>().color = GameTheme.themeColors.title.titleShadow;
            GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "title/GameObject").GetComponent<Text>().color = GameTheme.themeColors.title.title;
            __instance.longsongtitle.color = GameTheme.themeColors.title.songName;
            #endregion

            #region Lines
            GameObject lines = __instance.btnspanel.transform.Find("RightLines").gameObject;
            lines.GetComponent<RectTransform>().anchoredPosition += new Vector2(-2, 0);
            LineRenderer redLine = lines.transform.Find("Red").GetComponent<LineRenderer>();
            redLine.startColor = GameTheme.themeColors.leaderboard.panelBody;
            redLine.endColor = GameTheme.themeColors.leaderboard.scoresBody;
            for (int i = 1; i < 8; i++)
            {
                LineRenderer yellowLine = lines.transform.Find("Yellow" + i).GetComponent<LineRenderer>();
                yellowLine.startColor = GameTheme.themeColors.leaderboard.panelBody;
                yellowLine.endColor = GameTheme.themeColors.leaderboard.scoresBody;
            }
            #endregion

            #region Capsules
            GameObject capsules = GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "capsules").gameObject;
            GameObject capsulesPrefab = GameObject.Instantiate(capsules);

            foreach (Transform t in capsulesPrefab.transform) GameObject.Destroy(t.gameObject);
            RectTransform rectTrans = capsulesPrefab.GetComponent<RectTransform>();
            rectTrans.localScale = Vector3.one;
            rectTrans.anchoredPosition = Vector2.zero;


            GameObject capsulesYearShadow = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesYearShadow, "YearCapsule.png", GameTheme.themeColors.capsules.yearShadow);
            capsulesYearShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

            GameObject capsulesYear = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesYear, "YearCapsule.png", GameTheme.themeColors.capsules.year);

            songyear = GameObject.Instantiate(__instance.songyear, capsulesYear.transform);

            GameObject capsulesGenreShadow = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesGenreShadow, "GenreCapsule.png", GameTheme.themeColors.capsules.genreShadow);
            capsulesGenreShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

            GameObject capsulesGenre = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesGenre, "GenreCapsule.png", GameTheme.themeColors.capsules.genre);
            songgenre = GameObject.Instantiate(__instance.songgenre, capsulesGenre.transform);

            GameObject capsulesComposerShadow = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesComposerShadow, "ComposerCapsule.png", GameTheme.themeColors.capsules.composerShadow);
            capsulesComposerShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

            GameObject capsulesComposer = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesComposer, "ComposerCapsule.png", GameTheme.themeColors.capsules.composer);
            songcomposer = GameObject.Instantiate(__instance.songcomposer, capsulesComposer.transform);

            GameObject capsulesTempo = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesTempo, "BPMTimeCapsule.png", GameTheme.themeColors.capsules.tempo);
            songtempo = GameObject.Instantiate(__instance.songtempo, capsulesTempo.transform);
            songduration = GameObject.Instantiate(__instance.songduration, capsulesTempo.transform);

            GameObject capsulesDescTextShadow = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesDescTextShadow, "DescCapsule.png", GameTheme.themeColors.capsules.descriptionShadow);
            capsulesDescTextShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

            GameObject capsulesDescText = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesDescText, "DescCapsule.png", GameTheme.themeColors.capsules.description);
            songdesctext = GameObject.Instantiate(__instance.songdesctext, capsulesDescText.transform);

            GameObject.DestroyImmediate(capsules.GetComponent<Image>());
            GameObject.DestroyImmediate(capsulesPrefab);
            #endregion

            #region PlayButton
            GameObject playButtonBG = __instance.playbtn.transform.Find("BG").gameObject;
            GameObject playBGPrefab = GameObject.Instantiate(playButtonBG, __instance.playbtn.transform);
            foreach (Transform t in playBGPrefab.transform) GameObject.Destroy(t.gameObject);

            GameObject playBackgroundImg = GameObject.Instantiate(playBGPrefab, __instance.playbtn.transform);
            playBackgroundImg.name = "playBackground";
            OverwriteGameObjectSpriteAndColor(playBackgroundImg, "PlayBackground.png", GameTheme.themeColors.playButton.background);

            GameObject playOutline = GameObject.Instantiate(playBGPrefab, __instance.playbtn.transform);
            playOutline.name = "playOutline";
            OverwriteGameObjectSpriteAndColor(playOutline, "PlayOutline.png", GameTheme.themeColors.playButton.outline);

            GameObject playText = GameObject.Instantiate(playBGPrefab, __instance.playbtn.transform);
            playText.name = "playText";
            OverwriteGameObjectSpriteAndColor(playText, "PlayText.png", GameTheme.themeColors.playButton.text);

            GameObject playShadow = GameObject.Instantiate(playBGPrefab, __instance.playbtn.transform);
            playShadow.name = "playShadow";
            OverwriteGameObjectSpriteAndColor(playShadow, "PlayShadow.png", GameTheme.themeColors.playButton.shadow);

            GameObject.DestroyImmediate(playButtonBG);
            GameObject.DestroyImmediate(playBGPrefab);
            #endregion

            #region BackButton
            GameObject backButtonBG = __instance.backbutton.transform.Find("BG").gameObject;
            GameObject backBGPrefab = GameObject.Instantiate(backButtonBG, __instance.backbutton.transform);
            foreach (Transform t in backBGPrefab.transform) GameObject.Destroy(t.gameObject);

            GameObject backBackgroundImg = GameObject.Instantiate(backBGPrefab, __instance.backbutton.transform);
            backBackgroundImg.name = "backBackground";
            OverwriteGameObjectSpriteAndColor(backBackgroundImg, "BackBackground.png", GameTheme.themeColors.backButton.background);

            GameObject backOutline = GameObject.Instantiate(backBGPrefab, __instance.backbutton.transform);
            backOutline.name = "backOutline";
            OverwriteGameObjectSpriteAndColor(backOutline, "BackOutline.png", GameTheme.themeColors.backButton.outline);

            GameObject backText = GameObject.Instantiate(backBGPrefab, __instance.backbutton.transform);
            backText.name = "backText";
            OverwriteGameObjectSpriteAndColor(backText, "BackText.png", GameTheme.themeColors.backButton.text);

            GameObject backShadow = GameObject.Instantiate(backBGPrefab, __instance.backbutton.transform);
            backShadow.name = "backShadow";
            OverwriteGameObjectSpriteAndColor(backShadow, "BackShadow.png", GameTheme.themeColors.backButton.shadow);

            GameObject.DestroyImmediate(backButtonBG);
            GameObject.DestroyImmediate(backBGPrefab);
            #endregion

            #region RandomButton
            __instance.btnrandom.transform.Find("Text").GetComponent<Text>().color = GameTheme.themeColors.randomButton.text;

            GameObject randomButtonPrefab = GameObject.Instantiate(__instance.btnrandom);
            RectTransform randomRectTransform = randomButtonPrefab.GetComponent<RectTransform>();
            randomRectTransform.anchoredPosition = Vector2.zero;
            randomRectTransform.localScale = Vector3.one;
            GameObject.DestroyImmediate(randomButtonPrefab.GetComponent<Button>());

            GameObject randomButtonBackground = GameObject.Instantiate(randomButtonPrefab, __instance.btnrandom.transform);
            randomButtonBackground.name = "RandomBackground";
            OverwriteGameObjectSpriteAndColor(randomButtonBackground, "RandomBackground.png", GameTheme.themeColors.randomButton.background);

            foreach (Transform t in randomButtonPrefab.transform) GameObject.DestroyImmediate(t.gameObject); // destroying text only after making our background object

            GameObject randomButtonOutline = GameObject.Instantiate(randomButtonPrefab, __instance.btnrandom.transform);
            randomButtonOutline.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -1);
            randomButtonOutline.name = "RandomOutline";
            OverwriteGameObjectSpriteAndColor(randomButtonOutline, "RandomOutline.png", GameTheme.themeColors.randomButton.outline);

            GameObject randomButtonIcon = GameObject.Instantiate(randomButtonPrefab, __instance.btnrandom.transform);
            randomButtonIcon.name = "RandomIcon";
            OverwriteGameObjectSpriteAndColor(randomButtonIcon, "RandomIcon.png", GameTheme.themeColors.randomButton.text);

            GameObject.DestroyImmediate(__instance.btnrandom.GetComponent<Image>());
            GameObject.DestroyImmediate(randomButtonPrefab);

            EventTrigger randomBtnEvents = __instance.btnrandom.AddComponent<EventTrigger>();
            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) => OnPointerEnterRandomEvent(__instance));
            randomBtnEvents.triggers.Add(pointerEnterEvent);

            EventTrigger.Entry pointerExitEvent = new EventTrigger.Entry();
            pointerExitEvent.eventID = EventTriggerType.PointerExit;
            pointerExitEvent.callback.AddListener((data) => OnPointerLeaveRandomEvent(__instance));
            randomBtnEvents.triggers.Add(pointerExitEvent);

            #endregion

            #region PointerArrow
            GameObject arrowPointerPrefab = GameObject.Instantiate(__instance.pointerarrow.gameObject);
            OverwriteGameObjectSpriteAndColor(__instance.pointerarrow.gameObject, "pointerBG.png", GameTheme.themeColors.pointer.background);

            GameObject arrowPointerShadow = GameObject.Instantiate(arrowPointerPrefab, __instance.pointerarrow.transform);
            arrowPointerShadow.name = "Shadow";
            arrowPointerShadow.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            OverwriteGameObjectSpriteAndColor(arrowPointerShadow, "pointerShadow.png", GameTheme.themeColors.pointer.shadow);

            GameObject arrowPointerPointerOutline = GameObject.Instantiate(arrowPointerPrefab, __instance.pointerarrow.transform);
            arrowPointerPointerOutline.name = "Outline";
            arrowPointerPointerOutline.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            OverwriteGameObjectSpriteAndColor(arrowPointerPointerOutline, "pointerOutline.png", GameTheme.themeColors.pointer.outline);

            GameObject.DestroyImmediate(arrowPointerPrefab);
            #endregion

            #region Background
            __instance.bgdots.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 165.5f);
            __instance.bgdots.transform.Find("Image").GetComponent<Image>().color = GameTheme.themeColors.background.dots;
            __instance.bgdots.transform.Find("Image (1)").GetComponent<Image>().color = GameTheme.themeColors.background.dots;
            __instance.bgdots2.transform.Find("Image").GetComponent<Image>().color = GameTheme.themeColors.background.dots2;
            GameObject extraDotsBecauseGameDidntLeanTweenFarEnoughSoWeCanSeeTheEndOfTheTextureFix = GameObject.Instantiate(__instance.bgdots.transform.Find("Image").gameObject, __instance.bgdots.transform.Find("Image").transform);
            extraDotsBecauseGameDidntLeanTweenFarEnoughSoWeCanSeeTheEndOfTheTextureFix.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -1010);
            GameObject.Find("bgcamera").GetComponent<Camera>().backgroundColor = GameTheme.themeColors.background.background;
            GameObject.Find("BG Shape").GetComponent<Image>().color = GameTheme.themeColors.background.shape;
            GameObject MainCanvas = GameObject.Find("MainCanvas").gameObject;
            MainCanvas.transform.Find("FullScreenPanel/diamond").GetComponent<Image>().color = GameTheme.themeColors.background.diamond;
            #endregion


            //CapsulesTextColor
            songyear.color = GameTheme.themeColors.leaderboard.text;
            songgenre.color = GameTheme.themeColors.leaderboard.text;
            songduration.color = GameTheme.themeColors.leaderboard.text;
            songcomposer.color = GameTheme.themeColors.leaderboard.text;
            songtempo.color = GameTheme.themeColors.leaderboard.text;
            songdesctext.color = GameTheme.themeColors.leaderboard.text;
            OnAdvanceSongsPostFix(__instance);
        }

        #region hoverAndUnHoverSongButtons
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverBtn))]
        [HarmonyPostfix]
        public static void OnHoverBtnPostfix(LevelSelectController __instance, object[] __args)
        {
            if (GameTheme.isDefault) return;
            if ((int)__args[0] >= 7)
            {
                __instance.btnbgs[(int)__args[0]].GetComponent<Image>().color = GameTheme.themeColors.songButton.outline;
                return;
            }
            __instance.btnbgs[(int)__args[0]].GetComponent<Image>().color = GameTheme.themeColors.songButton.background;
            __instance.btnbgs[(int)__args[0]].transform.Find("Outline").GetComponent<Image>().color = GameTheme.themeColors.songButton.outlineOver;
            __instance.btnbgs[(int)__args[0]].transform.parent.Find("Text").GetComponent<Text>().color = GameTheme.themeColors.songButton.textOver;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.unHoverBtn))]
        [HarmonyPostfix]
        public static void OnUnHoverBtnPostfix(LevelSelectController __instance, object[] __args)
        {
            if (GameTheme.isDefault) return;
            if ((int)__args[0] >= 7)
            {
                __instance.btnbgs[(int)__args[0]].GetComponent<Image>().color = GameTheme.themeColors.songButton.background;
                return;
            }
            __instance.btnbgs[(int)__args[0]].GetComponent<Image>().color = GameTheme.themeColors.songButton.background;
            __instance.btnbgs[(int)__args[0]].transform.Find("Outline").GetComponent<Image>().color = GameTheme.themeColors.songButton.outline;
            __instance.btnbgs[(int)__args[0]].transform.parent.Find("Text").GetComponent<Text>().color = GameTheme.themeColors.songButton.text;
        }
        #endregion

        #region PlayAndBackEvents
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverPlay))]
        [HarmonyPrefix]
        public static bool OnHoverPlayBypassIfThemeNotDefault(LevelSelectController __instance)
        {
            if (GameTheme.isDefault) return true;
            __instance.hoversfx.Play();
            __instance.playhovering = true;
            __instance.playbtnobj.transform.Find("playBackground").GetComponent<Image>().color = GameTheme.themeColors.playButton.backgroundOver;
            __instance.playbtnobj.transform.Find("playOutline").GetComponent<Image>().color = GameTheme.themeColors.playButton.outlineOver;
            __instance.playbtnobj.transform.Find("playText").GetComponent<Image>().color = GameTheme.themeColors.playButton.textOver;
            __instance.playbtnobj.transform.Find("playShadow").GetComponent<Image>().color = GameTheme.themeColors.playButton.shadowOver;
            return false;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.unHoverPlay))]
        [HarmonyPrefix]
        public static bool OnUnHoverPlayBypassIfThemeNotDefault(LevelSelectController __instance)
        {
            if (GameTheme.isDefault) return true;
            __instance.playhovering = false;
            __instance.playbtnobj.transform.Find("playBackground").GetComponent<Image>().color = GameTheme.themeColors.playButton.background;
            __instance.playbtnobj.transform.Find("playOutline").GetComponent<Image>().color = GameTheme.themeColors.playButton.outline;
            __instance.playbtnobj.transform.Find("playText").GetComponent<Image>().color = GameTheme.themeColors.playButton.text;
            __instance.playbtnobj.transform.Find("playShadow").GetComponent<Image>().color = GameTheme.themeColors.playButton.shadow;
            return false;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverBack))]
        [HarmonyPrefix]
        public static bool OnHoverBackBypassIfThemeNotDefault(LevelSelectController __instance)
        {
            if (GameTheme.isDefault) return true;
            __instance.hoversfx.Play();
            __instance.backbutton.gameObject.transform.Find("backBackground").GetComponent<Image>().color = GameTheme.themeColors.backButton.backgroundOver;
            __instance.backbutton.gameObject.transform.Find("backOutline").GetComponent<Image>().color = GameTheme.themeColors.backButton.outlineOver;
            __instance.backbutton.gameObject.transform.Find("backText").GetComponent<Image>().color = GameTheme.themeColors.backButton.textOver;
            __instance.backbutton.gameObject.transform.Find("backShadow").GetComponent<Image>().color = GameTheme.themeColors.backButton.shadowOver;
            return false;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverOutBack))]
        [HarmonyPrefix]
        public static bool OnHoverOutBackBypassIfThemeNotDefault(LevelSelectController __instance)
        {
            if (GameTheme.isDefault) return true;
            __instance.backbutton.gameObject.transform.Find("backBackground").GetComponent<Image>().color = GameTheme.themeColors.backButton.background;
            __instance.backbutton.gameObject.transform.Find("backOutline").GetComponent<Image>().color = GameTheme.themeColors.backButton.outline;
            __instance.backbutton.gameObject.transform.Find("backText").GetComponent<Image>().color = GameTheme.themeColors.backButton.text;
            __instance.backbutton.gameObject.transform.Find("backShadow").GetComponent<Image>().color = GameTheme.themeColors.backButton.shadow;
            return false;
        }

        public static void OnPointerEnterRandomEvent(LevelSelectController __instance)
        {
            __instance.hoversfx.Play();
            __instance.btnrandom.transform.Find("RandomBackground").GetComponent<Image>().color = GameTheme.themeColors.randomButton.backgroundOver;
            __instance.btnrandom.transform.Find("RandomOutline").GetComponent<Image>().color = GameTheme.themeColors.randomButton.outlineOver;
            __instance.btnrandom.transform.Find("RandomIcon").GetComponent<Image>().color = GameTheme.themeColors.randomButton.textOver;
            __instance.btnrandom.transform.Find("Text").GetComponent<Text>().color = GameTheme.themeColors.randomButton.textOver;
        }
        public static void OnPointerLeaveRandomEvent(LevelSelectController __instance)
        {
            __instance.btnrandom.transform.Find("RandomBackground").GetComponent<Image>().color = GameTheme.themeColors.randomButton.background;
            __instance.btnrandom.transform.Find("RandomOutline").GetComponent<Image>().color = GameTheme.themeColors.randomButton.outline;
            __instance.btnrandom.transform.Find("RandomIcon").GetComponent<Image>().color = GameTheme.themeColors.randomButton.text;
            __instance.btnrandom.transform.Find("Text").GetComponent<Text>().color = GameTheme.themeColors.randomButton.text;
        }

        #endregion

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPostfix]
        public static void OnAdvanceSongsPostFix(LevelSelectController __instance)
        {
            for (int i = 0; i < 10; i++)
            {
                if (!GameTheme.isDefault)
                    __instance.diffstars[i].color = Color.Lerp(GameTheme.themeColors.diffStar.gradientStart, GameTheme.themeColors.diffStar.gradientEnd, i / 9f);
                __instance.diffstars[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 19, 0);
                __instance.diffstars[i].maskable = true;
            }
            if (GameTheme.isDefault || songyear == null) return;
            songyear.text = __instance.songyear.text;
            songgenre.text = __instance.songgenre.text;
            songduration.text = __instance.songduration.text;
            songcomposer.text = __instance.songcomposer.text;
            songtempo.text = __instance.songtempo.text;
            songdesctext.text = __instance.songdesctext.text;
        }
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.sortTracks))]
        [HarmonyPostfix]
        public static void OnSortTracksPostFix(LevelSelectController __instance) => OnAdvanceSongsPostFix(__instance);

        [HarmonyPatch(typeof(WaveController), nameof(WaveController.Start))]
        [HarmonyPostfix]
        public static void WaveControllerFuckeryOverwrite(WaveController __instance)
        {
            if (GameTheme.isDefault) return;

            foreach (SpriteRenderer sr in __instance.wavesprites)
                sr.color = __instance.gameObject.name == "BGWave" ? GameTheme.themeColors.background.waves : GameTheme.themeColors.background.waves2;
        }

        [HarmonyPatch(typeof(HumanPuppetController), nameof(HumanPuppetController.setTextures))]
        [HarmonyPostfix]
        public static void Test(HumanPuppetController __instance)
        {
            if (option.CustomTrombColor.Value)
                __instance.trombmaterials[__instance.trombone_texture_index].SetColor("_Color", new Color(option.TrombRed.Value, option.TrombGreen.Value, option.TrombBlue.Value));
        }

        public static void OverwriteGameObjectSpriteAndColor(GameObject gameObject, string spriteName, Color spriteColor)
        {
            gameObject.GetComponent<Image>().sprite = AssetManager.GetSprite(spriteName);
            gameObject.GetComponent<Image>().color = spriteColor;
        }

        public class Options
        {
            public ConfigEntry<ThemeTypes> Theme { get; set; }
            public ConfigEntry<bool> CustomTrombColor { get; set; }
            public ConfigEntry<float> TrombRed { get; set; }
            public ConfigEntry<float> TrombGreen { get; set; }
            public ConfigEntry<float> TrombBlue { get; set; }
        }

        public enum ThemeTypes
        {
            Default,
            Day,
            Night,
            Custom,
            Aradigm,
            Citrus,
            Electro,
            Gloomhonk,
            Guardie,
            Jeff,
            JoeSickPack,
            Katiny,
            Lavender,
            NightRider,
            Perandus,
            Samuran,
            Random,
        }
    }
}
