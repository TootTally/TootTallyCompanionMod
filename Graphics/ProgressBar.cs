using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public class ProgressBar : MonoBehaviour
    {
        private Slider _bar;
        private bool _isInitialized;
        private bool _isActive;
        private Image _barImage;

        public ProgressBar(Slider bar, bool active)
        {
            _bar = bar;
            _barImage = bar.transform.Find("Fill Area/Fill").GetComponent<Image>();
            UpdateColor(0);
            _isActive = active;
            _bar.gameObject.SetActive(active);
            _isInitialized = true;
        }

        public void UpdateValue(float value)
        {
            if (!_isInitialized) return;

            _bar.value = value;
            UpdateColor(value);
        }

        private void UpdateColor(float percent)
        {
            _barImage.color = new Color(1f - ((percent - .5f) * 2f), percent * 2f, 0f); //Basically algo to lerp from red -> yellow -> green
        }

        public void ToggleActive()
        {
            _isActive = !_isActive;
            _bar.gameObject.SetActive(_isActive);
        }
    }
}
