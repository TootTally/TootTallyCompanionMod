﻿using HarmonyLib;
using System.Collections.Generic;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings
{
    public static class TootTallySettingsManager
    {
        private const string MAIN_MENU_PATH = "MainCanvas/MainMenu";
        public static bool isInitialized;

        private static HomeController _currentInstance;

        private static GameObject _mainMenu, _mainSettingPanel, _settingPanelGridHolder;
        public static Transform GetSettingPanelGridHolderTransform { get => _settingPanelGridHolder.transform; }
        
        private static List<TootTallySettingPage> _settingPageList;
        private static TootTallySettingPage _currentActivePage;

        static TootTallySettingsManager()
        {
            _settingPageList = new List<TootTallySettingPage>();
        }

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        public static void InitializeTootTallySettingsManager(HomeController __instance)
        {
            _currentInstance = __instance;

            TootTallySettingObjectFactory.Initialize(__instance);

            GameObject mainCanvas = GameObject.Find("MainCanvas");
            _mainMenu = mainCanvas.transform.Find("MainMenu").gameObject;

            var btn = GameObjectFactory.CreateCustomButton(_mainMenu.transform, new Vector2(-1661, -456), new Vector2(164, 164), AssetManager.GetSprite("icon.png"), false, "TTSettingsOpenButton", delegate
            {
                AnimationManager.AddNewPositionAnimation(_mainMenu, new Vector2(1940, 0), 1.5f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            });
            btn.GetComponent<Image>().sprite = AssetManager.GetSprite("PfpMask.png");
            btn.GetComponent<Image>().color = Color.black;
            btn.button.colors = new ColorBlock()
            {
                normalColor = Color.white,
                pressedColor = Color.gray,
                highlightedColor = new Color(.3f, .3f, 0),
                colorMultiplier = 1f,
                fadeDuration = 0.2f
            };
            var outline = btn.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.yellow;
            outline.effectDistance = new Vector2(3, -3);
            _mainSettingPanel = TootTallySettingObjectFactory.CreateMainSettingPanel(_mainMenu.transform);

            _settingPanelGridHolder = _mainSettingPanel.transform.Find("SettingsPanelGridHolder").gameObject;
            ShowMainSettingPanel();

            _settingPageList.ForEach(page => page.Initialize());
            isInitialized = true;
        }

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Update))]
        [HarmonyPostfix]
        public static void Update()
        {
            if (!isInitialized) return;
            if (Input.GetKeyDown(KeyCode.Escape)) OnBackButtonClick();
        }

        public static void OnBackButtonClick()
        {
            if (_currentActivePage != null)
            {
                _currentActivePage.Hide();
                _currentActivePage = null;
                ShowMainSettingPanel();
            }
            else if (_mainSettingPanel.activeSelf)
                ReturnToMainMenu();
        }

        public static TootTallySettingPage AddNewPage(string pageName, string headerText, float elementSpacing, Color bgColor)
        {
            var page = GetSettingPageByName(pageName);
            if (page != null)
            {
                TootTallyLogger.LogInfo($"Page {pageName} already exist.");
                return page;
            }

            page = new TootTallySettingPage(pageName, headerText, elementSpacing, bgColor);
            page.OnPageAdd();
            _settingPageList.Add(page);

            return page;
        }

        public static TootTallySettingPage AddNewPage(TootTallySettingPage settingPage)
        {
            var page = GetSettingPageByName(settingPage.name);
            if (page != null)
            {
                TootTallyLogger.LogInfo($"Page {page.name} already exist.");
                return page;
            }

            page = settingPage;
            page.OnPageAdd();
            _settingPageList.Add(page);

            return page;
        }

        public static void SwitchActivePage(TootTallySettingPage page)
        {
            _currentActivePage?.Hide();
            _currentActivePage = page;
            HideMainSettingPanel();
            page.Show();
        }

        public static void RemovePage(TootTallySettingPage page)
        {
            page.OnPageRemove();
            if (_settingPageList.Contains(page))
                _settingPageList.Remove(page);
            else
                TootTallyLogger.LogInfo($"Page {page.name} couldn't be found.");
        }

        public static void ShowMainSettingPanel()
        {
            _mainSettingPanel.SetActive(true);
        }

        public static void HideMainSettingPanel()
        {
            _mainSettingPanel.SetActive(false);
        }

        public static void ReturnToMainMenu()
        {
            _currentInstance.tryToSaveSettings();
            AnimationManager.AddNewPositionAnimation(_mainMenu, Vector2.zero, 1.5f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
        }

        public static TootTallySettingPage GetSettingPageByName(string name) => _settingPageList.Find(page => page.name == name);
    }
}
