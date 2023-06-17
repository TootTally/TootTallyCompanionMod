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
using UnityEngine.UI;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingPage
    {
        public string name;
        private List<BaseTootTallySettingObject> _settingObjectList;
        private GameObject _fullPanel;
        public GameObject gridPanel;
        public TootTallySettingPage(string pageName, string headerName)
        {
            this.name = pageName;

            _settingObjectList = new List<BaseTootTallySettingObject>();
            _fullPanel = TootTallySettingObjectFactory.CreateSettingPanel(GameObject.Find("MainCanvas").transform, pageName, headerName);
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

        public void RemoveSettingObjectFromList(BaseTootTallySettingObject settingObject) => _settingObjectList.Remove(settingObject);

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
        public TootTallySettingSlider AddSlider(string name, float length, string text) => AddSettingObjectToList(new TootTallySettingSlider(this, name, length, text)) as TootTallySettingSlider;

    }
}
