using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingTextField : BaseTootTallySettingObject
    {
        public TMP_InputField inputField;
        public TMP_Text label;
        private string _defaultValue;
        private float _fontSize;
        private bool _isPassword;
        private Vector2 _size;
        public Action<string> onValueChanged, onEditEnd, onSubmit, onSelect, onDeselect;

        public TootTallySettingTextField(TootTallySettingPage page, string name, Vector2 size, float fontSize, string defaultValue, bool isPassword, Action<string> onSubmit = null) : base(name, page)
        {
            _defaultValue = defaultValue;
            _size = size;
            _fontSize = fontSize;
            _isPassword = isPassword;
            this.onSubmit = onSubmit;
            if (TootTallySettingsManager.isInitialized)
                Initialize();
        }

        public override void Initialize()
        {
            inputField = TootTallySettingObjectFactory.CreateInputField(_page.gridPanel.transform, name, _size, _fontSize, _defaultValue, _isPassword);
            label = inputField.textComponent;
            inputField.onValueChanged.AddListener(OnInputFieldTextChangeResizeBox);
            if (onValueChanged != null)
                inputField.onValueChanged.AddListener(onValueChanged.Invoke);
            if (onEditEnd != null)
                inputField.onEndEdit.AddListener(onEditEnd.Invoke);
            if (onSubmit != null)
                inputField.onSubmit.AddListener(onSubmit.Invoke);
            if (onSelect != null)
                inputField.onSelect.AddListener(onSelect.Invoke);
            if (onDeselect != null)
                inputField.onDeselect.AddListener(onDeselect.Invoke);

            base.Initialize();
        }

        public void OnInputFieldTextChangeResizeBox(string text)
        {
            var sizeDelta = new Vector2(_size.x, _size.y + (label.textInfo.lineCount - 1) * (_fontSize + 2.5f)); //2.5f extra offset cause the more text the more off it would be...
            inputField.GetComponent<RectTransform>().sizeDelta = sizeDelta;
            inputField.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = sizeDelta;
            label.GetComponent<RectTransform>().sizeDelta = sizeDelta;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_page.gridPanel.GetComponent<RectTransform>());
        }


        public override void Dispose()
        {
            GameObject.DestroyImmediate(inputField.gameObject);
        }
    }
}
