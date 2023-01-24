using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public static class Theme
    {
        public static bool isDefault;

        public static Color panelBodyColor, scoresbodyColor, rowEntryImageColor, rowEntryImageYouColor;
        public static Color leaderboardVerticalSliderHandleColor, leaderboardVerticalSliderBackgroundColor, leaderboardVerticalSliderFillColor;
        public static Color leaderboardHeaderTextColor, leaderboardTextColor, leaderboardTextOutlineColor;
        public static ColorBlock tabsColors;

        public static Color scrollSpeedSliderTextColor, scrollSpeedSliderBackgroundColor, scrollSpeedSliderFillColor, scrollSpeedSliderHandleColor;

        public static Color notificationBorderColor, notificationBackgroundColor, notificationTextColor, notificationTextOutlineColor;
        public static Color defaultNotifColor, warningNotifColor, errorNotifColor;

        public static Color replayButtonTextColor;
        public static ColorBlock replayButtonColors;

        public static Color capsuleYearColor, capsuleYearShadowColor, capsuleComposerColor, capsuleComposerShadowColor, capsuleGenreColor, capsuleGenreShadowColor, capsuleDescColor, capsuleDescShadowColor, capsuleTempoColor;

        public static Color randomBtnOutlineColor, randomBtnBackgroundColor, randomBtnTextColor;
        public static ColorBlock randomBtnIconColors;

        public static Color backBtnOutlineColor, backBtnBackgroundColor, backBtnTextColor, backBtnShadowColor;
        public static Color playBtnOutlineColor, playBtnBackgroundColor, playBtnTextColor, playBtnShadowColor;

        public static Color songButtonBackgroundColor, songButtonTextColor, songButtonTextOverColor, songButtonOutlineColor, songButtonShadowColor, songButtonOutlineOverColor, songButtonImageColor;

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
            tabsColors.normalColor = new Color(1, 1, 1);
            tabsColors.colorMultiplier = 1f;
            tabsColors.fadeDuration = 0.1f;

            notificationBorderColor = new Color(1, 0.3f, 0.5f, 0.75f);
            notificationBackgroundColor = new Color(0, 0, 0, .95f);
            notificationTextColor = new Color(1, 1, 1);
            notificationTextOutlineColor = new Color(0, 0, 0);
            defaultNotifColor = new Color(1, 1, 1);
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
            notificationTextColor = new Color(1, 1, 1);
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
            songButtonImageColor = new Color(0, 0, 0);

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
            notificationTextColor = new Color(0, 0, 0);
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
            notificationTextColor = new Color(0, 0, 0);
            notificationTextOutlineColor = new Color(0.85f, 0.85f, 0.85f, .84f);

            replayButtonTextColor = new Color(0, 0, 0);
            replayButtonColors.normalColor = new Color(1, 1, 1);
            replayButtonColors.highlightedColor = new Color(.7f, .7f, .7f);
            replayButtonColors.pressedColor = new Color(.42f, .42f, .42f);
            replayButtonColors.selectedColor = new Color(1, 1, 1);


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
            notificationTextColor = GetRandomColor(rdm, 1);
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
            songButtonImageColor = GetRandomColor(rdm, 1);

            diffStarStartColor = GetRandomColor(rdm, 1);
            diffStarEndColor = GetRandomColor(rdm, 1);
        }

        private static Color GetRandomColor(System.Random rdm, float alpha)
        {
            return new Color((float)rdm.NextDouble(), (float)rdm.NextDouble(), (float)rdm.NextDouble(), alpha);
        }



    }
}
