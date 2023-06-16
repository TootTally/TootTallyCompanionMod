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
        private VerticalLayoutGroup _verticalLayoutGroup;
        public TootTallySettingPage(string pageName)
        {
            this.name = pageName;

            _settingObjectList = new List<BaseTootTallySettingObject>();
            _fullPanel = GameObject.Instantiate(TootTallySettingsManager.GetPanelPrefab, GameObject.Find("MainCanvas").transform);
            _fullPanel.name = $"{pageName}Page";
            _fullPanel.transform.Find("TootTallySettingsHeader").GetComponent<TMP_Text>().text = name;
            gridPanel = _fullPanel.transform.Find("SettingsPanelGridHolder").gameObject;
            _verticalLayoutGroup = gridPanel.AddComponent<VerticalLayoutGroup>();
            _verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            _verticalLayoutGroup.childControlHeight = _verticalLayoutGroup.childControlWidth = false;
            _verticalLayoutGroup.childForceExpandHeight = _verticalLayoutGroup.childForceExpandWidth = false;
            _verticalLayoutGroup.childScaleHeight = _verticalLayoutGroup.childScaleWidth = false;
            _verticalLayoutGroup.padding = new RectOffset(100, 100, 20, 20);
            GameObjectFactory.CreateCustomButton(_fullPanel.transform, new Vector2(-1570, -66), new Vector2(250, 80), "Return", $"{pageName}ReturnButton", TootTallySettingsManager.OnBackButtonClick);
            

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

        public BaseTootTallySettingObject AddButton(string name, Vector2 size, string text, Action OnClick = null) => AddSettingObjectToList(new TootTallySettingButton(this, name, size, text, OnClick));

    }
}
