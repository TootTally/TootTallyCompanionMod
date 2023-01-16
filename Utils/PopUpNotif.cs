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
        private Vector2 _endPosition;
        private float _lifespan;
        private CanvasGroup _canvasGroup;
        EasingHelper.SecondOrderDynamics _secondOrderDynamic;

        public void SetText(string message) => _text = message;
        public void SetTextColor(Color color) => _textColor = color;
        public void Initialize(float lifespan, Vector2 endPosition)
        {
            this._rectTransform = gameObject.GetComponent<RectTransform>();
            _secondOrderDynamic = new EasingHelper.SecondOrderDynamics(1.3f, 0.75f, 0.75f);
            SetTransitionToNewPosition(endPosition);
            this._textHolder = gameObject.transform.Find("NotifText").gameObject.GetComponent<Text>();
            _textHolder.text = _text;
            _textHolder.color = _textColor;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _lifespan = lifespan;
        }

        public void SetTransitionConstants(float f, float z, float r) => _secondOrderDynamic.SetConstants(f, z, r);

        public void SetTransitionToNewPosition(Vector2 endPosition)
        {
            _secondOrderDynamic.SetStartPosition(_rectTransform.anchoredPosition);
            _endPosition = endPosition;
        }

        public void Update()
        {
            if (_secondOrderDynamic != null && _rectTransform.anchoredPosition != _endPosition)
                _rectTransform.anchoredPosition = _secondOrderDynamic.GetNewPosition(_endPosition, Time.deltaTime);

            _lifespan -= Time.deltaTime;
            if (_lifespan / 1.75f <= 1)
            {
                _canvasGroup.alpha = EasingHelper.EaseIn(_lifespan / 1.25f);
            }
            if (_lifespan < 0)
                PopUpNotifManager.QueueToRemovedFromList(this);
        }
    }
}
