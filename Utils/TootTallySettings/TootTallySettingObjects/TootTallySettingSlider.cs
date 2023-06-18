using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using TootTally.Utils.TootTallySettings.TootTallySetting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace TootTally.Utils.TootTallySettings.TootTallySettingObjects
{
    public class TootTallySettingSlider : BaseTootTallySettingObject
    {
        public Slider slider;
        public TMP_Text label;
        public TootTallySettingSlider(TootTallySettingPage page, string name, float length, string text) :base(name, page)
        {
            slider = TootTallySettingObjectFactory.CreateSlider(page.gridPanel.transform, name);
            slider.GetComponent<RectTransform>().sizeDelta = new Vector2(length, 20);
            var handleText = slider.transform.Find("Handle Slide Area/Handle/SliderHandleText").GetComponent<TMP_Text>();
            handleText.rectTransform.anchoredPosition = Vector2.zero;
            handleText.rectTransform.sizeDelta = new Vector2(35, 0);
            slider.onValueChanged.AddListener((float _value) => { handleText.text = SliderValueToText(_value); });
            label = GameObjectFactory.CreateSingleText(slider.transform, $"{name}Label", text, GameTheme.themeColors.leaderboard.text);
            label.rectTransform.anchoredPosition = new Vector2(0, 35);
            label.alignment = TextAlignmentOptions.TopLeft;
        }
        public static string SliderValueToText(float value) => ((int)(value * 100)).ToString();

        public override void Dispose()
        {
            GameObject.DestroyImmediate(slider.gameObject);
        }
    }
}
