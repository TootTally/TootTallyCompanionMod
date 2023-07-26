using System;
using TootTally.Graphics;
using UnityEngine;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingButton : BaseTootTallySettingObject
    {
        public CustomButton button;

        private Vector2 _size;
        private string _text;
        private Action<CustomButton> _onClick;

        public TootTallySettingButton(TootTallySettingPage page, string name, Vector2 size, string text, Action<CustomButton> onClick = null) : base(name, page)
        {
            _size = size;
            _text = text;
            _onClick = onClick;
            if (TootTallySettingsManager.isInitialized)
                Initialize();
        }

        public override void Initialize()
        {
            button = GameObjectFactory.CreateCustomButton(_page.gridPanel.transform, Vector2.zero, _size, _text, name, _onClick);
        }

        public override void Dispose()
        {
            GameObject.DestroyImmediate(button.gameObject);
        }

    }
}
