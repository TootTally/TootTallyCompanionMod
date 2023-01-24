using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TootTally.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public static class Theme
    {
        public const string version = "0.0.1";
        public static bool isDefault;

        public static Color panelBodyColor, scoresbodyColor, rowEntryImageColor, rowEntryImageYouColor;
        public static Color leaderboardVerticalSliderHandleColor, leaderboardVerticalSliderBackgroundColor, leaderboardVerticalSliderFillColor;
        public static Color leaderboardHeaderTextColor, leaderboardTextColor, leaderboardTextOutlineColor;
        public static ColorBlock tabsColors;

        public static Color scrollSpeedSliderTextColor, scrollSpeedSliderBackgroundColor, scrollSpeedSliderFillColor, scrollSpeedSliderHandleColor;

        public static Color notificationBorderColor, notificationBackgroundColor, notificationTextOutlineColor;
        public static Color defaultNotifColor, warningNotifColor, errorNotifColor;

        public static Color replayButtonTextColor;
        public static ColorBlock replayButtonColors;

        public static Color capsuleYearColor, capsuleYearShadowColor, capsuleComposerColor, capsuleComposerShadowColor, capsuleGenreColor, capsuleGenreShadowColor, capsuleDescColor, capsuleDescShadowColor, capsuleTempoColor;

        public static Color randomBtnOutlineColor, randomBtnBackgroundColor, randomBtnTextColor;
        public static ColorBlock randomBtnIconColors;

        public static Color backBtnOutlineColor, backBtnBackgroundColor, backBtnTextColor, backBtnShadowColor;
        public static Color playBtnOutlineColor, playBtnBackgroundColor, playBtnTextColor, playBtnShadowColor;

        public static Color songButtonBackgroundColor, songButtonTextColor, songButtonTextOverColor, songButtonOutlineColor, songButtonShadowColor, songButtonOutlineOverColor, songButtonSquareColor;

        public static Color diffStarStartColor, diffStarEndColor;

        public static void SetDefaultTheme()
        {
            isDefault = true;
            panelBodyColor = new Color(0.95f, 0.22f, 0.35f);
            scoresbodyColor = new Color(0.06f, 0.06f, 0.06f);
            rowEntryImageColor = new Color(0.10f, 0.10f, 0.10f);
            rowEntryImageYouColor = new Color(0.65f, 0.65f, 0.65f, 0.25f);

            leaderboardVerticalSliderBackgroundColor = new Color(0, 0, 0);
            leaderboardVerticalSliderFillColor = new Color(1, 1, 1);
            leaderboardVerticalSliderHandleColor = new Color(1, 1, 1);

            leaderboardHeaderTextColor = new Color(0.95f, 0.22f, 0.35f);
            leaderboardTextColor = new Color(1, 1, 1);
            leaderboardTextOutlineColor = new Color(0, 0, 0, .5f);

            tabsColors.normalColor = new Color(1, 1, 1);
            tabsColors.pressedColor = new Color(1, 1, 0);
            tabsColors.highlightedColor = new Color(.75f, .75f, .75f);
            tabsColors.colorMultiplier = 1f;
            tabsColors.fadeDuration = 0.1f;

            notificationBorderColor = new Color(1, 0.3f, 0.5f, 0.75f);
            notificationBackgroundColor = new Color(0, 0, 0, .95f);
            defaultNotifColor = new Color(1, 1, 1);
            notificationTextOutlineColor = new Color(0, 0, 0);
            warningNotifColor = new Color(1, 1, 0);
            errorNotifColor = new Color(1, 0, 0);

            replayButtonTextColor = new Color(0, 0, 0);
            replayButtonColors.normalColor = new Color(0.95f, 0.22f, 0.35f);
            replayButtonColors.highlightedColor = new Color(0.77f, 0.18f, 0.29f);
            replayButtonColors.pressedColor = new Color(1, 1, 0);
            replayButtonColors.selectedColor = new Color(0.95f, 0.22f, 0.35f);

            scrollSpeedSliderBackgroundColor = new Color(0, 0, 0);
            scrollSpeedSliderTextColor = new Color(0, 0, 0);
            scrollSpeedSliderHandleColor = new Color(1, 1, 0);
            scrollSpeedSliderFillColor = new Color(0.95f, 0.22f, 0.35f);
        }

        public static void SetNightTheme()
        {
            panelBodyColor = new Color(0.2f, 0.2f, 0.2f);
            scoresbodyColor = new Color(0, 0, 0);
            rowEntryImageColor = new Color(0.12f, 0.12f, 0.12f);
            rowEntryImageYouColor = new Color(0.35f, 0.35f, 0.35f, 0.65f);

            leaderboardVerticalSliderBackgroundColor = new Color(0.15f, 0.15f, 0.15f);
            leaderboardVerticalSliderFillColor = new Color(0.35f, 0.35f, 0.35f);
            leaderboardVerticalSliderHandleColor = new Color(0.35f, 0.35f, 0.35f);

            scrollSpeedSliderBackgroundColor = new Color(0.15f, 0.15f, 0.15f);
            scrollSpeedSliderTextColor = new Color(1, 1, 1);
            scrollSpeedSliderHandleColor = new Color(0.35f, 0.35f, 0.35f);
            scrollSpeedSliderFillColor = new Color(0.35f, 0.35f, 0.35f);

            leaderboardHeaderTextColor = new Color(1, 1, 1);
            leaderboardTextColor = new Color(1, 1, 1);
            leaderboardTextOutlineColor = new Color(0, 0, 0);

            tabsColors.normalColor = new Color(1, 1, 1);
            tabsColors.pressedColor = new Color(1, 1, 0);
            tabsColors.highlightedColor = new Color(.75f, .75f, .75f);
            tabsColors.selectedColor = new Color(1, 1, 1);
            tabsColors.colorMultiplier = 1f;
            tabsColors.fadeDuration = 0.1f;

            notificationBorderColor = new Color(0.2f, 0.2f, 0.2f, 0.75f);
            notificationBackgroundColor = new Color(0, 0, 0, .95f);
            defaultNotifColor = new Color(1, 1, 1);
            notificationTextOutlineColor = new Color(0.2f, 0.2f, 0.2f);

            replayButtonTextColor = new Color(1, 1, 1);
            replayButtonColors.normalColor = new Color(0f, 0f, 0f);
            replayButtonColors.highlightedColor = new Color(.2f, .2f, .2f);
            replayButtonColors.pressedColor = new Color(.1f, .1f, .1f);
            replayButtonColors.selectedColor = new Color(0f, 0f, 0f);

            capsuleYearColor = new Color(0, 0, 0);
            capsuleTempoColor = new Color(0.2f, 0.2f, 0.2f, 0.45f);
            capsuleGenreColor = new Color(0.12f, 0.12f, 0.12f);
            capsuleComposerColor = new Color(0.12f, 0.12f, 0.12f);
            capsuleDescColor = new Color(0, 0, 0);
            capsuleDescShadowColor = capsuleGenreShadowColor = capsuleYearShadowColor = capsuleComposerShadowColor = Color.gray;

            randomBtnBackgroundColor = new Color(0.2f, 0.2f, 0.2f);
            randomBtnOutlineColor = new Color(0, 0, 0);
            randomBtnTextColor = new Color(1, 1, 1);
            randomBtnIconColors = tabsColors;

            backBtnBackgroundColor = new Color(0.2f, 0.2f, 0.2f);
            backBtnOutlineColor = new Color(0,0,0);
            backBtnTextColor = new Color(1, 1, 1);
            backBtnShadowColor = Color.gray;

            playBtnBackgroundColor = new Color(0.2f, 0.2f, 0.2f);
            playBtnOutlineColor = new Color(0, 0, 0);
            playBtnTextColor = new Color(1, 1, 1);
            playBtnShadowColor = Color.gray;

            songButtonBackgroundColor = new Color(0, 0, 0);
            songButtonOutlineColor = new Color(0.12f, 0.12f, 0.12f);
            songButtonOutlineOverColor = new Color(0.2f, 0.2f, 0.2f);
            songButtonShadowColor = Color.gray;
            songButtonTextOverColor = new Color(.92f, .92f, .92f);
            songButtonTextColor = new Color(.35f, .35f, .35f);
            songButtonSquareColor = new Color(0, 0, 0);

            diffStarStartColor = new Color(.2f, .2f, .2f);
            diffStarEndColor = new Color(.7f, .7f, .7f);
        }

        public static void SetDayTheme()
        {
            panelBodyColor = new Color(1, 1, 1);
            scoresbodyColor = new Color(0.9f, 0.9f, 0.9f);
            rowEntryImageColor = new Color(1, 1, 1);
            rowEntryImageYouColor = new Color(0.95f, 0.22f, 0.35f, 0.35f);

            leaderboardVerticalSliderBackgroundColor = new Color(1, 1, 1);
            leaderboardVerticalSliderFillColor = new Color(0.95f, 0.22f, 0.35f);
            leaderboardVerticalSliderHandleColor = new Color(0.95f, 0.22f, 0.35f);

            leaderboardHeaderTextColor = new Color(0, 0, 0);
            leaderboardTextColor = new Color(0, 0, 0);
            leaderboardTextOutlineColor = new Color(0.85f, 0.85f, 0.85f, .84f);

            tabsColors.normalColor = new Color(0, 0, 0);
            tabsColors.pressedColor = new Color(.2f, .2f, .2f);
            tabsColors.highlightedColor = new Color(.1f, .1f, .1f);
            tabsColors.selectedColor = new Color(0, 0, 0);
            tabsColors.colorMultiplier = 1f;
            tabsColors.fadeDuration = 0.1f;

            notificationBorderColor = new Color(1, 1f, 1f, 0.75f);
            notificationBackgroundColor = new Color(0.9f, 0.9f, 0.9f, .95f);
            defaultNotifColor = new Color(0, 0, 0);
            notificationTextOutlineColor = new Color(0.85f, 0.85f, 0.85f, .84f);

            replayButtonTextColor = new Color(0, 0, 0);
            replayButtonColors.normalColor = new Color(1, 1, 1);
            replayButtonColors.highlightedColor = new Color(.7f, .7f, .7f);
            replayButtonColors.pressedColor = new Color(.42f, .42f, .42f);
            replayButtonColors.selectedColor = new Color(1, 1, 1);

            capsuleYearColor = new Color(.95f, .22f, .35f);
            capsuleTempoColor = new Color(.074f, .188f, .203f);
            capsuleGenreColor = new Color(.22f, .69f, .75f);
            capsuleComposerColor = new Color(.95f, .65f, 0f);
            capsuleDescColor = new Color(.22f, .69f, .75f);
            capsuleDescShadowColor = capsuleGenreShadowColor = capsuleYearShadowColor = Color.black;
        }

        public static void SetCustomTheme()
        {
            string jsonFile = File.ReadAllText(Paths.BepInExRootPath + "/Themes/ElectroTheme.json");
            Plugin.LogInfo(jsonFile);
            SerializableClass.JsonThemeDeserializer deserializedTheme = JsonUtility.FromJson<SerializableClass.JsonThemeDeserializer>(jsonFile);
            Plugin.LogInfo(deserializedTheme.theme.ToString());
            Color normalColor, pressedColor, highlightedColor, selectedColor;

            Plugin.LogInfo("123");
            Plugin.LogInfo(deserializedTheme.theme.leaderboard.panelBody);

            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.panelBody, out panelBodyColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.scoresBody, out scoresbodyColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.rowEntry, out rowEntryImageColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.yourRowEntry, out rowEntryImageYouColor);

            Plugin.LogInfo("A");
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.scrollSpeedSlider.background, out scrollSpeedSliderBackgroundColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.scrollSpeedSlider.text, out scrollSpeedSliderTextColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.scrollSpeedSlider.handle, out scrollSpeedSliderHandleColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.scrollSpeedSlider.fill, out scrollSpeedSliderFillColor);

            Plugin.LogInfo("B");
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.slider.background, out leaderboardVerticalSliderBackgroundColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.slider.fill, out leaderboardVerticalSliderFillColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.slider.handle, out leaderboardVerticalSliderHandleColor);

            Plugin.LogInfo("C");
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.headerText, out leaderboardHeaderTextColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.text, out leaderboardTextColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.textOutline, out leaderboardTextOutlineColor);

            Plugin.LogInfo("D");
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.tabs.normal, out normalColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.tabs.pressed, out pressedColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.tabs.highlighted, out highlightedColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.leaderboard.tabs.selected, out selectedColor);

            tabsColors.normalColor = normalColor;
            tabsColors.pressedColor = pressedColor;
            tabsColors.highlightedColor = highlightedColor;
            tabsColors.selectedColor = selectedColor;
            tabsColors.colorMultiplier = 1f;
            tabsColors.fadeDuration = 0.1f;

            ColorUtility.TryParseHtmlString(deserializedTheme.theme.notification.border, out notificationBorderColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.notification.background, out notificationBackgroundColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.notification.defaultText, out defaultNotifColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.notification.defaultText, out warningNotifColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.notification.defaultText, out errorNotifColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.notification.textOutline, out notificationTextOutlineColor);

            ColorUtility.TryParseHtmlString(deserializedTheme.theme.replayButton.text, out replayButtonTextColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.replayButton.normal, out normalColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.replayButton.pressed, out pressedColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.replayButton.highlighted, out highlightedColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.replayButton.selected, out selectedColor);

            replayButtonColors.normalColor = normalColor;
            replayButtonColors.pressedColor = pressedColor;
            replayButtonColors.highlightedColor = highlightedColor;
            replayButtonColors.selectedColor = selectedColor;

            ColorUtility.TryParseHtmlString(deserializedTheme.theme.capsules.year, out capsuleYearColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.capsules.yearShadow, out capsuleYearShadowColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.capsules.composer, out capsuleComposerColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.capsules.composerShadow, out capsuleComposerShadowColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.capsules.genre, out capsuleGenreColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.capsules.genreShadow, out capsuleGenreShadowColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.capsules.description, out capsuleDescColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.capsules.descriptionShadow, out capsuleDescShadowColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.capsules.tempo, out capsuleTempoColor);

            ColorUtility.TryParseHtmlString(deserializedTheme.theme.randomButton.background, out randomBtnBackgroundColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.randomButton.outline, out randomBtnOutlineColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.randomButton.text, out randomBtnTextColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.randomButton.normal, out normalColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.randomButton.pressed, out pressedColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.randomButton.highlighted, out highlightedColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.randomButton.selected, out selectedColor);

            randomBtnIconColors.normalColor = normalColor;
            randomBtnIconColors.pressedColor = pressedColor;
            randomBtnIconColors.highlightedColor = highlightedColor;
            randomBtnIconColors.selectedColor = selectedColor;

            ColorUtility.TryParseHtmlString(deserializedTheme.theme.backButton.background, out backBtnBackgroundColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.backButton.outline, out backBtnOutlineColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.backButton.text, out backBtnTextColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.backButton.shadow, out backBtnShadowColor);

            ColorUtility.TryParseHtmlString(deserializedTheme.theme.playButton.background, out playBtnBackgroundColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.playButton.outline, out playBtnOutlineColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.playButton.text, out playBtnTextColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.playButton.shadow, out playBtnShadowColor);

            ColorUtility.TryParseHtmlString(deserializedTheme.theme.songButton.background, out songButtonBackgroundColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.songButton.outline, out songButtonOutlineColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.songButton.outlineOver, out songButtonOutlineOverColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.songButton.shadow, out songButtonShadowColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.songButton.text, out songButtonTextColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.songButton.textOver, out songButtonTextOverColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.songButton.square, out songButtonSquareColor);

            ColorUtility.TryParseHtmlString(deserializedTheme.theme.diffStar.gradientStart, out diffStarStartColor);
            ColorUtility.TryParseHtmlString(deserializedTheme.theme.diffStar.gradientEnd, out diffStarEndColor);
        }

        public static void SetRandomTheme()
        {
            System.Random rdm = new System.Random(System.DateTime.Now.Millisecond);

            panelBodyColor = GetRandomColor(rdm, 1);
            scoresbodyColor = GetRandomColor(rdm, 1);
            rowEntryImageColor = GetRandomColor(rdm, 1);
            rowEntryImageYouColor = GetRandomColor(rdm, 0.35f);

            scrollSpeedSliderBackgroundColor = GetRandomColor(rdm, 1);
            scrollSpeedSliderTextColor = GetRandomColor(rdm, 1);
            scrollSpeedSliderHandleColor = GetRandomColor(rdm, 1);
            scrollSpeedSliderFillColor = GetRandomColor(rdm, 1);

            leaderboardVerticalSliderBackgroundColor = GetRandomColor(rdm, 1);
            leaderboardVerticalSliderFillColor = GetRandomColor(rdm, 1);
            leaderboardVerticalSliderHandleColor = GetRandomColor(rdm, 1);

            leaderboardHeaderTextColor = GetRandomColor(rdm, 1);
            leaderboardTextColor = GetRandomColor(rdm, 1);
            leaderboardTextOutlineColor = GetRandomColor(rdm, 0.84f);

            tabsColors.normalColor = GetRandomColor(rdm, 1);
            tabsColors.pressedColor = GetRandomColor(rdm, 1);
            tabsColors.highlightedColor = GetRandomColor(rdm, 1);
            tabsColors.selectedColor = GetRandomColor(rdm, 1);
            tabsColors.colorMultiplier = 1f;
            tabsColors.fadeDuration = 0.1f;

            notificationBorderColor = GetRandomColor(rdm, 0.75f);
            notificationBackgroundColor = GetRandomColor(rdm, 0.95f);
            defaultNotifColor = GetRandomColor(rdm, 1);
            notificationTextOutlineColor = GetRandomColor(rdm, 0.84f);

            replayButtonTextColor = GetRandomColor(rdm, 1);
            replayButtonColors.normalColor = GetRandomColor(rdm, 1);
            replayButtonColors.highlightedColor = GetRandomColor(rdm, 1);
            replayButtonColors.pressedColor = GetRandomColor(rdm, 1);
            replayButtonColors.selectedColor = GetRandomColor(rdm, 1);

            capsuleYearColor = GetRandomColor(rdm, 1);
            capsuleTempoColor = GetRandomColor(rdm, 1);
            capsuleGenreColor = GetRandomColor(rdm, 1);
            capsuleComposerColor = GetRandomColor(rdm, 1);
            capsuleDescColor = GetRandomColor(rdm, 1);
            capsuleDescShadowColor = capsuleGenreShadowColor = capsuleComposerShadowColor = capsuleYearShadowColor = GetRandomColor(rdm, 1);

            randomBtnBackgroundColor = GetRandomColor(rdm, 1);
            randomBtnOutlineColor = GetRandomColor(rdm, 1);
            randomBtnTextColor = GetRandomColor(rdm, 1);
            randomBtnIconColors = tabsColors;

            backBtnBackgroundColor = GetRandomColor(rdm, 1);
            backBtnOutlineColor = GetRandomColor(rdm, 1);
            backBtnTextColor = GetRandomColor(rdm, 1);
            backBtnShadowColor = GetRandomColor(rdm, 1);

            playBtnBackgroundColor = GetRandomColor(rdm, 1);
            playBtnOutlineColor = GetRandomColor(rdm, 1);
            playBtnTextColor = GetRandomColor(rdm, 1);
            playBtnShadowColor = GetRandomColor(rdm, 1);

            songButtonBackgroundColor = GetRandomColor(rdm, 1);
            songButtonOutlineColor = GetRandomColor(rdm, 1);
            songButtonOutlineOverColor = GetRandomColor(rdm, 1);
            songButtonShadowColor = GetRandomColor(rdm, 1);
            songButtonTextColor = GetRandomColor(rdm, 1);
            songButtonTextOverColor = GetRandomColor(rdm, 1);
            songButtonSquareColor = GetRandomColor(rdm, 1);

            diffStarStartColor = GetRandomColor(rdm, 1);
            diffStarEndColor = GetRandomColor(rdm, 1);
        }

        private static Color GetRandomColor(System.Random rdm, float alpha)
        {
            return new Color((float)rdm.NextDouble(), (float)rdm.NextDouble(), (float)rdm.NextDouble(), alpha);
        }



    }
}
