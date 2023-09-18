using BepInEx.Configuration;
using TMPro;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings.TootTallySettingObjects
{
    public class TootTallySettingColorSliders : BaseTootTallySettingObject
    {
        public Slider sliderR, sliderG, sliderB;
        public TMP_Text labelR, labelG, labelB;

        private float _length;
        private string _text;
        private ConfigEntry<Color> _config;

        public TootTallySettingColorSliders(TootTallySettingPage page, string name, string text, float length, ConfigEntry<Color> config) : base(name, page)
        {
            _text = text;
            _length = length;
            _config = config;
            if (TootTallySettingsManager.isInitialized)
                Initialize();
        }

        public override void Initialize()
        {
            sliderR = TootTallySettingObjectFactory.CreateSlider(_page.gridPanel.transform, name, 0, 1, false);
            sliderG = TootTallySettingObjectFactory.CreateSlider(_page.gridPanel.transform, name, 0, 1, false);
            sliderB = TootTallySettingObjectFactory.CreateSlider(_page.gridPanel.transform, name, 0, 1, false);

            SetSlider(sliderR, _length, _config.Value.r, "Red", out labelR);
            SetSlider(sliderG, _length, _config.Value.g, "Green", out labelG);
            SetSlider(sliderB, _length, _config.Value.b, "Blue", out labelB);

            var handleTextR = sliderR.transform.Find("Handle Slide Area/Handle/SliderHandleText").GetComponent<TMP_Text>();
            sliderR.onValueChanged.AddListener((value) => { OnSliderValueChange(sliderB, handleTextR, value); });

            var handleTextG = sliderG.transform.Find("Handle Slide Area/Handle/SliderHandleText").GetComponent<TMP_Text>();
            sliderG.onValueChanged.AddListener((value) => { OnSliderValueChange(sliderB, handleTextG, value); });

            var handleTextB = sliderB.transform.Find("Handle Slide Area/Handle/SliderHandleText").GetComponent<TMP_Text>();
            sliderB.onValueChanged.AddListener((value) => { OnSliderValueChange(sliderB,handleTextB, value); });

            base.Initialize();
        }

        public void OnSliderValueChange(Slider s, TMP_Text label, float value)
        {
            label.text = SliderValueToText(s, value);
            UpdateConfig();
        }

        public void UpdateConfig()
        {
            _config.Value = new Color(sliderR.value, sliderG.value, sliderB.value);
        }

        public static void SetSlider(Slider s, float length, float value, string text, out TMP_Text label)
        {
            s.GetComponent<RectTransform>().sizeDelta = new Vector2(length, 20);
            var handleText = s.transform.Find("Handle Slide Area/Handle/SliderHandleText").GetComponent<TMP_Text>();
            handleText.rectTransform.anchoredPosition = Vector2.zero;
            handleText.rectTransform.sizeDelta = new Vector2(35, 0);
            handleText.fontSize = 10;
            s.value = value;
            label = GameObjectFactory.CreateSingleText(s.transform, $"{s.name}Label", text, GameTheme.themeColors.leaderboard.text);
            label.rectTransform.anchoredPosition = new Vector2(0, 35);
            label.alignment = TextAlignmentOptions.TopLeft;
        }

        public static string SliderValueToText(Slider s, float value)
        {
            string text;

            if (s.minValue == 0 && s.maxValue == 1)
                text = ((int)(value * 100)).ToString() + "%";
            else if (s.wholeNumbers)
                text = value.ToString();
            else
                text = $"{(s.wholeNumbers ? value : value * 100f):0:00}";
            return text;
        }

        public override void Dispose()
        {
            GameObject.DestroyImmediate(sliderR.gameObject);
            GameObject.DestroyImmediate(sliderG.gameObject);
            GameObject.DestroyImmediate(sliderB.gameObject);
        }
    }
}
