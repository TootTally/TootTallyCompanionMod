using HarmonyLib;
using System.Collections.Generic;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Utils.Helpers;
using UnityEngine;

namespace TootTally.Utils.TootTallySettings
{
    public static class TootTallySettingsManager
    {
        private const string MAIN_MENU_PATH = "MainCanvas/MainMenu";
        public static bool isInitialized;

        private static HomeController _currentInstance;

        private static GameObject _mainMenu, _mainSettingPanel, _settingPanelGridHolder;
        public static Transform GetSettingPanelGridHolderTransform { get => _settingPanelGridHolder.transform; }
        
        private static GameObject _sliderPrefab;
        private static List<TootTallySettingPage> _settingPageList;
        private static TootTallySettingPage _currentActivePage;

        static TootTallySettingsManager()
        {
            _settingPageList = new List<TootTallySettingPage>();
        }

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        static public void InitializeTootTallySettingsManager(HomeController __instance)
        {
            _currentInstance = __instance;

            TootTallySettingObjectFactory.Initialize(__instance);

            GameObject mainCanvas = GameObject.Find("MainCanvas");
            _mainMenu = mainCanvas.transform.Find("MainMenu").gameObject;

            GameObjectFactory.CreateCustomButton(_mainMenu.transform, new Vector2(-1860, -415), new Vector2(60, 250), "<", "TTSettingsOpenButton", delegate
            {
                AnimationManager.AddNewPositionAnimation(_mainMenu, new Vector2(1940, 0), 1.5f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            });

            _mainSettingPanel = TootTallySettingObjectFactory.CreateMainSettingPanel(_mainMenu.transform);

            _settingPanelGridHolder = _mainSettingPanel.transform.Find("SettingsPanelGridHolder").gameObject;
            ShowMainSettingPanel();

            _settingPageList.ForEach(page => page.Initialize());
            isInitialized = true;

        }

        private static void UpdateHexLabel(float r, float g, float b, TootTallySettingLabel label)
        {
            label.SetText(ColorUtility.ToHtmlStringRGB(new Color(r, g, b)));
        }

        public static void OnBackButtonClick(CustomButton sender)
        {
            if (_currentActivePage != null)
            {
                _currentActivePage.Hide();
                _currentActivePage = null;
                ShowMainSettingPanel();
            }
            else
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
