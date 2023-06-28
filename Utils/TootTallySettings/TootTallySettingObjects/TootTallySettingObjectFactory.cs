using Rewired.UI.ControlMapper;
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
using static Mono.Security.X509.X520;

namespace TootTally.Utils.TootTallySettings
{
    public static class TootTallySettingObjectFactory
    {
        private static GameObject _mainCanvas;
        private static GameObject _panelPrefab;
        private static Slider _sliderPrefab;
        private static Toggle _togglePrefab;
        private static Dropdown _dropdownPrefab;
        private static TMP_InputField _inputFieldPrefab;
        private static bool _isInitialized;

        public static void Initialize(HomeController __instance)
        {
            if (_isInitialized) return;

            _mainCanvas = GameObject.Find("MainCanvas");
            SetPanelPrefab();
            SetSliderPrefab(__instance);
            SetTogglePrefab(__instance);
            SetDropdownPrefab(__instance);
            SetInputFieldPrefab(__instance);

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

            var headerText = GameObjectFactory.CreateSingleText(mainPanel.transform, "TootTallySettingsHeader", "TootTally Settings (BETA)", GameTheme.themeColors.leaderboard.text);
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

        private static void SetDropdownPrefab(HomeController __instance)
        {
            _dropdownPrefab = GameObject.Instantiate(__instance.set_drp_antialiasing);
            _dropdownPrefab.name = "TootTallySettingsDropdownPrefab";
            _dropdownPrefab.onValueChanged = new Dropdown.DropdownEvent();
            _dropdownPrefab.ClearOptions();

            GameObject.DontDestroyOnLoad(_dropdownPrefab);
        }

        private static void SetInputFieldPrefab(HomeController __instance)
        {
            var inputHolder = new GameObject("InputFieldHolder");
            var rectHolder = inputHolder.AddComponent<RectTransform>();
            rectHolder.anchoredPosition = Vector2.zero;
            rectHolder.sizeDelta = new Vector2(350, 50);
            var inputImageHolder = GameObject.Instantiate(inputHolder, inputHolder.transform);
            var inputTextHolder = GameObject.Instantiate(inputImageHolder, inputHolder.transform);
            inputImageHolder.name = "Image";
            inputTextHolder.name = "Text";

            _inputFieldPrefab = inputHolder.AddComponent<TMP_InputField>();

            rectHolder.anchorMax = rectHolder.anchorMin = Vector2.zero;

            //pain... @_@
            _inputFieldPrefab.image = inputImageHolder.AddComponent<Image>();
            RectTransform rectImage = inputImageHolder.GetComponent<RectTransform>();

            rectImage.anchorMin = rectImage.anchorMax = rectImage.pivot = Vector2.zero;
            rectImage.anchoredPosition = new Vector2(0, 4);
            rectImage.sizeDelta = new Vector2(350,2);

            RectTransform rectText = inputTextHolder.GetComponent<RectTransform>();
            rectText.anchoredPosition = rectText.anchorMin = rectText.anchorMax = rectText.pivot = Vector2.zero;
            rectText.sizeDelta = new Vector2(350, 50);

            _inputFieldPrefab.textComponent = GameObjectFactory.CreateSingleText(inputTextHolder.transform, $"TextLabel", "", GameTheme.themeColors.leaderboard.text);
            _inputFieldPrefab.textComponent.rectTransform.pivot = new Vector2(0, 0.5f);
            _inputFieldPrefab.textComponent.alignment = TextAlignmentOptions.Left;
            _inputFieldPrefab.textComponent.margin = new Vector4(5, 0, 0, 0);
            _inputFieldPrefab.textComponent.enableWordWrapping = true;
            _inputFieldPrefab.textViewport = _inputFieldPrefab.textComponent.rectTransform;

            GameObject.DontDestroyOnLoad(_inputFieldPrefab);
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
            TootTallySettingObjectFactory.CreateVerticalSlider(panel.transform, $"{name}VerticalSlider", new Vector2(-200, -66), new Vector2(20, 1080), 0, 100, false);

            return panel;
        }

        public static Slider CreateSlider(Transform canvasTransform, string name, float min, float max, bool integerOnly)
        {
            var slider = GameObject.Instantiate(_sliderPrefab, canvasTransform);
            slider.name = name;
            slider.maxValue = max;
            slider.minValue = min;
            slider.wholeNumbers = integerOnly;

            return slider;
        }

        public static Slider CreateVerticalSlider(Transform canvasTransform, string name, Vector2 position, Vector2 size, float min, float max, bool integerOnly)
        {
            var slider = GameObject.Instantiate(_sliderPrefab, canvasTransform);
            slider.direction = Slider.Direction.TopToBottom;
            slider.name = name;
            

            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            sliderRect.sizeDelta = size;
            sliderRect.anchoredPosition = position;
            sliderRect.anchorMin = Vector2.one;

            RectTransform fillAreaRect = sliderRect.transform.Find("Fill Area").GetComponent<RectTransform>();
            fillAreaRect.sizeDelta = new Vector2(-19, -2);
            fillAreaRect.anchoredPosition = new Vector2(-5, 0);

            RectTransform handleSlideAreaRect = sliderRect.transform.Find("Handle Slide Area").GetComponent<RectTransform>();
            handleSlideAreaRect.sizeDelta = new Vector2(0, sliderRect.sizeDelta.y / -2f);
            RectTransform handleRect = handleSlideAreaRect.gameObject.transform.Find("Handle").GetComponent<RectTransform>();
            handleRect.anchoredPosition = new Vector2(-5, -3);
            handleRect.sizeDelta = new Vector2(0, 20);
            handleRect.pivot = Vector2.zero;
            RectTransform backgroundSliderRect = slider.transform.Find("Background").GetComponent<RectTransform>();
            backgroundSliderRect.anchoredPosition = new Vector2(-5, backgroundSliderRect.anchoredPosition.y);
            backgroundSliderRect.sizeDelta = new Vector2(-10, backgroundSliderRect.sizeDelta.y);

            GameObject.DestroyImmediate(handleRect.gameObject.transform.Find("SliderHandleText").gameObject);

            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = integerOnly;

            return slider;
        }

        public static Toggle CreateToggle(Transform canvasTransform, string name, Vector2 size, string text, bool value)
        {
            var toggle = GameObject.Instantiate(_togglePrefab, canvasTransform);
            toggle.transform.Find("Label").GetComponent<Text>().text = text;
            RectTransform rect = toggle.GetComponent<RectTransform>();
            rect.pivot = Vector3.zero;
            rect.anchoredPosition = Vector3.zero;
            rect.sizeDelta = size;
            toggle.name = name;
            toggle.isOn = value;

            return toggle;
        }

        public static Dropdown CreateDropdown(Transform canvasTransform, string name)
        {
            var dropdown = GameObject.Instantiate(_dropdownPrefab, canvasTransform);
            dropdown.name = name;

            return dropdown;

        }

        public static TMP_InputField CreateInputField(Transform canvasTransform, string name, Vector2 size, float fontSize, string text, bool isPassword)
        {
            var inputField = GameObject.Instantiate(_inputFieldPrefab, canvasTransform);
            inputField.name = name;
            inputField.GetComponent<RectTransform>().sizeDelta = size;
            inputField.transform.Find("Image").GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, 2);
            inputField.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = size;
            inputField.textComponent.GetComponent<RectTransform>().sizeDelta = size;
            inputField.textComponent.fontSize = fontSize;
            inputField.text = text;
            inputField.inputType = isPassword ? TMP_InputField.InputType.Password : TMP_InputField.InputType.Standard;

            return inputField;
        }

    }
}
