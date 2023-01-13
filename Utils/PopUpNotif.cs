using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils
{
    public class PopUpNotif : MonoBehaviour
    {
        public Text textHolder;
        public RectTransform rectTransform;
        private float _lifespan, _maxLifespan;
        private CanvasGroup _canvasGroup;

        public void SetText(string message) => textHolder.text = message;

        public void Initialize(float lifespan)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _lifespan = _maxLifespan = lifespan;
        }

        public void Update()
        {
            _lifespan -= Time.deltaTime;
            _canvasGroup.alpha = Mathf.Clamp(_lifespan * 3 / _maxLifespan, 0, 1);
            if (_lifespan < 0)
                PopUpNotifManager.QueueToRemovedFromList(this);
        }
    }
}
