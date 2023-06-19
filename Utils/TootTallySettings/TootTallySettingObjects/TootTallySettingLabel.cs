﻿using TMPro;
using TootTally.Graphics;
using UnityEngine;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingLabel : BaseTootTallySettingObject
    {
        public TMP_Text label;
        public TootTallySettingLabel(TootTallySettingPage page, string name, string text, float fontSize, FontStyles fontStyles, TextAlignmentOptions align) : base(name, page)
        {
            label = GameObjectFactory.CreateSingleText(page.gridPanel.transform, name, text, GameTheme.themeColors.leaderboard.text);
            label.rectTransform.anchoredPosition = Vector2.zero;
            label.rectTransform.sizeDelta = new Vector2(0, fontSize + 10);
            label.enableWordWrapping = false;
            label.fontSize = fontSize;
            label.fontStyle = fontStyles;
            label.alignment = align;
        }

        public void SetText(string text) =>
            label.text = text;

        public override void Dispose()
        {
            GameObject.DestroyImmediate(label.gameObject);
        }
    }
}
