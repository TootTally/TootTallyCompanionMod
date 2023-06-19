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
        public TootTallySettingDropdown(TootTallySettingPage page, string name) : base(name, page)
        {
            dropdown = TootTallySettingObjectFactory.CreateDropdown(page.gridPanel.transform, name);
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
