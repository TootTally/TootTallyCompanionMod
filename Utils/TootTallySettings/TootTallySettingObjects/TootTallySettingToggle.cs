using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingToggle : BaseTootTallySettingObject
    {
        public Toggle toggle;
        public TootTallySettingToggle(TootTallySettingPage page, string name, Vector2 size, string text, UnityAction<bool> onValueChange) : base(name, page)
        {
            toggle = TootTallySettingObjectFactory.CreateToggle(page.gridPanel.transform, name, size, text);
            toggle.onValueChanged = new Toggle.ToggleEvent();
            toggle.onValueChanged.AddListener(onValueChange);
        }

        public override void Dispose()
        {
            GameObject.DestroyImmediate(toggle.gameObject);
        }
    }
}
