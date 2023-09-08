using System;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils
{
    public class ScrollableSliderHandler : MonoBehaviour
    {
        public Slider slider;
        public float accelerationMult = 1f;

        private float _acceleration;
        private float _deceleration = 10f;

        public void Update()
        {
            if (slider == null) return;

            if (Input.mouseScrollDelta.y != 0)
                AddScrollAcceleration(Input.mouseScrollDelta.y * ((slider.maxValue - slider.minValue)/100f));
            UpdateScrolling();
        }

        private void AddScrollAcceleration(float value)
        {
            _acceleration -= value * accelerationMult * 125f; //Abitrary value just so it looks nice / feel nice
        }

        private void UpdateScrolling()
        {
            if (slider.value < slider.minValue)
            {
                slider.value = slider.minValue;
                _acceleration = 0;
            }
            else if (slider.value > slider.maxValue)
            {
                slider.value = slider.maxValue;
                _acceleration = 0;
            }
            else
            {
                if (Math.Round(Math.Abs(_acceleration), 2) <= 0.001f)
                    _acceleration = 0f;
                else
                    _acceleration -= (_acceleration * _deceleration) * Time.deltaTime;
                slider.value += _acceleration * Time.deltaTime;
            }
        }

        public void ResetAcceleration()
        {
            _acceleration = 0f;
        }
    }
}
