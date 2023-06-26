using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingTextField : BaseTootTallySettingObject
    {
        public TMP_InputField inputField;
        public TMP_Text label;
        private string _defaultValue;
        private Vector2 _size;

        public TootTallySettingTextField(TootTallySettingPage page, string name, Vector2 size, string defaultValue) : base(name, page)
        {
            _defaultValue = defaultValue;
            _size = size;
        }

        public override void Initialize()
        {
            inputField = TootTallySettingObjectFactory.CreateInputField(_page.gridPanel.transform, name, _size, _defaultValue);
            label = inputField.textComponent;
            base.Initialize();
        }

        public override void Dispose()  
        {
            GameObject.DestroyImmediate(inputField.gameObject);
        }
    }
}
