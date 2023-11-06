using System;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingButton : BaseTootTallySettingObject
    {
        public CustomButton button;

        private Vector2 _size;
        private string _text;
        private string _description;
        private Action _onClick;

        public TootTallySettingButton(TootTallySettingPage page, string name, Vector2 size, string text, string description, Action onClick = null) : base(name, page)
        {
            _size = size;
            _text = text;
            _description = description;
            _onClick = onClick;
            if (TootTallySettingsManager.isInitialized)
                Initialize();
        }

        public override void Initialize()
        {
            button = GameObjectFactory.CreateCustomButton(_page.gridPanel.transform, Vector2.zero, _size, _text, name, _onClick);
            if (_description != "")
            {
                var bubble = button.gameObject.AddComponent<BubblePopupHandler>();
                bubble.Initialize(GameObjectFactory.CreateBubble(Vector2.zero, $"{name}Bubble", _description, Vector2.zero, 6, true), true);
            }

        }

        public override void Dispose()
        {
            GameObject.DestroyImmediate(button.gameObject);
        }

    }
}
