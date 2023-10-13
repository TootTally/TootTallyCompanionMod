using System;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils
{
    public class ScrollableSliderHandler : MonoBehaviour
    {
        public float accelerationMult = 1f;

        private Slider _slider;
        private float _acceleration;
        private float _deceleration = 10f;

        public void Awake()
        {
            try
            {
                _slider = GetComponent<Slider>();
            }
            catch
            {
                TootTallyLogger.LogError("ScrollableHanlder was not attached to a slider.");
                GameObject.DestroyImmediate(gameObject);
            }
        }


        public void Update()
        {
            if (_slider == null) return;

            if (Input.mouseScrollDelta.y != 0)
                AddScrollAcceleration(Input.mouseScrollDelta.y * ((_slider.maxValue - _slider.minValue)/100f));
            UpdateScrolling();
        }

        private void AddScrollAcceleration(float value)
        {
            _acceleration -= value * accelerationMult * 125f; //Abitrary value just so it looks nice / feel nice
        }

        private void UpdateScrolling()
        {
            if (_slider.value < _slider.minValue)
            {
                _slider.value = _slider.minValue;
                _acceleration = 0;
            }
            else if (_slider.value > _slider.maxValue)
            {
                _slider.value = _slider.maxValue;
                _acceleration = 0;
            }
            else
            {
                if (Math.Round(Math.Abs(_acceleration), 2) <= 0.001f)
                    _acceleration = 0f;
                else
                    _acceleration -= (_acceleration * _deceleration) * Time.deltaTime;
                _slider.value += _acceleration * Time.deltaTime;
            }
        }

        public void ResetAcceleration()
        {
            _acceleration = 0f;
        }
    }
}
