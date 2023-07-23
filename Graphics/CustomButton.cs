using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public class CustomButton : MonoBehaviour
    {
        public Button button;
        public Text textHolder;

        public void ConstructNewButton(Button button, Text textHolder)
        {
            this.button = button;
            this.textHolder = textHolder;
            textHolder.supportRichText = true;
            textHolder.maskable = true;
            this.name = name;
            RemoveAllOnClickActions();
        }

        public void RemoveAllOnClickActions() => button.onClick.RemoveAllListeners();

    }
}
