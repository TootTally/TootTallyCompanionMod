using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings.TootTallySettingObjects
{
    public static class TootTallySettingObjectFactory
    {
        private static GameObject _mainCanvas;
        private static GameObject _panelPrefab;
        private static Slider _sliderPrefab;
        private static Toggle _togglePrefab;
        private static bool _isInitialized;

        public static void Initialize(HomeController __instance)
        {
            if (_isInitialized) return;

            _mainCanvas = GameObject.Find("MainCanvas");
            SetPanelPrefab();
            SetSliderPrefab(__instance);
            SetTogglePrefab(__instance);

            _isInitialized = true;
        }

        private static void SetPanelPrefab()
        {
            var mainMenu = _mainCanvas.transform.Find("MainMenu").gameObject;

            var mainPanel = GameObject.Instantiate(_mainCanvas.transform.Find("SettingsPanel").gameObject, mainMenu.transform);
            mainPanel.name = "TootTallySettingsPanelPrefab";
            mainPanel.GetComponent<Image>().color = new Color(0, .2f, 0, 0); //Hide box

            int childCount = mainPanel.transform.childCount - 1;
            for (int i = childCount; i >= 0; i--)
                GameObject.DestroyImmediate(mainPanel.transform.GetChild(i).gameObject);

            var settingPanelGridHolder = GameObject.Instantiate(mainPanel, mainPanel.transform);
            settingPanelGridHolder.name = "SettingsPanelGridHolder";
            settingPanelGridHolder.SetActive(true);

            settingPanelGridHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
            settingPanelGridHolder.GetComponent<Image>().color = new Color(.2f, 0, 0, 0); //Hide box

            var headerText = GameObjectFactory.CreateSingleText(mainPanel.transform, "TootTallySettingsHeader", "TootTally Settings", GameTheme.themeColors.leaderboard.text);
            headerText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 475);
            headerText.fontSize = 80;
            headerText.fontStyle = TMPro.FontStyles.Bold | TMPro.FontStyles.UpperCase;
            headerText.alignment = TMPro.TextAlignmentOptions.Top;

            _panelPrefab = GameObject.Instantiate(mainPanel);
            GameObject.DestroyImmediate(mainPanel);
            _panelPrefab.name = "SettingPanelPrefab";
            _panelPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            GameObject.DontDestroyOnLoad(_panelPrefab);
        }

        private static void SetSliderPrefab(HomeController __instance)
        {
            _sliderPrefab = GameObject.Instantiate(__instance.fullsettingspanel.transform.Find("Settings/AUDIO/master_volume/SET_sld_volume").GetComponent<Slider>());
            _sliderPrefab.name = "TootTallySettingsSliderPrefab";
            var handle = _sliderPrefab.transform.Find("Handle Slide Area/Handle");
            var scrollSpeedSliderText = GameObjectFactory.CreateSingleText(handle, "SliderHandleText", "1", GameTheme.themeColors.scrollSpeedSlider.text);
            scrollSpeedSliderText.text = "50";

            _sliderPrefab.onValueChanged = new Slider.SliderEvent();

            _sliderPrefab.fillRect.gameObject.GetComponent<Image>().color = GameTheme.themeColors.scrollSpeedSlider.fill;
            _sliderPrefab.transform.Find("Background").GetComponent<Image>().color = GameTheme.themeColors.scrollSpeedSlider.background;
            _sliderPrefab.minValue = 0;
            _sliderPrefab.maxValue = 1;
            _sliderPrefab.value = .5f;

            GameObject.DontDestroyOnLoad(_sliderPrefab);
        }

        private static void SetTogglePrefab(HomeController __instance)
        {
            _togglePrefab = GameObject.Instantiate(__instance.set_tog_accessb_jumpscare);
            _togglePrefab.name = "TootTallySettingsTogglePrefab";

            GameObject.DontDestroyOnLoad(_togglePrefab);
        }

        public static GameObject CreateMainSettingPanel(Transform canvasTransform)
        {
            var panel = GameObject.Instantiate(_panelPrefab, canvasTransform);
            panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1940, 0);

            var gridHolder = panel.transform.Find("SettingsPanelGridHolder").gameObject;

            var gridLayoutGroup = gridHolder.AddComponent<GridLayoutGroup>();
            gridLayoutGroup.padding = new RectOffset(100, 100, 20, 20);
            gridLayoutGroup.spacing = new Vector2(25, 25);
            gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            gridLayoutGroup.cellSize = new Vector2(250, 80);

            GameObjectFactory.CreateCustomButton(panel.transform, new Vector2(-1570, -66), new Vector2(250, 80), "Back", "TTSettingsBackButton", TootTallySettingsManager.OnBackButtonClick);

            return panel;
        }

        public static GameObject CreateSettingPanel(Transform canvasTransform, string name, string headerText, float elementSpacing)
        {
            var panel = GameObject.Instantiate(_panelPrefab, canvasTransform);
            panel.name = $"TootTally{name}Panel";

            panel.transform.Find("TootTallySettingsHeader").GetComponent<TMP_Text>().text = headerText;

            var gridPanel = panel.transform.Find("SettingsPanelGridHolder").gameObject;
            var verticalLayoutGroup = gridPanel.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            verticalLayoutGroup.childControlHeight = verticalLayoutGroup.childControlWidth = false;
            verticalLayoutGroup.childForceExpandHeight = verticalLayoutGroup.childForceExpandWidth = false;
            verticalLayoutGroup.childScaleHeight = verticalLayoutGroup.childScaleWidth = false;
            verticalLayoutGroup.padding = new RectOffset(100, 100, 20, 20);
            verticalLayoutGroup.spacing = elementSpacing;
            GameObjectFactory.CreateCustomButton(panel.transform, new Vector2(-1570, -66), new Vector2(250, 80), "Return", $"{name}ReturnButton", TootTallySettingsManager.OnBackButtonClick);

            return panel;
        }

        public static Slider CreateSlider(Transform canvasTransform, string name)
        {
            var slider = GameObject.Instantiate(_sliderPrefab, canvasTransform);
            slider.name = name;

            return slider;
        }

        public static Toggle CreateToggle(Transform canvasTransform, string name, Vector2 size, string text)
        {
            var toggle = GameObject.Instantiate(_togglePrefab, canvasTransform);
            toggle.transform.Find("Label").GetComponent<Text>().text = text;
            RectTransform rect = toggle.GetComponent<RectTransform>();
            rect.pivot = Vector3.zero;
            rect.anchoredPosition = Vector3.zero;
            rect.sizeDelta = size;
            toggle.name = name;

            return toggle;
        }

    }
}
