using BepInEx.Configuration;
using System;
using System.Drawing;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingToggle : BaseTootTallySettingObject
    {
        public Toggle toggle;
        private Vector2 _size;
        private string _text;
        private UnityAction<bool> _onValueChange;
        private ConfigEntry<bool> _config;
        public TootTallySettingToggle(TootTallySettingPage page, string name, Vector2 size, string text, ConfigEntry<bool> config, UnityAction<bool> onValueChange) : base(name, page)
        {
            _size = size;
            _text = text;
            _config = config;
            _onValueChange = onValueChange;
            if (TootTallySettingsManager.isInitialized)
                Initialize();
        }

        public override void Initialize()
        {
            toggle = TootTallySettingObjectFactory.CreateToggle(_page.gridPanel.transform, name, _size, _text, _config);
            if (_onValueChange != null)
                toggle.onValueChanged.AddListener(_onValueChange);
            base.Initialize();
        }

        public override void Dispose()
        {
            GameObject.DestroyImmediate(toggle.gameObject);
        }
    }
}
