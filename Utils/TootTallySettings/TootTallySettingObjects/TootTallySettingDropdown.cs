using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingDropdown : BaseTootTallySettingObject
    {
        public Dropdown dropdown;
        private string[] _optionValues;
        private ConfigEntry<string> _config;
        public TootTallySettingDropdown(TootTallySettingPage page, string name, ConfigEntry<string> config, string[] optionValues = null) : base(name, page)
        {
            _optionValues = optionValues;
            _config = config;
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
