using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public static class GameObjectFactory
    {
        private static CustomButton _buttonPrefab;
        private static Text _textPrefab;
        private static Toggle _togglePrefab;
        private static Slider _sliderPrefab;
        private static GameObject _starPrefab;

        private static GameObject _settingsGraphics;

        private static bool _isInitialized = false;

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        static void YoinkSettingsGraphics(HomeController __instance)
        {
            _settingsGraphics = __instance.fullsettingspanel.transform.Find("Settings").gameObject;
            Initialize();
        }


        public static void Initialize()
        {

            if (_isInitialized) return;

            SetCustomButtonPrefab();
            SetTextPrefab();
            SetStarPrefab();
            SetTogglePrefab();
            SetCustomSliderPrefab();
        }

        //yoink the gameobject that contains a button, copy it, yeet its content and add my own custom button instead
        public static void SetCustomButtonPrefab()
        {
            GameObject settingBtn = _settingsGraphics.transform.Find("GRAPHICS/btn_opengraphicspanel").gameObject;

            GameObject gameObjectHolder = UnityEngine.Object.Instantiate(settingBtn);

            var tempBtn = gameObjectHolder.GetComponent<Button>();
            var oldBtnColors = tempBtn.colors;


            UnityEngine.Object.DestroyImmediate(tempBtn);

            var myBtn = gameObjectHolder.AddComponent<Button>();
            myBtn.colors = oldBtnColors;

            _buttonPrefab = gameObjectHolder.AddComponent<CustomButton>();
            _buttonPrefab.ConstructNewButton(gameObjectHolder.GetComponent<Button>(), gameObjectHolder.GetComponent<RectTransform>(), gameObjectHolder.GetComponentInChildren<Text>());

            gameObjectHolder.SetActive(false);

            UnityEngine.Object.DontDestroyOnLoad(gameObjectHolder);
        }

        public static void SetStarPrefab()
        {
            GameObject star = _settingsGraphics.transform.Find("ALLEGIANCE/star (back)").gameObject;

            _starPrefab = UnityEngine.Object.Instantiate(star);

            _starPrefab.SetActive(false);

            UnityEngine.Object.DontDestroyOnLoad(_starPrefab);
        } 

        public static void SetTextPrefab() //Plan on yoinking some other graphics in the future
        {

        }
        public static void SetTogglePrefab()
        {


        }
        public static void SetCustomSliderPrefab()
        {

        }

        public static CustomButton CreateCustomButton(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, string text, string name, Action onClick = null)
        {
            CustomButton newButton = UnityEngine.Object.Instantiate(_buttonPrefab, canvasTransform);
            newButton.name = name;
            newButton.gameObject.SetActive(true);

            newButton.textHolder.text = text;
            newButton.textHolder.alignment = TextAnchor.MiddleCenter;
            newButton.textHolder.fontSize = 22;
            newButton.textHolder.horizontalOverflow = HorizontalWrapMode.Overflow;
            newButton.textHolder.verticalOverflow = VerticalWrapMode.Overflow;
            newButton.textHolder.color = Color.black;


            newButton.rectTransform.sizeDelta = size;
            newButton.rectTransform.anchoredPosition = anchoredPosition;

            newButton.button.onClick.AddListener(() => onClick?.Invoke());

            return newButton;
        }

        public static GameObject CreateLoadingScreenStar(Transform canvasTransform, Vector2 anchoredPosition, Vector2 size, float eulerRotationY, string name)
        {
            GameObject myStar = UnityEngine.Object.Instantiate(_starPrefab, canvasTransform);
            myStar.name = name;
            myStar.gameObject.SetActive(true);

            RectTransform rectBack = myStar.GetComponent<RectTransform>();
            rectBack.anchoredPosition = anchoredPosition;
            rectBack.eulerAngles = new Vector3(0, eulerRotationY, 0);
            rectBack.sizeDelta = size;

            RectTransform rectFront = myStar.transform.Find("star (front)").GetComponent<RectTransform>();
            rectFront.anchoredPosition = new Vector2(-1, 1);
            rectFront.sizeDelta = size;

            return myStar;
        }
    }
}
