using System;
using System.Collections.Generic;
using System.Text;
using TootTally.Utils.Helpers;
using UnityEngine;

namespace TootTally.Graphics.Animation
{
    public class CustomAnimation
    {
        private GameObject _gameObject;
        private Vector2 _targetVector;
        private float _speedMultiplier, _timeSpan;
        private EasingHelper.SecondOrderDynamics _secondDegreeAnimation;
        private bool _disposeOnFinish;
        private Action<GameObject> _onFinishCallback;
        private VectorType _vectorType;
        private bool _isAlreadyDisposed;

        public CustomAnimation(GameObject gameObject, Vector2 startingVector, Vector2 targetVector, float speedMultiplier,
            float timeSpan, VectorType vectorType, EasingHelper.SecondOrderDynamics secondDegreeAnimation, bool disposeOnFinish, Action<GameObject> onFinishCallback = null)
        {
            _gameObject = gameObject;
            _targetVector = targetVector;
            _speedMultiplier = speedMultiplier;
            _timeSpan = timeSpan;
            _vectorType = vectorType;
            _disposeOnFinish = disposeOnFinish;
            _onFinishCallback = onFinishCallback;
            _secondDegreeAnimation = secondDegreeAnimation;
            _secondDegreeAnimation.SetStartVector(startingVector);
        }

        public void SetStartVector(Vector2 startVector) => _secondDegreeAnimation.SetStartVector(startVector);

        public void SetTargetVector(Vector2 targetVector) => _targetVector = targetVector;

        public void UpdateVector()
        {
            if (_gameObject == null)
            {
                Dispose();
                return;
            }

            var delta = Time.deltaTime;
            _timeSpan -= delta;
            if (_timeSpan <= 0)
            {
                if (_onFinishCallback != null)
                {
                    _onFinishCallback(_gameObject);
                    _onFinishCallback = null;
                }
                if (_disposeOnFinish)
                    Dispose();
            }
            else
            {
                switch (_vectorType)
                {
                    case VectorType.Position:
                        _gameObject.GetComponent<RectTransform>().anchoredPosition = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.SizeDelta:
                        _gameObject.GetComponent<RectTransform>().sizeDelta = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.Scale:
                        _gameObject.GetComponent<RectTransform>().localScale = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                }
            }
        }

        public void Dispose()
        {
            if (_isAlreadyDisposed) return;
            AnimationManager.RemoveFromList(this);
            _isAlreadyDisposed = true;
        }

        public enum VectorType
        {
            Position,
            SizeDelta,
            Scale,
        }
    }
}
