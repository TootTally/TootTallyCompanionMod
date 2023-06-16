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
        private static GameObject _mainMenu, _mainSettingPanel, _settingPanelGridHolder, _panelPrefab;
        private static GridLayoutGroup _settingPanelGridLayoutGroup;
        private static List<TootTallySettingPage> _settingPageList;
        private static TootTallySettingPage _currentActivePage;

        public static GameObject GetPanelPrefab { get => _panelPrefab; }

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        static public void OnHomeControllerStartAddSettingsPage(HomeController __instance)
        {
            _settingPageList = new List<TootTallySettingPage>();

            GameObject mainCanvas = GameObject.Find("MainCanvas");
            _mainMenu = mainCanvas.transform.Find("MainMenu").gameObject;

            GameObjectFactory.CreateCustomButton(_mainMenu.transform, new Vector2(-1860, -415), new Vector2(60, 250), "<", "TTSettingsOpenButton", delegate
            {
                AnimationManager.AddNewPositionAnimation(_mainMenu, new Vector2(1940, 0), 1.5f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            });

            _mainSettingPanel = GameObject.Instantiate(mainCanvas.transform.Find("SettingsPanel").gameObject, _mainMenu.transform);
            _mainSettingPanel.name = "TootTallySettingsPanel";
            _mainSettingPanel.GetComponent<Image>().color = new Color(0, .2f, 0, 0); //Hide box

            int childCount = _mainSettingPanel.transform.childCount - 1;
            for (int i = childCount; i >= 0; i--)
                GameObject.DestroyImmediate(_mainSettingPanel.transform.GetChild(i).gameObject);

            _settingPanelGridHolder = GameObject.Instantiate(_mainSettingPanel, _mainSettingPanel.transform);
            _settingPanelGridHolder.name = "SettingsPanelGridHolder";
            _settingPanelGridHolder.SetActive(true);

            _settingPanelGridHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
            _settingPanelGridHolder.GetComponent<Image>().color = new Color(.2f, 0, 0, 0); //Hide box

            _mainSettingPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1940, 0);
            var headerText = GameObjectFactory.CreateSingleText(_mainSettingPanel.transform, "TootTallySettingsHeader", "TootTally Settings", GameTheme.themeColors.leaderboard.text);
            headerText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 475);
            headerText.fontSize = 80;
            headerText.fontStyle = TMPro.FontStyles.Bold | TMPro.FontStyles.UpperCase;
            headerText.alignment = TMPro.TextAlignmentOptions.Top;

            _panelPrefab = GameObject.Instantiate(_mainSettingPanel);
            _panelPrefab.name = "SettingPanelPrefab";
            _panelPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            _settingPanelGridLayoutGroup = _settingPanelGridHolder.AddComponent<GridLayoutGroup>();
            _settingPanelGridLayoutGroup.padding = new RectOffset(100, 100, 20, 20);
            _settingPanelGridLayoutGroup.spacing = new Vector2(25, 25);
            _settingPanelGridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            _settingPanelGridLayoutGroup.cellSize = new Vector2(250, 80);

            GameObjectFactory.CreateCustomButton(_mainSettingPanel.transform, new Vector2(-1570, -66), new Vector2(250, 80), "Back", "TTSettingsBackButton", OnBackButtonClick);

            TootTallySettingPage tootTallyPage = AddNewPage("TootTally");
            tootTallyPage.AddButton("testButton", new Vector2(250, 100), "testButton", () =>
            {
                tootTallyPage.AddButton("DynamicButton", new Vector2(250, 100), "Dynamic Test");
            });
            TootTallySettingPage testPage1 = AddNewPage("Test Page1");
            TootTallySettingPage testPage2 = AddNewPage("Test Page2");

            ShowMainSettingPanel();
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
            {
                TootTallyLogger.LogInfo("test12312313");
                ReturnToMainMenu();
            }
        }

        public static TootTallySettingPage AddNewPage(string pageName)
        {
            var page = new TootTallySettingPage(pageName);
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
            _settingPageList.Remove(page);
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
