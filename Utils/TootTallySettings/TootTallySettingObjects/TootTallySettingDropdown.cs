using BepInEx.Configuration;
using System.Linq;
using TMPro;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingDropdown : BaseTootTallySettingObject
    {
        public Dropdown dropdown;
        public TMP_Text label;
        private string[] _optionValues;
        private ConfigEntry<string> _config;
        private string _text;
        public TootTallySettingDropdown(TootTallySettingPage page, string name, string text, ConfigEntry<string> config, string[] optionValues = null) : base(name, page)
        {
            _optionValues = optionValues;
            _config = config;
            _text = text;
            if (TootTallySettingsManager.isInitialized)
                Initialize();
        }
        public override void Initialize()
        {
            dropdown = TootTallySettingObjectFactory.CreateDropdown(_page.gridPanel.transform, name);
            if (_optionValues != null)
                AddOptions(_optionValues);
            if (!_optionValues.Contains(_config.Value))
                AddOptions(_config.Value);
            dropdown.value = dropdown.options.FindIndex(x => x.text == _config.Value);
            dropdown.onValueChanged.AddListener((value) => { _config.Value = dropdown.options[value].text; });
            //Gonna rework that at some point.
            /*label = GameObjectFactory.CreateSingleText(dropdown.transform, $"{name}Label", _text, GameTheme.themeColors.leaderboard.text);
            label.rectTransform.anchoredPosition = new Vector2(0, 35);
            label.alignment = TextAlignmentOptions.TopLeft;*/

            base.Initialize();
        }
        public void AddOptions(params string[] name)
        {
            if (name.Length != 0)
                dropdown.AddOptions(name.ToList());
        }

        public override void Dispose()
        {
            GameObject.DestroyImmediate(dropdown.gameObject);
        }
    }
}
