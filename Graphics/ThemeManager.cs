using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public static class ThemeManager
    {
        public static Text songyear, songgenre, songcomposer, songtempo, songduration, songdesctext;

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void ChangeThemeOnLevelSelectControllerStartPostFix(LevelSelectController __instance)
        {
            if (Theme.isDefault) return;

            foreach (GameObject btn in __instance.btns)
            {
                btn.transform.Find("ScoreText").gameObject.GetComponent<Text>().color = Theme.leaderboardTextColor;
            }

            #region SongButton
            GameObject btnBGPrefab = GameObject.Instantiate(__instance.btnbgs[0].gameObject);
            GameObject.DestroyImmediate(btnBGPrefab.transform.Find("Image").gameObject);

            for (int i = 0; i < 7; i++) //songbuttons only, not the arrow ones
            {
                Image img = __instance.btnbgs[i];
                img.sprite = AssetManager.GetSprite("SongButtonBackground.png");
                img.transform.parent.Find("Text").GetComponent<Text>().color = i == 0 ? Theme.songButtonTextOverColor : Theme.songButtonTextColor;

                GameObject btnBGShadow = GameObject.Instantiate(btnBGPrefab, img.gameObject.transform.parent);
                btnBGShadow.name = "Shadow";
                OverwriteGameObjectSpriteAndColor(btnBGShadow, "SongButtonShadow.png", Theme.songButtonShadowColor);

                GameObject btnBGOutline = GameObject.Instantiate(btnBGPrefab, img.gameObject.transform);
                btnBGOutline.name = "Outline";
                OverwriteGameObjectSpriteAndColor(btnBGOutline, "SongButtonOutline.png", Theme.songButtonOutlineColor);

                img.transform.Find("Image").GetComponent<Image>().color = Theme.songButtonImageColor;
                img.color = Theme.songButtonBackgroundColor;

            }

            for (int i = 7; i < __instance.btnbgs.Length; i++)
                __instance.btnbgs[i].color = Theme.songButtonOutlineColor;
            #endregion

            #region SongTitle
            __instance.songtitlebar.GetComponent<Image>().color = Theme.panelBodyColor;
            __instance.scenetitle.GetComponent<Text>().color = Theme.panelBodyColor;
            __instance.songtitle.GetComponent<Text>().color = Theme.leaderboardTextColor;
            GameObject.Find(GameObjectPathHelper.FULLSCREEN_PANEL_PATH + "title/GameObject").GetComponent<Text>().color = Theme.leaderboardTextColor;
            __instance.longsongtitle.color = Theme.leaderboardTextColor;
            #endregion

            #region Lines
            GameObject lines = __instance.btnspanel.transform.Find("RightLines").gameObject;
            LineRenderer redLine = lines.transform.Find("Red").GetComponent<LineRenderer>();
            redLine.startColor = Theme.panelBodyColor;
            redLine.endColor = Theme.scoresbodyColor;
            for (int i = 1; i < 8; i++)
            {
                LineRenderer yellowLine = lines.transform.Find("Yellow" + i).GetComponent<LineRenderer>();
                yellowLine.startColor = Theme.scoresbodyColor;
                yellowLine.endColor = Theme.panelBodyColor;
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
            OverwriteGameObjectSpriteAndColor(capsulesYearShadow, "YearCapsule.png", Theme.capsuleYearShadowColor);
            capsulesYearShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

            GameObject capsulesYear = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesYear, "YearCapsule.png", Theme.capsuleYearColor);

            songyear = GameObject.Instantiate(__instance.songyear, capsulesYear.transform);

            GameObject capsulesGenreShadow = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesGenreShadow, "GenreCapsule.png", Theme.capsuleGenreShadowColor);
            capsulesGenreShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

            GameObject capsulesGenre = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesGenre, "GenreCapsule.png", Theme.capsuleGenreColor);
            songgenre = GameObject.Instantiate(__instance.songgenre, capsulesGenre.transform);

            GameObject capsulesComposerShadow = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesComposerShadow, "ComposerCapsule.png", Theme.capsuleComposerShadowColor);
            capsulesComposerShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

            GameObject capsulesComposer = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesComposer, "ComposerCapsule.png", Theme.capsuleComposerColor);
            songcomposer = GameObject.Instantiate(__instance.songcomposer, capsulesComposer.transform);

            GameObject capsulesTempo = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesTempo, "BPMTimeCapsule.png", Theme.capsuleTempoColor);
            songtempo = GameObject.Instantiate(__instance.songtempo, capsulesTempo.transform);
            songduration = GameObject.Instantiate(__instance.songduration, capsulesTempo.transform);

            GameObject capsulesDescTextShadow = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesDescTextShadow, "DescCapsule.png", Theme.capsuleDescShadowColor);
            capsulesDescTextShadow.GetComponent<RectTransform>().anchoredPosition += new Vector2(5, -3);

            GameObject capsulesDescText = GameObject.Instantiate(capsulesPrefab, capsules.transform);
            OverwriteGameObjectSpriteAndColor(capsulesDescText, "DescCapsule.png", Theme.capsuleDescColor);
            songdesctext = GameObject.Instantiate(__instance.songdesctext, capsulesDescText.transform);

            GameObject.DestroyImmediate(capsules.GetComponent<Image>());
            GameObject.DestroyImmediate(capsulesPrefab);
            #endregion

            #region PlayButton
            GameObject playButtonBG = __instance.playbtn.transform.Find("BG").gameObject;
            GameObject playBGPrefab = GameObject.Instantiate(playButtonBG, __instance.playbtn.transform);
            foreach (Transform t in playBGPrefab.transform) GameObject.DestroyImmediate(t.gameObject);

            GameObject playBackgroundImg = GameObject.Instantiate(playBGPrefab, __instance.playbtn.transform);
            playBackgroundImg.name = "playBackground";
            OverwriteGameObjectSpriteAndColor(playBackgroundImg, "PlayBackground.png", Theme.playBtnBackgroundColor);

            GameObject playOutline = GameObject.Instantiate(playBGPrefab, __instance.playbtn.transform);
            playOutline.name = "playOutline";
            OverwriteGameObjectSpriteAndColor(playOutline, "PlayOutline.png", Theme.playBtnOutlineColor);

            GameObject playText = GameObject.Instantiate(playBGPrefab, __instance.playbtn.transform);
            playText.name = "playText";
            OverwriteGameObjectSpriteAndColor(playText, "PlayText.png", Theme.playBtnTextColor);

            GameObject playShadow = GameObject.Instantiate(playBGPrefab, __instance.playbtn.transform);
            playShadow.name = "playShadow";
            OverwriteGameObjectSpriteAndColor(playShadow, "PlayShadow.png", Theme.playBtnShadowColor);

            GameObject.DestroyImmediate(playButtonBG);
            GameObject.DestroyImmediate(playBGPrefab);
            #endregion

            #region BackButton
            GameObject backButtonBG = __instance.backbutton.transform.Find("BG").gameObject;
            GameObject backBGPrefab = GameObject.Instantiate(backButtonBG, __instance.backbutton.transform);
            foreach (Transform t in backBGPrefab.transform) GameObject.Destroy(t.gameObject);

            GameObject backBackgroundImg = GameObject.Instantiate(backBGPrefab, __instance.backbutton.transform);
            backBackgroundImg.name = "backBackgroundImg";
            OverwriteGameObjectSpriteAndColor(backBackgroundImg, "BackBackground.png", Theme.backBtnBackgroundColor);

            GameObject backOutline = GameObject.Instantiate(backBGPrefab, __instance.backbutton.transform);
            backOutline.name = "backOutline";
            OverwriteGameObjectSpriteAndColor(backOutline, "BackOutline.png", Theme.backBtnOutlineColor);

            GameObject backText = GameObject.Instantiate(backBGPrefab, __instance.backbutton.transform);
            backText.name = "backText";
            OverwriteGameObjectSpriteAndColor(backText, "BackText.png", Theme.backBtnTextColor);

            GameObject backShadow = GameObject.Instantiate(backBGPrefab, __instance.backbutton.transform);
            backShadow.name = "backShadow";
            OverwriteGameObjectSpriteAndColor(backShadow, "BackShadow.png", Theme.backBtnShadowColor);

            GameObject.DestroyImmediate(backButtonBG);
            GameObject.DestroyImmediate(backBGPrefab);
            #endregion

            #region RandomButton
            __instance.btnrandom.transform.Find("Text").GetComponent<Text>().color = Theme.randomBtnTextColor;

            GameObject randomButtonPrefab = GameObject.Instantiate(__instance.btnrandom);
            RectTransform randomRectTransform = randomButtonPrefab.GetComponent<RectTransform>();
            randomRectTransform.anchoredPosition = Vector2.zero;
            randomRectTransform.localScale = Vector3.one;

            GameObject randomButtonBackground = GameObject.Instantiate(randomButtonPrefab, __instance.btnrandom.transform);
            randomButtonBackground.name = "RandomBackground";
            OverwriteGameObjectSpriteAndColor(randomButtonBackground, "RandomBackground.png", Theme.randomBtnBackgroundColor);

            foreach (Transform t in randomButtonPrefab.transform) GameObject.DestroyImmediate(t.gameObject); // destroying text only after making our background object

            GameObject randomButtonOutline = GameObject.Instantiate(randomButtonPrefab, __instance.btnrandom.transform);
            randomButtonOutline.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -1);
            randomButtonOutline.name = "RandomOutline";
            OverwriteGameObjectSpriteAndColor(randomButtonOutline, "RandomOutline.png", Theme.randomBtnOutlineColor);

            GameObject randomButtonIcon = GameObject.Instantiate(randomButtonPrefab, __instance.btnrandom.transform);
            randomButtonIcon.name = "RandomIcon";
            OverwriteGameObjectSpriteAndColor(randomButtonIcon, "RandomIcon.png", Theme.randomBtnTextColor);

            GameObject.DestroyImmediate(__instance.btnrandom.GetComponent<Image>());
            GameObject.DestroyImmediate(randomButtonPrefab);
            #endregion

            //CapsulesTextColor
            songyear.color = Theme.leaderboardTextColor;
            songgenre.color = Theme.leaderboardTextColor;
            songduration.color = Theme.leaderboardTextColor;
            songcomposer.color = Theme.leaderboardTextColor;
            songtempo.color = Theme.leaderboardTextColor;
            songdesctext.color = Theme.leaderboardTextColor;
            OnPopulateSongNamesPostFix(__instance);
        }

        #region hoverAndUnHoverSongButtons
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverBtn))]
        [HarmonyPostfix]
        public static void OnHoverBtnPrefix(LevelSelectController __instance, object[] __args)
        {
            if (Theme.isDefault) return;
            if ((int)__args[0] >= 7)
            {
                __instance.btnbgs[(int)__args[0]].GetComponent<Image>().color = Theme.songButtonOutlineOverColor;
                return;
            }
            __instance.btnbgs[(int)__args[0]].GetComponent<Image>().color = Theme.songButtonBackgroundColor;
            __instance.btnbgs[(int)__args[0]].transform.Find("Outline").GetComponent<Image>().color = Theme.songButtonOutlineOverColor;
            __instance.btnbgs[(int)__args[0]].transform.parent.Find("Text").GetComponent<Text>().color = Theme.songButtonTextOverColor;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.unHoverBtn))]
        [HarmonyPostfix]
        public static void OnUnHoverBtnPrefix(LevelSelectController __instance, object[] __args)
        {
            if (Theme.isDefault) return;
            if ((int)__args[0] >= 7)
            {
                __instance.btnbgs[(int)__args[0]].GetComponent<Image>().color = Theme.songButtonOutlineColor;
                return;
            }
            __instance.btnbgs[(int)__args[0]].GetComponent<Image>().color = Theme.songButtonBackgroundColor;
            __instance.btnbgs[(int)__args[0]].transform.Find("Outline").GetComponent<Image>().color = Theme.songButtonOutlineColor;
            __instance.btnbgs[(int)__args[0]].transform.parent.Find("Text").GetComponent<Text>().color = Theme.songButtonTextColor;
        }
        #endregion

        #region PlayAndBackEvents
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverPlay))]
        [HarmonyPrefix]
        public static bool OnHoverPlayBypassIfThemeNotDefault() => Theme.isDefault;
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.unHoverPlay))]
        [HarmonyPrefix]
        public static bool OnUnHoverPlayBypassIfThemeNotDefault() => Theme.isDefault;
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverBack))]
        [HarmonyPrefix]
        public static bool OnHoverBackBypassIfThemeNotDefault() => Theme.isDefault;
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.hoverOutBack))]
        [HarmonyPrefix]
        public static bool OnHoverOutBackBypassIfThemeNotDefault() => Theme.isDefault;
        #endregion

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPostfix]
        public static void OnPopulateSongNamesPostFix(LevelSelectController __instance)
        {
            if (Theme.isDefault) return;

            for (int i = 0; i < 10; i++)
            {
                if (__instance.diffstars[i].color.a == 1)
                    __instance.diffstars[i].color = Color.Lerp(Theme.diffStarStartColor, Theme.diffStarEndColor, i / 9f);
            }

            songyear.text = __instance.songyear.text;
            songgenre.text = __instance.songgenre.text;
            songduration.text = __instance.songduration.text;
            songcomposer.text = __instance.songcomposer.text;
            songtempo.text = __instance.songtempo.text;
            songdesctext.text = __instance.songdesctext.text;
        }

        public static void OverwriteGameObjectSpriteAndColor(GameObject gameObject, string spriteName, Color spriteColor)
        {
            gameObject.GetComponent<Image>().sprite = AssetManager.GetSprite(spriteName);
            gameObject.GetComponent<Image>().color = spriteColor;
        }


    }
}
