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
        private string _defaultValue;
        public TootTallySettingDropdown(TootTallySettingPage page, string name, string defaultValue, string[] optionValues = null) : base(name, page)
        {
            _optionValues = optionValues;
            _defaultValue = defaultValue;
            if (TootTallySettingsManager.isInitialized)
                Initialize();
        }
        public override void Initialize()
        {
            dropdown = TootTallySettingObjectFactory.CreateDropdown(_page.gridPanel.transform, name);
            if (_optionValues != null)
                AddOptions(_optionValues);
            dropdown.value = dropdown.options.FindIndex(x => x.text == _defaultValue);
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
