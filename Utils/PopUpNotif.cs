using System;
using System.Collections.Generic;
using System.Text;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils
{
    public class PopUpNotif : MonoBehaviour
    {
        private Text _textHolder;
        private string _text;
        private Color _textColor;
        private RectTransform _rectTransform;
        private Vector2 _startPosition, _endPosition;
        private float _lifespan, _maxLifespan, _transitionTimer, _maxTransitionTimer;
        private CanvasGroup _canvasGroup;

        public void SetText(string message) => _text = message;
        public void SetTextColor(Color color) => _textColor = color;
        public void Initialize(float lifespan, Vector2 endPosition, float transitionTime)
        {
            this._rectTransform = gameObject.GetComponent<RectTransform>();
            SetTransitionToNewPosition(endPosition, transitionTime);
            this._textHolder = gameObject.transform.Find("NotifText").gameObject.GetComponent<Text>();
            _textHolder.text = _text;
            _textHolder.color = _textColor;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _lifespan = _maxLifespan = lifespan;
        }

        public void SetTransitionToNewPosition(Vector2 endPosition, float transitionTime)
        {
            _transitionTimer = _maxTransitionTimer = transitionTime;
            _startPosition = _rectTransform.anchoredPosition;
            _endPosition = endPosition;
        }

        public void Update()
        {
            if (_transitionTimer > 0)
            {
                _transitionTimer -= Time.deltaTime;
                float by = 1 - (_transitionTimer / _maxTransitionTimer);
                _rectTransform.anchoredPosition = EasingHelper.Lerp(_startPosition, _endPosition, EasingHelper.EaseOut(by));
            }

            _lifespan -= Time.deltaTime;
            if (_lifespan / 1.75f <= 1)
                _canvasGroup.alpha = EasingHelper.EaseIn(_lifespan / 1.25f);
            if (_lifespan < 0)
                PopUpNotifManager.QueueToRemovedFromList(this);
        }
    }
}
