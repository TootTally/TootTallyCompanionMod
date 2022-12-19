using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public class CustomButton : MonoBehaviour
    {
        public Button button;
        public RectTransform rectTransform;
        public Text textHolder;

        public void ConstructNewButton(Button button, RectTransform rectTransform, Text textHolder)
        {
            this.button = button;
            this.rectTransform = rectTransform;
            this.textHolder = textHolder;
            this.name = name;
            RemoveAllOnClickActions();
        }

        public void SetPosition(Vector2 anchoredPosition)
        {
            rectTransform.anchoredPosition = anchoredPosition; 
        }

        public void RemoveAllOnClickActions() => button.onClick.RemoveAllListeners();

    }
}
