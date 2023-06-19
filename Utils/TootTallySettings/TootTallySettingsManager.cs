using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        private static GameObject _mainMenu, _mainSettingPanel, _settingPanelGridHolder;
        private static GameObject _sliderPrefab;
        private static List<TootTallySettingPage> _settingPageList;
        private static TootTallySettingPage _currentActivePage;

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        static public void OnHomeControllerStartAddSettingsPage(HomeController __instance)
        {
            _settingPageList = new List<TootTallySettingPage>();

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
        }

        private static void UpdateHexLabel(float r, float g, float b, TootTallySettingLabel label)
        {
            label.SetText(ColorUtility.ToHtmlStringRGB(new Color(r, g, b)));
        }

        public static void OnBackButtonClick()
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

        public static TootTallySettingPage AddNewPage(string pageName, string headerText, float elementSpacing)
        {
            var page = GetSettingPageByName(pageName);
            if (page != null)
            {
                TootTallyLogger.LogInfo($"Page {pageName} already exist.");
                return page;
            }

            page = new TootTallySettingPage(pageName, headerText, elementSpacing);
            page.OnPageAdd();
            _settingPageList.Add(page);

            GameObjectFactory.CreateCustomButton(_settingPanelGridHolder.transform, Vector2.zero, new Vector2(250, 60), pageName, $"Open{pageName}Button", delegate
            {
                _currentActivePage?.Hide();
                _currentActivePage = page;
                HideMainSettingPanel();
                page.Show();
            });

            return page;
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
            AnimationManager.AddNewPositionAnimation(_mainMenu, Vector2.zero, 1.5f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
        }

        public static TootTallySettingPage GetSettingPageByName(string name) => _settingPageList.Find(page => page.name == name);
    }
}
