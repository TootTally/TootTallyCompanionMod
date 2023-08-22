using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using TMPro;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.Events;

namespace TootTally.Utils.TootTallySettings
{
    public class TootTallySettingPage
    {
        public static readonly Vector2 DEFAULT_OBJECT_SIZE = new Vector2(350, 50);
        public static readonly float DEFAULT_SLIDER_LENGTH = 250;
        public static readonly float DEFAULT_HEADER_FONTSIZE = 40;
        public static readonly float DEFAULT_FONTSIZE = 20;

        public bool isInitialized;

        public string name, headerName;
        public float elementSpacing;
        private List<BaseTootTallySettingObject> _settingObjectList;
        private GameObject _pageButton;
        private GameObject _fullPanel;
        public GameObject gridPanel;
        private Color _bgColor;
        public TootTallySettingPage(string pageName, string headerName, float elementSpacing, Color bgColor)
        {
            this.name = pageName;
            this.headerName = headerName;
            this.elementSpacing = elementSpacing;
            _bgColor = bgColor;
            _settingObjectList = new List<BaseTootTallySettingObject>();
            if (TootTallySettingsManager.isInitialized)
                Initialize();
        }

        public void Initialize()
        {
            _fullPanel = TootTallySettingObjectFactory.CreateSettingPanel(GameObject.Find("MainCanvas").transform, name, headerName, elementSpacing, _bgColor);
            gridPanel = _fullPanel.transform.Find("SettingsPanelGridHolder").gameObject;
            _pageButton = GameObjectFactory.CreateCustomButton(TootTallySettingsManager.GetSettingPanelGridHolderTransform, Vector2.zero, new Vector2(250, 60), name, $"Open{name}Button", () => TootTallySettingsManager.SwitchActivePage(this)).gameObject;
            _settingObjectList.ForEach(obj => obj.Initialize());
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
            GameObject.DestroyImmediate(_fullPanel);
            GameObject.DestroyImmediate(_pageButton);
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

        public void Remove()
        {
            TootTallySettingsManager.RemovePage(this);
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
        public TootTallySettingButton AddButton(string name, Action OnClick = null) => AddButton(name, DEFAULT_OBJECT_SIZE, name, OnClick);

        public TootTallySettingSlider AddSlider(string name, float min, float max, float length, string text, ConfigEntry<float> config, bool integerOnly) => AddSettingObjectToList(new TootTallySettingSlider(this, name, min, max, length, text, config, integerOnly)) as TootTallySettingSlider;
        public TootTallySettingSlider AddSlider(string name, float min, float max, ConfigEntry<float> config, bool integerOnly) => AddSlider(name, min, max, DEFAULT_SLIDER_LENGTH, name, config, integerOnly);

        public TootTallySettingToggle AddToggle(string name, Vector2 size, string text, ConfigEntry<bool> config, UnityAction<bool> onValueChange = null) => AddSettingObjectToList(new TootTallySettingToggle(this, name, size, text, config, onValueChange)) as TootTallySettingToggle;
        public TootTallySettingToggle AddToggle(string name, ConfigEntry<bool> config, UnityAction<bool> onValueChange = null) => AddToggle(name, DEFAULT_OBJECT_SIZE, name, config, onValueChange);

        public TootTallySettingDropdown AddDropdown(string name, string text, ConfigEntry<string> config, params string[] optionValues) => AddSettingObjectToList(new TootTallySettingDropdown(this, name, text, config, optionValues)) as TootTallySettingDropdown;
        public TootTallySettingDropdown AddDropdown(string name, ConfigEntry<string> config, params string[] optionValues) => AddDropdown(name, name, config, optionValues);

        public TootTallySettingTextField AddTextField(string name, Vector2 size, float fontSize, string defaultValue, bool isPassword = false, Action<string> onSubmit = null) => AddSettingObjectToList(new TootTallySettingTextField(this, name, size, fontSize, defaultValue, isPassword, onSubmit)) as TootTallySettingTextField;
        public TootTallySettingTextField AddTextField(string name, string defaultValue, bool isPassword = false, Action<string> onSubmit = null) => AddSettingObjectToList(new TootTallySettingTextField(this, name, DEFAULT_OBJECT_SIZE, DEFAULT_FONTSIZE, defaultValue, isPassword, onSubmit)) as TootTallySettingTextField;



        public TootTallySettingLabel AddLabel(string name, string text, float fontSize, FontStyles fontStyles = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft) => AddSettingObjectToList(new TootTallySettingLabel(this, name, text, fontSize, fontStyles, align)) as TootTallySettingLabel;
        public TootTallySettingLabel AddLabel(string name, FontStyles fontStyles = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft) => AddLabel(name, name, DEFAULT_FONTSIZE, fontStyles, align);


    }
}
