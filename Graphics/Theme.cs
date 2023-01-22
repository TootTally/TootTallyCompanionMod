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
        public static Color notificationBorderColor, notificationBackgroundColor, notificationTextColor, notificationTextOutlineColor;
        public static Color replayButtonTextColor;
        public static ColorBlock replayButtonColors;
        public static Color defaultNotifColor, warningNotifColor, errorNotifColor;

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
            notificationBackgroundColor = new Color(0,0,0,.95f);
            notificationTextColor = new Color(1, 1, 1);
            notificationTextOutlineColor = new Color(0, 0, 0);

            replayButtonTextColor = new Color(0, 0, 0);
            replayButtonColors.normalColor = new Color(0.95f, 0.22f, 0.35f);
            replayButtonColors.highlightedColor = new Color(0.77f, 0.18f, 0.29f);
            replayButtonColors.pressedColor = new Color(1, 1, 0);
            replayButtonColors.selectedColor = new Color(0.95f, 0.22f, 0.35f);

        }

        public static void SetNightTheme()
        {
            panelBodyColor = new Color(0.2f, 0.2f, 0.2f);
            scoresbodyColor = new Color(0, 0, 0);
            rowEntryImageColor = new Color(0.12f, 0.12f, 0.12f);
            rowEntryImageYouColor = new Color(0.35f, 0.35f, 0.35f, 0.65f);

            leaderboardVerticalSliderBackgroundColor = new Color(0.45f, 0.45f, 0.45f);
            leaderboardVerticalSliderFillColor = new Color(0.15f, 0.15f, 0.15f);
            leaderboardVerticalSliderHandleColor = new Color(0.15f, 0.15f, 0.15f);

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
        }
        
        public static Color GetRandomColor(System.Random rdm, float alpha)
        {
            return new Color((float)rdm.NextDouble()*2, (float)rdm.NextDouble()*2, (float)rdm.NextDouble()*2, alpha);
        }

    }
}
