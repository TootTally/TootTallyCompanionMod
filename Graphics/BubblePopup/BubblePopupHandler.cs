using TMPro;
using TootTally.Graphics.Animation;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TootTally.Graphics
{
    public class BubblePopupHandler : MonoBehaviour
    {
        private GameObject _bubble;
        private EventTrigger _parentTrigger;
        private CustomAnimation _positionAnimation;
        private CustomAnimation _scaleAnimation;

        public void Initialize(GameObject bubble)
        {
            this._bubble = bubble;
            this._bubble.transform.SetParent(transform);
            this._bubble.transform.position = transform.position;
        }

        public void Awake()
        {
            var parent = transform.gameObject;
            if (!parent.TryGetComponent(out _parentTrigger))
                _parentTrigger = parent.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) => OnPointerEnter());

            EventTrigger.Entry pointerExitEvent = new EventTrigger.Entry();
            pointerExitEvent.eventID = EventTriggerType.PointerExit;
            pointerExitEvent.callback.AddListener((data) => OnPointerExit());

            _parentTrigger.triggers.Add(pointerEnterEvent);
            _parentTrigger.triggers.Add(pointerExitEvent);
        }

        public void Update()
        {
            var v3 = Input.mousePosition;
            v3.z = 10;
            _positionAnimation?.SetTargetVector(Camera.main.ScreenToWorldPoint(v3));
        }

        private void OnPointerEnter()
        {
            if (_bubble == null) return;

            _positionAnimation?.Dispose();
            _scaleAnimation?.Dispose();
            _bubble.transform.localScale = Vector2.zero;
            _bubble.SetActive(true);
            _positionAnimation = AnimationManager.AddNewTransformPositionAnimation(_bubble, Camera.main.ScreenToWorldPoint(Input.mousePosition), 999f, GetSecondDegreeAnimation());
            _scaleAnimation = AnimationManager.AddNewTransformScaleAnimation(_bubble, Vector3.one, 0.8f, GetSecondDegreeAnimation());
        }

        private void OnPointerExit()
        {
            if (_bubble == null) return;

            _positionAnimation?.Dispose();
            _positionAnimation = null;
            _scaleAnimation?.Dispose();
            _scaleAnimation = AnimationManager.AddNewTransformScaleAnimation(_bubble, Vector2.zero, 0.8f, GetSecondDegreeAnimation(), delegate
            {
                _bubble.SetActive(false);
            });

        }

        public EasingHelper.SecondOrderDynamics GetSecondDegreeAnimation() => new(2.5f, .85f, 1f);

    }
}
