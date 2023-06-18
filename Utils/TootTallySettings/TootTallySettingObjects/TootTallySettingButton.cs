using System;
using TootTally.Graphics;
using TootTally.Utils.TootTallySettings.TootTallySetting;
using UnityEngine;

namespace TootTally.Utils.TootTallySettings.TootTallySettingObjects
{
    public class TootTallySettingButton : BaseTootTallySettingObject
    {
        public CustomButton button;
        public TootTallySettingButton(TootTallySettingPage page, string name, Vector2 size, string text, Action OnClick = null) : base(name, page)
        {
            button = GameObjectFactory.CreateCustomButton(page.gridPanel.transform, Vector2.zero, size, text, name, OnClick);
        }

        public override void Dispose()
        {
            GameObject.DestroyImmediate(button.gameObject);
        }
    }
}
