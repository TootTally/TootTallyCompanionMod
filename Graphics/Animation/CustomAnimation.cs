using System;
using TootTally.Utils.Helpers;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace TootTally.Graphics.Animation
{
    public class CustomAnimation
    {
        private GameObject _gameObject;
        private Vector3 _targetVector;
        private float _speedMultiplier, _timeSpan;
        private EasingHelper.SecondOrderDynamics _secondDegreeAnimation;
        private bool _disposeOnFinish;
        private Action<GameObject> _onFinishCallback;
        private VectorType _vectorType;
        private bool _isAlreadyDisposed;

        public CustomAnimation(GameObject gameObject, Vector3 startingVector, Vector3 targetVector, float speedMultiplier,
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

        public void SetStartVector(Vector3 startVector) => _secondDegreeAnimation.SetStartVector(startVector);

        public void SetTargetVector(Vector3 targetVector) => _targetVector = targetVector;

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
                if (_vectorType == VectorType.EulerAngle)
                    _gameObject.transform.eulerAngles = _targetVector;

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
                    case VectorType.TransformScale:
                        _gameObject.transform.localScale = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.TransformPosition:
                        _gameObject.transform.position = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.Position:
                        _gameObject.GetComponent<RectTransform>().anchoredPosition = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.SizeDelta:
                        _gameObject.GetComponent<RectTransform>().sizeDelta = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.Scale:
                        _gameObject.GetComponent<RectTransform>().localScale = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
                        break;
                    case VectorType.EulerAngle:
                        _gameObject.transform.eulerAngles = _secondDegreeAnimation.GetNewVector(_targetVector, delta * _speedMultiplier);
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
            TransformScale,
            TransformPosition,
            Position,
            SizeDelta,
            Scale,
            EulerAngle,
        }
    }
}
