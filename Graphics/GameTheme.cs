using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TootTally.Utils;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

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
                    text = new Color(0, 0, 0),
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
                    scoresBody = new Color(0, 0, 0),
                    rowEntry = new Color(0.12f, 0.12f, 0.12f),
                    yourRowEntry = new Color(0.35f, 0.35f, 0.35f, 0.65f),

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
                    background = new Color(0.2f, 0.2f, 0.2f),
                    backgroundOver = new Color(0.2f, 0.2f, 0.2f),
                    outline = new Color(0, 0, 0),
                    outlineOver = new Color(0, 0, 0),
                    text = new Color(1, 1, 1),
                    textOver = new Color(1, 1, 1),

                },
                backButton = new PlayBackButtonColors()
                {
                    background = new Color(0.2f, 0.2f, 0.2f),
                    backgroundOver = new Color(0.2f, 0.2f, 0.2f),
                    outline = new Color(0, 0, 0),
                    outlineOver = new Color(0, 0, 0),
                    text = new Color(1, 1, 1),
                    textOver = new Color(1, 1, 1),
                    shadow = Color.gray,
                    shadowOver = Color.gray
                },
                playButton = new PlayBackButtonColors()
                {
                    background = new Color(0.2f, 0.2f, 0.2f),
                    backgroundOver = new Color(0.2f, 0.2f, 0.2f),
                    outline = new Color(0, 0, 0),
                    outlineOver = new Color(0, 0, 0),
                    text = new Color(1, 1, 1),
                    textOver = new Color(1, 1, 1),
                    shadow = Color.gray,
                    shadowOver = Color.gray
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
                    background = new Color(0, 0, 0),
                    outline = new Color(0.2f, 0.2f, 0.2f),
                    shadow = new Color(0.2f, 0.2f, 0.2f)
                }
            };
        }

        public static void SetDayTheme()
        {
            themeColors = new ThemeColors()
            {
                leaderboard = new LeaderboardColors()
                {
                    panelBody = new Color(1, 1, 1),
                    scoresBody = new Color(0.9f, 0.9f, 0.9f),
                    rowEntry = new Color(1, 1, 1),
                    yourRowEntry = new Color(0.95f, 0.22f, 0.35f, 0.35f),
                    
                    headerText = new Color(0, 0, 0),
                    text = new Color(0, 0, 0),
                    textOutline = new Color(0.85f, 0.85f, 0.85f, .84f),

                    slider = new SliderColors()
                    {
                        background = new Color(1, 1, 1),
                        fill = new Color(0.95f, 0.22f, 0.35f),
                        handle = new Color(0.95f, 0.22f, 0.35f)
                        
                    },
                    
                    tabs = new ColorBlock()
                    {
                        normalColor = new Color(0, 0, 0),
                        pressedColor = new Color(.2f, .2f, .2f),
                        highlightedColor = new Color(.1f, .1f, .1f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.2f
                    }
                },
                notification = new NotificationColors()
                {
                    border = new Color(1, 1f, 1f, 0.75f),
                    background = new Color(0.9f, 0.9f, 0.9f, .95f),
                    defaultText = new Color(0, 0, 0),
                    textOutline = new Color(0.85f, 0.85f, 0.85f, .84f),
                    warningText = new Color(),
                    errorText = new Color()
                },
                replayButton = new ReplayButtonColors()
                {
                    text = new Color(0, 0, 0),
                    colors = new ColorBlock()
                    {
                        normalColor = new Color(1, 1, 1),
                        highlightedColor = new Color(.7f, .7f, .7f),
                        pressedColor = new Color(.42f, .42f, .42f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.2f
                    }
                },
                capsules = new CapsulesColors()
                {
                    year = new Color(.95f, .22f, .35f),
                    tempo = new Color(.074f, .188f, .203f),
                    genre = new Color(.22f, .69f, .75f),
                    composer = new Color(.95f, .65f, 0f),
                    description = new Color(.22f, .69f, .75f),
                    yearShadow = Color.black,
                    genreShadow = Color.black,
                    composerShadow = Color.black,
                    descriptionShadow = Color.black
                },
                randomButton = new RandomButtonColors()
                {
                    background = new Color(),
                    backgroundOver = new Color(),
                    outline = new Color(),
                    outlineOver = new Color(),
                    text = new Color(),
                    textOver = new Color()
                },
                backButton = new PlayBackButtonColors()
                {
                    background = new Color(),
                    backgroundOver = new Color(),
                    outline = new Color(),
                    outlineOver = new Color(),
                    text = new Color(),
                    textOver = new Color(),
                    shadow = new Color(),
                    shadowOver = new Color()
                },
                playButton = new PlayBackButtonColors()
                {
                    background = new Color(),
                    backgroundOver = new Color(),
                    outline = new Color(),
                    outlineOver = new Color(),
                    text = new Color(),
                    textOver = new Color(),
                    shadow = new Color(),
                    shadowOver = new Color()
                },
                songButton = new SongButtonColors()
                {
                    background = new Color(),
                    outline = new Color(),
                    outlineOver = new Color(),
                    shadow = new Color(),
                    textOver = new Color(),
                    text = new Color(),
                    selectedText = new Color(),
                    square = new Color()
                },
                scrollSpeedSlider = new ScrollSpeedSliderColors()
                {
                    background = new Color(),
                    text = new Color(),
                    handle = new Color(),
                    fill = new Color()
                },
                diffStar = new DiffStarColors()
                {
                    gradientStart = new Color(),
                    gradientEnd = new Color()
                },
                pointer = new PointerColors()
                {
                    background = new Color(),
                    outline = new Color(),
                    shadow = new Color()
                }
            };


        }

        public static void SetCustomTheme(string themeFileName)
        {
            if (!Directory.Exists(Paths.BepInExRootPath + "/Themes")) Directory.CreateDirectory(Paths.BepInExRootPath + "/Themes");
            if (File.Exists(Paths.BepInExRootPath + $"/Themes/{themeFileName}.json"))
            {
                string jsonFilePath = File.ReadAllText(Paths.BepInExRootPath + $"/Themes/{themeFileName}.json");
                SerializableClass.JsonThemeDeserializer deserializedTheme = JsonConvert.DeserializeObject<SerializableClass.JsonThemeDeserializer>(jsonFilePath);
                LoadTheme(deserializedTheme);
            }
            else
                SetDefaultTheme();

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
            ColorUtility.TryParseHtmlString(themeConfig.theme.notification.defaultText, out themeColors.notification.defaultText);
            ColorUtility.TryParseHtmlString(themeConfig.theme.notification.defaultText, out themeColors.notification.defaultText);
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
                }
            };
        }

        /*public static void SetElectroTheme()
        {
            SerializableClass.JsonThemeDeserializer themejson = new SerializableClass.JsonThemeDeserializer();
            themejson.theme.leaderboard.panelBody = "#FA1A8EFF";
            themejson.theme.leaderboard.scoresBody = "#DC0071FF";
            themejson.theme.leaderboard.rowEntry = "#FF61B1FF";
            themejson.theme.leaderboard.yourRowEntry = "#1732A1FF";
            themejson.theme.leaderboard.headerText = "#FFFFFFFF";
            themejson.theme.leaderboard.text = "#FFFFFFFF";
            themejson.theme.leaderboard.textOutline = "#000000FF";
            themejson.theme.leaderboard.slider.handle = "#DC0071FF";
            themejson.theme.leaderboard.slider.background = "#FFFFFFFF";
            themejson.theme.leaderboard.slider.fill = "#FFA6D3FF";
            themejson.theme.leaderboard.tabs.normal = "#FFFFFFFF";
            themejson.theme.leaderboard.tabs.pressed = "#1732A1FF";
            themejson.theme.leaderboard.tabs.highlighted = "#FF61B1FF";
            themejson.theme.scrollSpeedSlider.handle = "#DC0071FF";
            themejson.theme.scrollSpeedSlider.text = "#FFFFFFFF";
            themejson.theme.scrollSpeedSlider.background = "#FFFFFFFF";
            themejson.theme.scrollSpeedSlider.fill = "#FFA6D3FF";
            themejson.theme.notification.border = "#FA1A8EFF";
            themejson.theme.notification.background = "#000000FF";
            themejson.theme.notification.defaultText = "#FFFFFFFF";
            themejson.theme.notification.warningText = "#FFFFFFFF";
            themejson.theme.notification.errorText = "#FF0000FF";
            themejson.theme.notification.textOutline = "#000000FF";
            themejson.theme.replayButton.text = "#FFFFFFFF";
            themejson.theme.replayButton.normal = "#FA1A8EFF";
            themejson.theme.replayButton.pressed = "#FFFFFFFF";
            themejson.theme.replayButton.highlighted = "#FFFFFFFF";
            themejson.theme.capsules.year = "#FF61B1FF";
            themejson.theme.capsules.yearShadow = "#73003b";
            themejson.theme.capsules.composer = "#DC0071FF";
            themejson.theme.capsules.composerShadow = "#73003b";
            themejson.theme.capsules.genre = "#DC0071FF";
            themejson.theme.capsules.genreShadow = "#73003b";
            themejson.theme.capsules.description = "#FF61B1FF";
            themejson.theme.capsules.descriptionShadow = "#73003b";
            themejson.theme.capsules.tempo = "#1732A199";
            themejson.theme.randomButton.background = "#1732A1FF";
            themejson.theme.randomButton.backgroundOver = "#1732A1FF";
            themejson.theme.randomButton.outline = "#FA1A8EFF";
            themejson.theme.randomButton.outlineOver = "#000000FF";
            themejson.theme.randomButton.text = "#FFFFFFFF";
            themejson.theme.randomButton.textOver = "#FFFFFFFF";
            themejson.theme.backButton.background = "#1732A1FF";
            themejson.theme.backButton.backgroundOver = "#1732A1FF";
            themejson.theme.backButton.outline = "#FA1A8EFF";
            themejson.theme.backButton.outlineOver = "#000000FF";
            themejson.theme.backButton.text = "#FFFFFFFF";
            themejson.theme.backButton.textOver = "#FFFFFFFF";
            themejson.theme.backButton.shadow = "#1732A1FF";
            themejson.theme.backButton.shadowOver = "#1732A1FF";
            themejson.theme.playButton.background = "#1732A1FF";
            themejson.theme.playButton.backgroundOver = "#1732A1FF";
            themejson.theme.playButton.outline = "#FA1A8EFF";
            themejson.theme.playButton.outlineOver = "#000000FF";
            themejson.theme.playButton.text = "#FFFFFFFF";
            themejson.theme.playButton.textOver = "#FFFFFFFF";
            themejson.theme.playButton.shadow = "#1732A1FF";
            themejson.theme.playButton.shadowOver = "#1732A1FF";
            themejson.theme.songButton.background = "#1732A1FF";
            themejson.theme.songButton.text = "#FFFFFFFF";
            themejson.theme.songButton.textOver = "#FFFFFFFF";
            themejson.theme.songButton.outline = "#FA1A8EFF";
            themejson.theme.songButton.outlineOver = "#000000FF";
            themejson.theme.songButton.shadow = "#73003b";
            themejson.theme.songButton.square = "#FA1A8EFF";
            themejson.theme.diffStar.gradientStart = "#FFFFFFFF";
            themejson.theme.diffStar.gradientEnd = "#FA1A8EFF";
            themejson.theme.pointer.background = "#FFFFFFFF";
            themejson.theme.pointer.shadow = "#73003b";
            themejson.theme.pointer.outline = "#FA1A8EFF";
            LoadTheme(themejson);
        }*/

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

            }
        }

        #endregion
    }
}
