using BepInEx.Configuration;
using System;
using System.Linq;
using TMPro;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingDropdown : BaseTootTallySettingObject
    {
        public Dropdown dropdown;
        public TMP_Text label;
        private string[] _optionValues;
        private ConfigEntryBase _config;
        private BubblePopupHandler _bubble;

        public TootTallySettingDropdown(TootTallySettingPage page, string name, string text, ConfigEntry<string> config, string[] optionValues = null) : base(name, page)
        {
            _config = config;
            _optionValues = optionValues;
            if (TootTallySettingsManager.isInitialized)
            {
                Initialize();
            }
        }

        public TootTallySettingDropdown(TootTallySettingPage page, string name, ConfigEntryBase config) : base(name, page)
        {
            _config = config;
            if (TootTallySettingsManager.isInitialized)
            {
                Initialize();
            }
        }

        public void ConfigureDropdownEnum()
        {
            dropdown.AddOptions(Enum.GetNames(_config.BoxedValue.GetType()).ToList());
            dropdown.value = (int)_config.BoxedValue;
            dropdown.onValueChanged.AddListener(value => { _config.BoxedValue = value; });
        }

        public void ConfigureDropdownString()
        {
            if (_optionValues != null)
                AddOptions(_optionValues);
            if (!_optionValues.Contains(_config.BoxedValue))
                AddOptions(_config.BoxedValue.ToString());

            dropdown.value = dropdown.options.FindIndex(x => x.text == _config.BoxedValue.ToString());
            dropdown.onValueChanged.AddListener(value => { _config.BoxedValue = dropdown.options[value].text; });
        }

        public void AddOptions(params string[] name)
        {
            if (name.Length != 0)
                dropdown.AddOptions(name.ToList());
        }

        public override void Initialize()
        {
            dropdown = TootTallySettingObjectFactory.CreateDropdown(_page.gridPanel.transform, name);
            if (_config.BoxedValue.GetType() == typeof(string)) ConfigureDropdownString(); else ConfigureDropdownEnum();
            if (_config.Description.Description != null && _config.Description.Description.Length > 0)
            {
                _bubble = dropdown.gameObject.AddComponent<BubblePopupHandler>();
                _bubble.Initialize(GameObjectFactory.CreateBubble(Vector2.zero, $"{name}Bubble", _config.Description.Description, Vector2.zero, 6, true), true);
            }

            //Gonna rework that at some point.
            /*label = GameObjectFactory.CreateSingleText(dropdown.transform, $"{name}Label", _text, GameTheme.themeColors.leaderboard.text);
            label.rectTransform.anchoredPosition = new Vector2(0, 35);
            label.alignment = TextAlignmentOptions.TopLeft;*/

            base.Initialize();
        }

        public override void Dispose()
        {
            if (dropdown != null)
                GameObject.DestroyImmediate(dropdown.gameObject);
        }
    }
}
