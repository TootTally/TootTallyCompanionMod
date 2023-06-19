using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingSlider : BaseTootTallySettingObject
    {
        public Slider slider;
        public TMP_Text label;
        public TootTallySettingSlider(TootTallySettingPage page, string name, float min, float max, float length, string text, bool integerOnly) : base(name, page)
        {
            slider = TootTallySettingObjectFactory.CreateSlider(page.gridPanel.transform, name, min, max, integerOnly);
            slider.GetComponent<RectTransform>().sizeDelta = new Vector2(length, 20);
            var handleText = slider.transform.Find("Handle Slide Area/Handle/SliderHandleText").GetComponent<TMP_Text>();
            handleText.rectTransform.anchoredPosition = Vector2.zero;
            handleText.rectTransform.sizeDelta = new Vector2(35, 0);
            handleText.fontSize = 10;
            slider.onValueChanged.AddListener((float _value) => { handleText.text = SliderValueToText(_value); });
            label = GameObjectFactory.CreateSingleText(slider.transform, $"{name}Label", text, GameTheme.themeColors.leaderboard.text);
            label.rectTransform.anchoredPosition = new Vector2(0, 35);
            label.alignment = TextAlignmentOptions.TopLeft;
        }
        public string SliderValueToText(float value)
        {
            string text;

            if (slider.minValue == 0 && slider.maxValue == 1)
                text = ((int)(value * 100)).ToString() + "%";
            else if (slider.wholeNumbers)
                text = value.ToString();
            else
                text = $"{value:0:0}";
            return text;
        }

        public override void Dispose()
        {
            GameObject.DestroyImmediate(slider.gameObject);
        }
    }
}
