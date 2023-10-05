using System;
using System.Collections.Generic;
using TootTally.Utils.Helpers;
using UnityEngine;

namespace TootTally.Graphics.Animation
{
    public class AnimationManager : MonoBehaviour
    {
        private static List<CustomAnimation> _animationList;
        private static List<CustomAnimation> _animationToAdd;
        private static List<CustomAnimation> _animationToRemove;
        private static bool _isInitialized;

        public static CustomAnimation AddNewTransformPositionAnimation(GameObject gameObject, Vector3 targetVector,
            float timeSpan, EasingHelper.SecondOrderDynamics secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, gameObject.transform.position, targetVector, 1f, timeSpan, CustomAnimation.VectorType.TransformPosition, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static CustomAnimation AddNewPositionAnimation(GameObject gameObject, Vector3 targetVector,
            float timeSpan, EasingHelper.SecondOrderDynamics secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, gameObject.GetComponent<RectTransform>().anchoredPosition, targetVector, 1f, timeSpan, CustomAnimation.VectorType.Position, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static CustomAnimation AddNewSizeDeltaAnimation(GameObject gameObject, Vector3 targetVector,
            float timeSpan, EasingHelper.SecondOrderDynamics secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, gameObject.GetComponent<RectTransform>().sizeDelta, targetVector, 1f, timeSpan, CustomAnimation.VectorType.SizeDelta, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static CustomAnimation AddNewScaleAnimation(GameObject gameObject, Vector2 targetVector,
           float timeSpan, EasingHelper.SecondOrderDynamics secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, gameObject.GetComponent<RectTransform>().localScale, targetVector, 1f, timeSpan, CustomAnimation.VectorType.Scale, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static CustomAnimation AddNewTransformScaleAnimation(GameObject gameObject, Vector2 targetVector,
           float timeSpan, EasingHelper.SecondOrderDynamics secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, gameObject.transform.localScale, targetVector, 1f, timeSpan, CustomAnimation.VectorType.TransformScale, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static CustomAnimation AddNewEulerAngleAnimation(GameObject gameObject, Vector3 targetVector,
           float timeSpan, EasingHelper.SecondOrderDynamics secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, gameObject.transform.eulerAngles, targetVector, 1f, timeSpan, CustomAnimation.VectorType.EulerAngle, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static CustomAnimation AddNewAnimation(GameObject gameObject, Vector3 startingVector, Vector3 targetVector, float speedMultiplier,
            float timeSpan, CustomAnimation.VectorType vectorType, EasingHelper.SecondOrderDynamics secondDegreeAnimation, bool disposeOnFinish, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, startingVector, targetVector, speedMultiplier, timeSpan, vectorType, secondDegreeAnimation, disposeOnFinish, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        private void Awake()
        {
            if (_isInitialized) return;

            _animationList = new List<CustomAnimation>();
            _animationToAdd = new List<CustomAnimation>();
            _animationToRemove = new List<CustomAnimation>();
            _isInitialized = true;
        }

        public static void AddToList(CustomAnimation anim) => _animationToAdd.Add(anim);
        public static void RemoveFromList(CustomAnimation anim) => _animationToRemove.Add(anim);

        private void Update()
        {
            if (!_isInitialized) return;

            //add animation the needs to be added
            if (_animationToAdd.Count > 0)
            {
                _animationToAdd.ForEach(anim => _animationList.Add(anim));
                _animationToAdd.Clear();
            }

            //update all animations
            _animationList.ForEach(anim => anim.UpdateVector());

            //remove animations that are done
            if (_animationToRemove.Count > 0)
            {
                _animationToRemove.ForEach(anim => _animationList.Remove(anim));
                _animationToRemove.Clear();
            }

        }

    }
}
