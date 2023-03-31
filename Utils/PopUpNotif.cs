using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils
{
    public class PopUpNotif : MonoBehaviour
    {
        public string GetText { get => _text; }

        private Text _textHolder;
        private string _text;
        private Color _textColor;
        private RectTransform _rectTransform;
        private Vector2 _endPosition;
        private float _lifespan;
        private CanvasGroup _canvasGroup;
        EasingHelper.SecondOrderDynamics _secondOrderDynamic;

        public void SetText(string message) => _text = message;
        public void SetTextSize(int size) => _textHolder.fontSize = size;

        public void SetTextAlign(TextAnchor textAnchor) => _textHolder.alignment = textAnchor;
        public void UpdateText(string text) => _textHolder.text = _text = text;
        public void SetTextColor(Color color) => _textColor = color;
        public void Initialize(float lifespan, Vector2 endPosition)
        {
            this._rectTransform = gameObject.GetComponent<RectTransform>();
            _secondOrderDynamic = new EasingHelper.SecondOrderDynamics(1.3f, 0.75f, 0.75f);
            SetTransitionToNewPosition(endPosition);
            this._textHolder = gameObject.transform.Find("NotifText").gameObject.GetComponent<Text>();
            _textHolder.GetComponent<RectTransform>().sizeDelta = _rectTransform.sizeDelta - Vector2.one * 20;
            _textHolder.GetComponent<RectTransform>().anchoredPosition += new Vector2(1, -1) * 10;
            _textHolder.verticalOverflow = VerticalWrapMode.Overflow;
            _textHolder.text = _text;
            _textHolder.color = _textColor;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _lifespan = lifespan;
        }
        public void Initialize(float lifespan, Vector2 endPosition, Vector2 textRectSize)
        {
            this._rectTransform = gameObject.GetComponent<RectTransform>();
            _secondOrderDynamic = new EasingHelper.SecondOrderDynamics(1.3f, 0.75f, 0.75f);
            SetTransitionToNewPosition(endPosition);
            this._textHolder = gameObject.transform.Find("NotifText").gameObject.GetComponent<Text>();
            _textHolder.GetComponent<RectTransform>().sizeDelta = textRectSize;
            _textHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(.5f, 0) * textRectSize;
            _textHolder.text = _text;
            _textHolder.color = _textColor;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _lifespan = lifespan;
        }
        public void Initialize(float lifespan, Vector2 endPosition, Vector2 textRectSize, Vector2 textPosition)
        {
            this._rectTransform = gameObject.GetComponent<RectTransform>();
            _secondOrderDynamic = new EasingHelper.SecondOrderDynamics(1.3f, 0.75f, 0.75f);
            SetTransitionToNewPosition(endPosition);
            this._textHolder = gameObject.transform.Find("NotifText").gameObject.GetComponent<Text>();
            _textHolder.GetComponent<RectTransform>().sizeDelta = textRectSize;
            _textHolder.GetComponent<RectTransform>().anchoredPosition = textPosition;
            _textHolder.text = _text;
            _textHolder.color = _textColor;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _lifespan = lifespan;
        }

        public void SetTransitionConstants(float f, float z, float r) => _secondOrderDynamic.SetConstants(f, z, r);

        public void SetTransitionToNewPosition(Vector2 endPosition)
        {
            _secondOrderDynamic.SetStartVector(_rectTransform.anchoredPosition);
            _endPosition = endPosition;
        }

        public void Update()
        {
            if (_secondOrderDynamic != null && _rectTransform.anchoredPosition != _endPosition)
                _rectTransform.anchoredPosition = _secondOrderDynamic.GetNewVector(_endPosition, Time.deltaTime);

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
