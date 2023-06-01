using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTally.Graphics.Animation;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public class SlideTooltip
    {
        private GameObject _hitboxGameObject;
        private CustomAnimation _enterAnimation;

        public GameObject tooltipGameObject;
        private Vector2 _startPosition, _targetPosition;


        public SlideTooltip(GameObject hitboxGameObject, GameObject tooltipGameObject, Vector2 startPosition, Vector2 targetPosition)
        {
            _hitboxGameObject = hitboxGameObject;

            this.tooltipGameObject = tooltipGameObject;
            CanvasGroup canvasGroup = tooltipGameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;

            _startPosition = startPosition;
            _targetPosition = targetPosition;

            EventTrigger tooltipHitboxEvents = _hitboxGameObject.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
            pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
            pointerEnterEvent.callback.AddListener((data) => OnPointerEnterHitbox());
            tooltipHitboxEvents.triggers.Add(pointerEnterEvent);

            EventTrigger.Entry pointerExitEvent = new EventTrigger.Entry();
            pointerExitEvent.eventID = EventTriggerType.PointerExit;
            pointerExitEvent.callback.AddListener((data) => OnPointerExitHitbox());
            tooltipHitboxEvents.triggers.Add(pointerExitEvent);
        }


        private void OnPointerEnterHitbox()
        {
            AnimationManager.AddNewPositionAnimation(tooltipGameObject, _targetPosition, 1.5f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
        }
        private void OnPointerExitHitbox()
        {
            AnimationManager.AddNewPositionAnimation(tooltipGameObject, _startPosition, 1.2f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
        }
    }
}
