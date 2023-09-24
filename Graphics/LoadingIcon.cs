using TootTally.Graphics.Animation;
using TootTally.Utils.Helpers;
using UnityEngine;

namespace TootTally.Graphics
{
    public class LoadingIcon : MonoBehaviour
    {
        public GameObject iconHolder;
        private bool _isActive;
        private bool _recursiveAnimationActive;
        private CustomAnimation _currentAnimation;

        public LoadingIcon(GameObject iconHolder, bool isActive)
        {
            this.iconHolder = iconHolder;
            _isActive = isActive;
            this.iconHolder.SetActive(isActive);
        }

        public void StartRecursiveAnimation()
        {
            _recursiveAnimationActive = true;
            RecursiveAnimation();
        }

        public void StopRecursiveAnimation(bool immediate)
        {
            _recursiveAnimationActive = false;
            if (immediate && _currentAnimation != null)
            {
                _currentAnimation.Dispose();
                _currentAnimation = null;
            }
        }

        public void RecursiveAnimation()
        {
            _currentAnimation = AnimationManager.AddNewEulerAngleAnimation(iconHolder, new Vector3(0, 0, 359), 0.9f, GetSecondDegreeAnimation(), (sender) =>
            {
                _currentAnimation = AnimationManager.AddNewEulerAngleAnimation(iconHolder, new Vector3(0, 0, 0), 0.9f, GetSecondDegreeAnimation(), (sender) =>
                {
                    if (_recursiveAnimationActive)
                        RecursiveAnimation();
                });
            });
        }

        public void Show()
        {
            iconHolder.SetActive(true);
            _isActive = true;
        }

        public void Hide()
        {
            iconHolder.SetActive(false);
            _isActive = false;
        }

        public void ToggleShow()
        {
            _isActive = !_isActive;
            iconHolder.SetActive(_isActive);
        }

        public bool IsVisible() => _isActive;

        public void Dispose()
        {
            _currentAnimation?.Dispose();
            GameObject.DestroyImmediate(iconHolder);
        }

        public EasingHelper.SecondOrderDynamics GetSecondDegreeAnimation() => new(0.8f, .5f, 1f);
    }
}
