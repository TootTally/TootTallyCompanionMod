using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using TootTally.Utils.TootTallySettings.TootTallySetting;
using TootTally.Utils.TootTallySettings.TootTallySettingObjects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingPage
    {
        public string name;
        private List<BaseTootTallySettingObject> _settingObjectList;
        private GameObject _fullPanel;
        public GameObject gridPanel;
        public TootTallySettingPage(string pageName, string headerName, float elementSpacing)
        {
            this.name = pageName;

            _settingObjectList = new List<BaseTootTallySettingObject>();
            _fullPanel = TootTallySettingObjectFactory.CreateSettingPanel(GameObject.Find("MainCanvas").transform, pageName, headerName, elementSpacing);
            gridPanel = _fullPanel.transform.Find("SettingsPanelGridHolder").gameObject;
        }

        public void OnPageAdd()
        {

        }

        public void OnPageRemove()
        {
            List<BaseTootTallySettingObject> objToDelete = new(_settingObjectList);
            objToDelete.ForEach(obj =>
            {
                obj.Remove();
            });
        }

        public void RemoveSettingObjectFromList(string name)
        {
            var settingObject = _settingObjectList.Find(obj => obj.name == name);
            if (settingObject == null)
            {
                TootTallyLogger.LogInfo($"{name} object couldn't be found.");
                return;
            }

            RemoveSettingObjectFromList(settingObject);
        }

        public void RemoveSettingObjectFromList(BaseTootTallySettingObject settingObject)
        {
            if (!settingObject.isDisposed)
                settingObject.Dispose();
            _settingObjectList.Remove(settingObject);
        }

        public BaseTootTallySettingObject AddSettingObjectToList(BaseTootTallySettingObject settingObject)
        {
            _settingObjectList.Add(settingObject);
            return settingObject;
        }

        public BaseTootTallySettingObject GetSettingObjectByName(string name) => _settingObjectList.Find(obj => obj.name == name);

        public void Show()
        {
            _fullPanel.SetActive(true);
        }

        public void Hide()
        {
            _fullPanel.SetActive(false);
        }

        public TootTallySettingButton AddButton(string name, Vector2 size, string text, Action OnClick = null) => AddSettingObjectToList(new TootTallySettingButton(this, name, size, text, OnClick)) as TootTallySettingButton;
        public TootTallySettingSlider AddSlider(string name, float min, float max, float length, string text, bool integerOnly) => AddSettingObjectToList(new TootTallySettingSlider(this, name, min, max, length, text, integerOnly)) as TootTallySettingSlider;
        public TootTallySettingToggle AddToggle(string name, Vector2 size, string text, UnityAction<bool> onValueChange = null) => AddSettingObjectToList(new TootTallySettingToggle(this, name, size, text, onValueChange)) as TootTallySettingToggle;

        public TootTallySettingLabel AddLabel(string name, string text, float fontSize, FontStyles fontStyles = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft) => AddSettingObjectToList(new TootTallySettingLabel(this, name, text, fontSize, fontStyles, align)) as TootTallySettingLabel;

    }
}
