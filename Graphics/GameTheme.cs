using System;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using TootTally.Utils;
using TootTally.Utils.APIServices;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public static class GameTheme
    {
        public const string version = "0.0.1";
        public static bool isDefault;
        public static ThemeColors themeColors;

        public static void SetDefaultTheme()
        {
            isDefault = true;

            themeColors = new ThemeColors()
            {

                leaderboard = new LeaderboardColors()
                {
                    panelBody = new Color(0.95f, 0.22f, 0.35f),
                    scoresBody = new Color(0.06f, 0.06f, 0.06f),
                    rowEntry = new Color(0.10f, 0.10f, 0.10f),
                    yourRowEntry = new Color(0.65f, 0.65f, 0.65f, 0.25f),

                    headerText = new Color(0.95f, 0.22f, 0.35f),
                    text = new Color(1, 1, 1),
                    textOutline = new Color(0, 0, 0, .5f),

                    slider = new SliderColors()
                    {
                        background = new Color(0, 0, 0),
                        fill = new Color(1, 1, 1),
                        handle = new Color(1, 1, 1)
                    },

                    tabs = new ColorBlock()
                    {
                        normalColor = new Color(1, 1, 1),
                        pressedColor = new Color(1, 1, 0),
                        highlightedColor = new Color(.75f, .75f, .75f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.2f
                    }
                },
                notification = new NotificationColors()
                {
                    border = new Color(1, 0.3f, 0.5f, 0.75f),
                    background = new Color(0, 0, 0, .95f),
                    defaultText = new Color(1, 1, 1),
                    textOutline = new Color(0, 0, 0),
                    warningText = new Color(1, 1, 0),
                    errorText = new Color(1, 0, 0)
                },
                replayButton = new ReplayButtonColors()
                {
                    text = new Color(1, 1, 1),
                    colors = new ColorBlock()
                    {
                        normalColor = new Color(0.95f, 0.22f, 0.35f),
                        highlightedColor = new Color(0.77f, 0.18f, 0.29f),
                        pressedColor = new Color(1, 1, 0)
                    }

                },
                scrollSpeedSlider = new ScrollSpeedSliderColors()
                {
                    background = new Color(0, 0, 0),
                    text = new Color(0, 0, 0),
                    handle = new Color(1, 1, 0),
                    fill = new Color(0.95f, 0.22f, 0.35f)
                }
            };
        }

        public static void SetNightTheme()
        {
            themeColors = new ThemeColors()
            {

                leaderboard = new LeaderboardColors()
                {
                    panelBody = new Color(0.2f, 0.2f, 0.2f),
                    scoresBody = new Color(0.1f, 0.1f, 0.1f, 0.65f),
                    rowEntry = new Color(0.17f, 0.17f, 0.17f),
                    yourRowEntry = new Color(0f, 0f, 0f, 0.65f),

                    headerText = new Color(1, 1, 1),
                    text = new Color(1, 1, 1),
                    textOutline = new Color(0, 0, 0),

                    slider = new SliderColors()
                    {
                        background = new Color(0.15f, 0.15f, 0.15f),
                        fill = new Color(0.35f, 0.35f, 0.35f),
                        handle = new Color(0.35f, 0.35f, 0.35f)
                    },

                    tabs = new ColorBlock()
                    {
                        normalColor = new Color(1, 1, 1),
                        pressedColor = new Color(1, 1, 0),
                        highlightedColor = new Color(.75f, .75f, .75f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.2f
                    }
                },
                notification = new NotificationColors()
                {
                    border = new Color(0.2f, 0.2f, 0.2f, 0.75f),
                    background = new Color(0, 0, 0, .95f),
                    defaultText = new Color(1, 1, 1),
                    textOutline = new Color(0.2f, 0.2f, 0.2f),
                    warningText = new Color(1, 1, 0),
                    errorText = new Color(1, 0, 0)
                },
                replayButton = new ReplayButtonColors()
                {
                    text = new Color(1, 1, 1),
                    colors = new ColorBlock()
                    {
                        normalColor = new Color(0f, 0f, 0f),
                        highlightedColor = new Color(.2f, .2f, .2f),
                        pressedColor = new Color(.1f, .1f, .1f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.2f
                    }

                },
                capsules = new CapsulesColors()
                {
                    year = new Color(0, 0, 0),
                    tempo = new Color(0.2f, 0.2f, 0.2f, 0.45f),
                    genre = new Color(0.12f, 0.12f, 0.12f),
                    composer = new Color(0.12f, 0.12f, 0.12f),
                    description = new Color(0, 0, 0),
                    yearShadow = Color.gray,
                    genreShadow = Color.gray,
                    composerShadow = Color.gray,
                    descriptionShadow = Color.gray
                },
                randomButton = new RandomButtonColors()
                {
                    background = new Color(0f, 0f, 0f),
                    backgroundOver = new Color(0.2f, 0.2f, 0.2f),
                    outline = new Color(0.2f, 0.2f, 0.2f),
                    outlineOver = new Color(0, 0, 0),
                    text = new Color(.92f, .92f, .92f),
                    textOver = new Color(0.8f, .8f, .8f),

                },
                backButton = new PlayBackButtonColors()
                {
                    background = new Color(0f, 0f, 0f),
                    backgroundOver = new Color(0.2f, 0.2f, 0.2f),
                    outline = new Color(0.2f, 0.2f, 0.2f),
                    outlineOver = new Color(0, 0, 0),
                    text = new Color(.92f, .92f, .92f),
                    textOver = new Color(0.8f, .8f, .8f),
                    shadow = Color.gray,
                    shadowOver = Color.black
                },
                playButton = new PlayBackButtonColors()
                {
                    background = new Color(0f, 0f, 0f),
                    backgroundOver = new Color(0.2f, 0.2f, 0.2f),
                    outline = new Color(0.2f, 0.2f, 0.2f),
                    outlineOver = new Color(0, 0, 0),
                    text = new Color(.92f, .92f, .92f),
                    textOver = new Color(0.8f, .8f, .8f),
                    shadow = Color.gray,
                    shadowOver = Color.black
                },
                songButton = new SongButtonColors()
                {
                    background = new Color(0, 0, 0),
                    outline = new Color(0.12f, 0.12f, 0.12f),
                    outlineOver = new Color(0.2f, 0.2f, 0.2f),
                    selectedText = new Color(.35f, .35f, .35f),
                    shadow = Color.gray,
                    textOver = new Color(.92f, .92f, .92f),
                    text = new Color(.35f, .35f, .35f),
                    square = new Color(0, 0, 0)
                },
                scrollSpeedSlider = new ScrollSpeedSliderColors()
                {
                    background = new Color(0.15f, 0.15f, 0.15f),
                    text = new Color(1, 1, 1),
                    handle = new Color(0.35f, 0.35f, 0.35f),
                    fill = new Color(0.35f, 0.35f, 0.35f)
                },
                diffStar = new DiffStarColors()
                {
                    gradientStart = new Color(.2f, .2f, .2f),
                    gradientEnd = new Color(.7f, .7f, .7f)
                },
                pointer = new PointerColors()
                {
                    background = new Color(.92f, .92f, .92f),
                    outline = new Color(0.12f, 0.12f, 0.12f),
                    shadow = new Color(0f, 0f, 0f)
                },
                background = new BackgroundColors()
                {
                    waves = new Color(0f, 0f, 0f, .9f),
                    waves2 = new Color(.2f, .2f, .2f, .9f),
                    dots = new Color(0f, 0f, 0f, 1f),
                    dots2 = new Color(0f, 0f, 0f, 1f),
                    diamond = new Color(0f, 0f, 0f, .6f),
                    shape = new Color(0f, 0f, 0f, .75f),
                    background = new Color(.12f, .12f, .12f, .1f),
                },
                title = new TitleColors()
                {
                    songName = new Color(1f, 1f, 1f),
                    titleBar = new Color(0.2f, 0.2f, 0.2f),
                    title = new Color(1, 1, 1),
                    titleShadow = new Color(0.2f, 0.2f, 0.2f),
                }
            };
        }

        public static void SetDayTheme()
        {
            themeColors = new ThemeColors()
            {

                leaderboard = new LeaderboardColors()
                {
                    panelBody = new Color(0f, 0f, 0f),
                    scoresBody = new Color(0.9f, 0.9f, 0.9f, 0.9f),
                    rowEntry = new Color(0.75f, 0.75f, 0.75f),
                    yourRowEntry = new Color(1f, 1f, 1f, 0.65f),

                    headerText = new Color(0, 0, 0),
                    text = new Color(0, 0, 0),
                    textOutline = new Color(1, 1, 1),

                    slider = new SliderColors()
                    {
                        background = new Color(0.85f, 0.85f, 0.85f),
                        fill = new Color(0f, 0f, 0f),
                        handle = new Color(0f, 0f, 0f)
                    },

                    tabs = new ColorBlock()
                    {
                        normalColor = new Color(0, 0, 0),
                        pressedColor = new Color(1, 1, 0),
                        highlightedColor = new Color(.25f, .25f, .25f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.2f
                    }
                },
                notification = new NotificationColors()
                {
                    border = new Color(0.8f, 0.8f, 0.8f, 0.75f),
                    background = new Color(1, 1, 1, .95f),
                    defaultText = new Color(0, 0, 0),
                    textOutline = new Color(1f, 1f, 1f),
                    warningText = new Color(.6f, .6f, 0),
                    errorText = new Color(.6f, 0, 0)
                },
                replayButton = new ReplayButtonColors()
                {
                    text = new Color(0, 0, 0),
                    colors = new ColorBlock()
                    {
                        normalColor = new Color(1f, 1f, 1f),
                        highlightedColor = new Color(.8f, .8f, .8f),
                        pressedColor = new Color(.9f, .9f, .9f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.2f
                    }

                },
                capsules = new CapsulesColors()
                {
                    year = new Color(1, 1, 1),
                    tempo = new Color(0.8f, 0.8f, 0.8f, 0.75f),
                    genre = new Color(0.88f, 0.88f, 0.88f),
                    composer = new Color(0.88f, 0.88f, 0.88f),
                    description = new Color(1, 1, 1),
                    yearShadow = Color.black,
                    genreShadow = Color.black,
                    composerShadow = Color.black,
                    descriptionShadow = Color.black
                },
                randomButton = new RandomButtonColors()
                {
                    background = new Color(1f, 1f, 1f),
                    backgroundOver = new Color(0.8f, 0.8f, 0.8f),
                    outline = new Color(0.15f, 0.15f, 0.15f),
                    outlineOver = new Color(.25f, .25f, .25f),
                    text = new Color(.08f, .08f, .08f),
                    textOver = new Color(.2f, .2f, .2f),

                },
                backButton = new PlayBackButtonColors()
                {
                    background = new Color(1f, 1f, 1f),
                    backgroundOver = new Color(0.9f, 0.9f, 0.9f),
                    outline = new Color(0.15f, 0.15f, 0.15f),
                    outlineOver = new Color(.25f, .25f, .25f),
                    text = new Color(.08f, .08f, .08f),
                    textOver = new Color(.2f, .2f, .2f),
                    shadow = Color.black,
                    shadowOver = new Color(0.95f, 0.95f, 0.95f)
                },
                playButton = new PlayBackButtonColors()
                {
                    background = new Color(1f, 1f, 1f),
                    backgroundOver = new Color(0.9f, 0.9f, 0.9f),
                    outline = new Color(0.15f, 0.15f, 0.15f),
                    outlineOver = new Color(.25f, .25f, .25f),
                    text = new Color(.08f, .08f, .08f),
                    textOver = new Color(.2f, .2f, .2f),
                    shadow = Color.black,
                    shadowOver = new Color(0.95f, 0.95f, 0.95f)
                },
                songButton = new SongButtonColors()
                {
                    background = new Color(1, 1, 1),
                    outline = new Color(0.8f, 0.8f, 0.8f),
                    outlineOver = new Color(0.35f, 0.35f, 0.35f),
                    selectedText = new Color(0f, 0f, 0f),
                    shadow = new Color(0.05f, 0.05f, 0.05f),
                    textOver = new Color(.08f, .08f, .08f),
                    text = new Color(.35f, .35f, .35f),
                    square = new Color(.35f, .35f, .35f)
                },
                scrollSpeedSlider = new ScrollSpeedSliderColors()
                {
                    background = new Color(0.85f, 0.85f, 0.85f),
                    text = new Color(1, 1, 1),
                    handle = new Color(0f, 0f, 0f),
                    fill = new Color(0f, 0f, 0f)
                },
                diffStar = new DiffStarColors()
                {
                    gradientStart = new Color(.8f, .8f, .8f),
                    gradientEnd = new Color(.3f, .3f, .3f)
                },
                pointer = new PointerColors()
                {
                    background = new Color(.88f, .88f, .88f),
                    outline = new Color(0.08f, 0.08f, 0.08f),
                    shadow = Color.black
                },
                background = new BackgroundColors()
                {
                    waves = new Color(0f, 0f, 0f, .9f),
                    waves2 = new Color(.6f, .6f, .6f, .9f),
                    dots = new Color(0.9f, 0.9f, 0.9f, 1f),
                    dots2 = new Color(0.9f, 0.9f, 0.9f, 1f),
                    diamond = new Color(0.9f, 0.9f, 0.9f, .6f),
                    shape = new Color(0.9f, 0.9f, 0.9f, .45f),
                    background = new Color(.77f, .77f, .77f, .1f),
                },
                title = new TitleColors()
                {
                    songName = new Color(0, 0, 0),
                    titleBar = new Color(0.9f, 0.9f, 0.9f, 0.75f),
                    title = new Color(0, 0, 0),
                    titleShadow = new Color(0.9f, 0.9f, 0.9f)
                }

            };

        }

        public static void SetCustomTheme(string themeFileName)
        {           
            if (File.Exists(Paths.BepInExRootPath + $"/Themes/{themeFileName}.json"))
            {
                string jsonFilePath = File.ReadAllText(Paths.BepInExRootPath + $"/Themes/{themeFileName}.json");

                try
                {
                    SerializableClass.JsonThemeDeserializer deserializedTheme = JsonConvert.DeserializeObject<SerializableClass.JsonThemeDeserializer>(jsonFilePath);
                    LoadTheme(deserializedTheme);
                }
                catch (Exception ex)
                {
                    TootTallyLogger.LogError(ex.ToString());
                    TootTallyLogger.LogError("Corrupted theme: " + themeFileName);
                    TootTallyLogger.LogError("Loading default theme...");
                    SetDefaultTheme();
                }
            }
            else
            {
                TootTallyLogger.LogError("Missing theme: " + themeFileName);
                SetDefaultTheme();
            }

        }

        public static void LoadTheme(SerializableClass.JsonThemeDeserializer themeConfig)
        {
            themeColors = new ThemeColors();
            themeColors.InitializeEmpty();

            Color normalColor, pressedColor, highlightedColor;

            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.panelBody, out themeColors.leaderboard.panelBody);
            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.scoresBody, out themeColors.leaderboard.scoresBody);
            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.rowEntry, out themeColors.leaderboard.rowEntry);
            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.yourRowEntry, out themeColors.leaderboard.yourRowEntry);

            ColorUtility.TryParseHtmlString(themeConfig.theme.scrollSpeedSlider.background, out themeColors.scrollSpeedSlider.background);
            ColorUtility.TryParseHtmlString(themeConfig.theme.scrollSpeedSlider.text, out themeColors.scrollSpeedSlider.text);
            ColorUtility.TryParseHtmlString(themeConfig.theme.scrollSpeedSlider.handle, out themeColors.scrollSpeedSlider.handle);
            ColorUtility.TryParseHtmlString(themeConfig.theme.scrollSpeedSlider.fill, out themeColors.scrollSpeedSlider.fill);

            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.slider.background, out themeColors.leaderboard.slider.background);
            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.slider.fill, out themeColors.leaderboard.slider.fill);
            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.slider.handle, out themeColors.leaderboard.slider.handle);

            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.headerText, out themeColors.leaderboard.headerText);
            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.text, out themeColors.leaderboard.text);
            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.textOutline, out themeColors.leaderboard.textOutline);

            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.tabs.normal, out normalColor);
            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.tabs.pressed, out pressedColor);
            ColorUtility.TryParseHtmlString(themeConfig.theme.leaderboard.tabs.highlighted, out highlightedColor);

            themeColors.leaderboard.tabs = new ColorBlock()
            {
                normalColor = normalColor,
                pressedColor = pressedColor,
                highlightedColor = highlightedColor,
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            ColorUtility.TryParseHtmlString(themeConfig.theme.notification.border, out themeColors.notification.border);
            ColorUtility.TryParseHtmlString(themeConfig.theme.notification.background, out themeColors.notification.background);
            ColorUtility.TryParseHtmlString(themeConfig.theme.notification.defaultText, out themeColors.notification.defaultText);
            ColorUtility.TryParseHtmlString(themeConfig.theme.notification.warningText, out themeColors.notification.warningText);
            ColorUtility.TryParseHtmlString(themeConfig.theme.notification.errorText, out themeColors.notification.errorText);
            ColorUtility.TryParseHtmlString(themeConfig.theme.notification.textOutline, out themeColors.notification.textOutline);

            ColorUtility.TryParseHtmlString(themeConfig.theme.replayButton.text, out themeColors.replayButton.text);
            ColorUtility.TryParseHtmlString(themeConfig.theme.replayButton.normal, out normalColor);
            ColorUtility.TryParseHtmlString(themeConfig.theme.replayButton.pressed, out pressedColor);
            ColorUtility.TryParseHtmlString(themeConfig.theme.replayButton.highlighted, out highlightedColor);

            themeColors.replayButton.colors = new ColorBlock()
            {
                normalColor = normalColor,
                pressedColor = pressedColor,
                highlightedColor = highlightedColor,
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            ColorUtility.TryParseHtmlString(themeConfig.theme.capsules.year, out themeColors.capsules.year);
            ColorUtility.TryParseHtmlString(themeConfig.theme.capsules.yearShadow, out themeColors.capsules.yearShadow);
            ColorUtility.TryParseHtmlString(themeConfig.theme.capsules.composer, out themeColors.capsules.composer);
            ColorUtility.TryParseHtmlString(themeConfig.theme.capsules.composerShadow, out themeColors.capsules.composerShadow);
            ColorUtility.TryParseHtmlString(themeConfig.theme.capsules.genre, out themeColors.capsules.genre);
            ColorUtility.TryParseHtmlString(themeConfig.theme.capsules.genreShadow, out themeColors.capsules.genreShadow);
            ColorUtility.TryParseHtmlString(themeConfig.theme.capsules.description, out themeColors.capsules.description);
            ColorUtility.TryParseHtmlString(themeConfig.theme.capsules.descriptionShadow, out themeColors.capsules.descriptionShadow);
            ColorUtility.TryParseHtmlString(themeConfig.theme.capsules.tempo, out themeColors.capsules.tempo);

            ColorUtility.TryParseHtmlString(themeConfig.theme.randomButton.background, out themeColors.randomButton.background);
            ColorUtility.TryParseHtmlString(themeConfig.theme.randomButton.backgroundOver, out themeColors.randomButton.backgroundOver);
            ColorUtility.TryParseHtmlString(themeConfig.theme.randomButton.outline, out themeColors.randomButton.outline);
            ColorUtility.TryParseHtmlString(themeConfig.theme.randomButton.outlineOver, out themeColors.randomButton.outlineOver);
            ColorUtility.TryParseHtmlString(themeConfig.theme.randomButton.text, out themeColors.randomButton.text);
            ColorUtility.TryParseHtmlString(themeConfig.theme.randomButton.textOver, out themeColors.randomButton.textOver);

            ColorUtility.TryParseHtmlString(themeConfig.theme.backButton.background, out themeColors.backButton.background);
            ColorUtility.TryParseHtmlString(themeConfig.theme.backButton.backgroundOver, out themeColors.backButton.backgroundOver);
            ColorUtility.TryParseHtmlString(themeConfig.theme.backButton.outline, out themeColors.backButton.outline);
            ColorUtility.TryParseHtmlString(themeConfig.theme.backButton.outlineOver, out themeColors.backButton.outlineOver);
            ColorUtility.TryParseHtmlString(themeConfig.theme.backButton.text, out themeColors.backButton.text);
            ColorUtility.TryParseHtmlString(themeConfig.theme.backButton.textOver, out themeColors.backButton.textOver);
            ColorUtility.TryParseHtmlString(themeConfig.theme.backButton.shadow, out themeColors.backButton.shadow);
            ColorUtility.TryParseHtmlString(themeConfig.theme.backButton.shadowOver, out themeColors.backButton.shadowOver);

            ColorUtility.TryParseHtmlString(themeConfig.theme.playButton.background, out themeColors.playButton.background);
            ColorUtility.TryParseHtmlString(themeConfig.theme.playButton.backgroundOver, out themeColors.playButton.backgroundOver);
            ColorUtility.TryParseHtmlString(themeConfig.theme.playButton.outline, out themeColors.playButton.outline);
            ColorUtility.TryParseHtmlString(themeConfig.theme.playButton.outlineOver, out themeColors.playButton.outlineOver);
            ColorUtility.TryParseHtmlString(themeConfig.theme.playButton.text, out themeColors.playButton.text);
            ColorUtility.TryParseHtmlString(themeConfig.theme.playButton.textOver, out themeColors.playButton.textOver);
            ColorUtility.TryParseHtmlString(themeConfig.theme.playButton.shadow, out themeColors.playButton.shadow);
            ColorUtility.TryParseHtmlString(themeConfig.theme.playButton.shadowOver, out themeColors.playButton.shadowOver);

            ColorUtility.TryParseHtmlString(themeConfig.theme.songButton.background, out themeColors.songButton.background);
            ColorUtility.TryParseHtmlString(themeConfig.theme.songButton.outline, out themeColors.songButton.outline);
            ColorUtility.TryParseHtmlString(themeConfig.theme.songButton.outlineOver, out themeColors.songButton.outlineOver);
            ColorUtility.TryParseHtmlString(themeConfig.theme.songButton.shadow, out themeColors.songButton.shadow);
            ColorUtility.TryParseHtmlString(themeConfig.theme.songButton.text, out themeColors.songButton.text);
            ColorUtility.TryParseHtmlString(themeConfig.theme.songButton.textOver, out themeColors.songButton.textOver);
            ColorUtility.TryParseHtmlString(themeConfig.theme.songButton.square, out themeColors.songButton.square);

            ColorUtility.TryParseHtmlString(themeConfig.theme.diffStar.gradientStart, out themeColors.diffStar.gradientStart);
            ColorUtility.TryParseHtmlString(themeConfig.theme.diffStar.gradientEnd, out themeColors.diffStar.gradientEnd);

            ColorUtility.TryParseHtmlString(themeConfig.theme.pointer.background, out themeColors.pointer.background);
            ColorUtility.TryParseHtmlString(themeConfig.theme.pointer.shadow, out themeColors.pointer.shadow);
            ColorUtility.TryParseHtmlString(themeConfig.theme.pointer.outline, out themeColors.pointer.outline);

            ColorUtility.TryParseHtmlString(themeConfig.theme.background.waves, out themeColors.background.waves);
            ColorUtility.TryParseHtmlString(themeConfig.theme.background.waves2, out themeColors.background.waves2);
            ColorUtility.TryParseHtmlString(themeConfig.theme.background.dots, out themeColors.background.dots);
            ColorUtility.TryParseHtmlString(themeConfig.theme.background.dots2, out themeColors.background.dots2);
            ColorUtility.TryParseHtmlString(themeConfig.theme.background.shape, out themeColors.background.shape);
            ColorUtility.TryParseHtmlString(themeConfig.theme.background.diamond, out themeColors.background.diamond);
            ColorUtility.TryParseHtmlString(themeConfig.theme.background.background, out themeColors.background.background);

            ColorUtility.TryParseHtmlString(themeConfig.theme.title.songName, out themeColors.title.songName);
            ColorUtility.TryParseHtmlString(themeConfig.theme.title.titleBar, out themeColors.title.titleBar);
            ColorUtility.TryParseHtmlString(themeConfig.theme.title.title, out themeColors.title.title);
            ColorUtility.TryParseHtmlString(themeConfig.theme.title.titleShadow, out themeColors.title.titleShadow);
        }

        public static void SetRandomTheme()
        {
            System.Random rdm = new System.Random(System.DateTime.Now.Millisecond);

            themeColors = new ThemeColors()
            {
                leaderboard = new LeaderboardColors()
                {
                    panelBody = GetRandomColor(rdm, 1),
                    scoresBody = GetRandomColor(rdm, 1),
                    rowEntry = GetRandomColor(rdm, 1),
                    yourRowEntry = GetRandomColor(rdm, 0.35f),

                    headerText = GetRandomColor(rdm, 1),
                    text = GetRandomColor(rdm, 1),
                    textOutline = GetRandomColor(rdm, 0.75f),

                    slider = new SliderColors()
                    {
                        background = GetRandomColor(rdm, 1),
                        fill = GetRandomColor(rdm, 1),
                        handle = GetRandomColor(rdm, 1)
                    },

                    tabs = new ColorBlock()
                    {
                        normalColor = GetRandomColor(rdm, 1),
                        pressedColor = GetRandomColor(rdm, 1),
                        highlightedColor = GetRandomColor(rdm, 1),
                        colorMultiplier = 1f,
                        fadeDuration = 0.2f
                    }
                },
                notification = new NotificationColors()
                {
                    border = GetRandomColor(rdm, .75f),
                    background = GetRandomColor(rdm, .84f),
                    defaultText = GetRandomColor(rdm, 1),
                    textOutline = GetRandomColor(rdm, 0.84f),
                    warningText = GetRandomColor(rdm, 1),
                    errorText = GetRandomColor(rdm, 1)
                },
                replayButton = new ReplayButtonColors()
                {
                    text = GetRandomColor(rdm, 1),
                    colors = new ColorBlock()
                    {
                        normalColor = GetRandomColor(rdm, 1),
                        highlightedColor = GetRandomColor(rdm, 1),
                        pressedColor = GetRandomColor(rdm, 1)
                    }
                },
                capsules = new CapsulesColors()
                {
                    year = GetRandomColor(rdm, 1),
                    tempo = GetRandomColor(rdm, 1),
                    genre = GetRandomColor(rdm, 1),
                    composer = GetRandomColor(rdm, 1),
                    description = GetRandomColor(rdm, 1),
                    yearShadow = GetRandomColor(rdm, 1),
                    genreShadow = GetRandomColor(rdm, 1),
                    composerShadow = GetRandomColor(rdm, 1),
                    descriptionShadow = GetRandomColor(rdm, 1)
                },
                randomButton = new RandomButtonColors()
                {
                    background = GetRandomColor(rdm, 1),
                    backgroundOver = GetRandomColor(rdm, 1),
                    outline = GetRandomColor(rdm, 1),
                    outlineOver = GetRandomColor(rdm, 1),
                    text = GetRandomColor(rdm, 1),
                    textOver = GetRandomColor(rdm, 1),
                },
                backButton = new PlayBackButtonColors()
                {
                    background = GetRandomColor(rdm, 1),
                    backgroundOver = GetRandomColor(rdm, 1),
                    outline = GetRandomColor(rdm, 1),
                    outlineOver = GetRandomColor(rdm, 1),
                    text = GetRandomColor(rdm, 1),
                    textOver = GetRandomColor(rdm, 1),
                    shadow = GetRandomColor(rdm, 1),
                    shadowOver = GetRandomColor(rdm, 1)
                },
                playButton = new PlayBackButtonColors()
                {
                    background = GetRandomColor(rdm, 1),
                    backgroundOver = GetRandomColor(rdm, 1),
                    outline = GetRandomColor(rdm, 1),
                    outlineOver = GetRandomColor(rdm, 1),
                    text = GetRandomColor(rdm, 1),
                    textOver = GetRandomColor(rdm, 1),
                    shadow = GetRandomColor(rdm, 1),
                    shadowOver = GetRandomColor(rdm, 1)
                },
                songButton = new SongButtonColors()
                {
                    background = GetRandomColor(rdm, 1),
                    outline = GetRandomColor(rdm, 1),
                    outlineOver = GetRandomColor(rdm, 1),
                    shadow = GetRandomColor(rdm, 1),
                    textOver = GetRandomColor(rdm, 1),
                    text = GetRandomColor(rdm, 1),
                    selectedText = GetRandomColor(rdm, 1),
                    square = GetRandomColor(rdm, 1)
                },
                scrollSpeedSlider = new ScrollSpeedSliderColors()
                {
                    background = GetRandomColor(rdm, 1),
                    text = GetRandomColor(rdm, 1),
                    handle = GetRandomColor(rdm, 1),
                    fill = GetRandomColor(rdm, 1)
                },
                diffStar = new DiffStarColors()
                {
                    gradientStart = GetRandomColor(rdm, 1),
                    gradientEnd = GetRandomColor(rdm, 1)
                },
                pointer = new PointerColors()
                {
                    background = GetRandomColor(rdm, 1),
                    shadow = GetRandomColor(rdm, 1),
                    outline = GetRandomColor(rdm, 1)
                },
                background = new BackgroundColors()
                {
                    waves = GetRandomColor(rdm, .5f),
                    waves2 = GetRandomColor(rdm, .5f),
                    dots = GetRandomColor(rdm, .5f),
                    dots2 = GetRandomColor(rdm, .5f),
                    shape = GetRandomColor(rdm, .5f),
                    diamond = GetRandomColor(rdm, .5f),
                    background = GetRandomColor(rdm, .5f)
                },
                title = new TitleColors()
                {
                    songName = GetRandomColor(rdm, 1),
                    title = GetRandomColor(rdm, 1),
                    titleBar = GetRandomColor(rdm, 1),
                    titleShadow = GetRandomColor(rdm, 1)
                }
            };
        }

        private static Color GetRandomColor(System.Random rdm, float alpha)
        {
            return new Color((float)rdm.NextDouble(), (float)rdm.NextDouble(), (float)rdm.NextDouble(), alpha);
        }

        #region ColorClasses

        public class CapsulesColors
        {
            public Color year;
            public Color yearShadow;
            public Color composer;
            public Color composerShadow;
            public Color genre;
            public Color genreShadow;
            public Color description;
            public Color descriptionShadow;
            public Color tempo;
        }

        public class DiffStarColors
        {
            public Color gradientStart;
            public Color gradientEnd;
        }

        public class LeaderboardColors
        {
            public Color panelBody;
            public Color scoresBody;
            public Color rowEntry;
            public Color yourRowEntry;
            public Color headerText;
            public Color text;
            public Color textOutline;
            public SliderColors slider;
            public ColorBlock tabs;
        }

        public class NotificationColors
        {
            public Color border;
            public Color background;
            public Color defaultText;
            public Color warningText;
            public Color errorText;
            public Color textOutline;
        }

        public class PlayBackButtonColors
        {
            public Color background;
            public Color backgroundOver;
            public Color outline;
            public Color outlineOver;
            public Color text;
            public Color textOver;
            public Color shadow;
            public Color shadowOver;
        }

        public class RandomButtonColors
        {
            public Color background;
            public Color backgroundOver;
            public Color outline;
            public Color outlineOver;
            public Color text;
            public Color textOver;
            public Color shadow;
            public Color shadowOver;
        }

        public class ReplayButtonColors
        {
            public Color text;
            public ColorBlock colors;
        }

        public class ScrollSpeedSliderColors
        {
            public Color handle;
            public Color text;
            public Color background;
            public Color fill;
        }

        public class SliderColors
        {
            public Color handle;
            public Color background;
            public Color fill;
        }

        public class SongButtonColors
        {
            public Color background;
            public Color text;
            public Color textOver;
            public Color selectedText;
            public Color outline;
            public Color outlineOver;
            public Color shadow;
            public Color square;
        }

        public class PointerColors
        {
            public Color background;
            public Color shadow;
            public Color outline;

        }

        public class BackgroundColors
        {
            public Color waves;
            public Color waves2;
            public Color dots;
            public Color dots2;
            public Color shape;
            public Color diamond;
            public Color background;
        }

        public class TitleColors
        {
            public Color songName;
            public Color titleBar;
            public Color title;
            public Color titleShadow;
            public Color outline;
        }

        public class ThemeColors
        {
            public LeaderboardColors leaderboard;
            public ScrollSpeedSliderColors scrollSpeedSlider;
            public NotificationColors notification;
            public ReplayButtonColors replayButton;
            public CapsulesColors capsules;
            public RandomButtonColors randomButton;
            public PlayBackButtonColors backButton;
            public PlayBackButtonColors playButton;
            public SongButtonColors songButton;
            public DiffStarColors diffStar;
            public PointerColors pointer;
            public BackgroundColors background;
            public TitleColors title;

            public void InitializeEmpty()
            {
                leaderboard = new LeaderboardColors()
                {
                    slider = new SliderColors()
                };
                scrollSpeedSlider = new ScrollSpeedSliderColors();
                notification = new NotificationColors();
                replayButton = new ReplayButtonColors();
                capsules = new CapsulesColors();
                randomButton = new RandomButtonColors();
                backButton = new PlayBackButtonColors();
                playButton = new PlayBackButtonColors();
                songButton = new SongButtonColors();
                diffStar = new DiffStarColors();
                pointer = new PointerColors();
                backButton = new PlayBackButtonColors();
                background = new BackgroundColors();
                title = new TitleColors();
            }
        }

        #endregion
    }
}
