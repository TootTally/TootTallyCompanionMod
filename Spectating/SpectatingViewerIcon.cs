using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.UI;
using static TootTally.Spectating.SpectatingManager;

namespace TootTally.Spectating
{
    public class SpectatingViewerIcon
    {
        private GameObject _imageHolder;
        private RectTransform _imageRect;
        private TMP_Text _text;
        private CustomAnimation _currentRotAnimation, _currentScaleAnimation;
        private bool _isActive;
        private int _lastCount;
        public bool isInitialized;

        public SpectatingViewerIcon(Transform canvasTransform, Vector2 position, Vector2 size, string name)
        {
            _imageHolder = GameObjectFactory.CreateImageHolder(canvasTransform, position, size, AssetManager.GetSprite("SpectatorIcon.png"), name);
            _imageHolder.GetComponent<Image>().color = new Color(1, 1, 1, 0.8f);
            _imageHolder.AddComponent<Outline>();
            _imageHolder.SetActive(false);

            _imageRect = _imageHolder.GetComponent<RectTransform>();
            SetAnchorMinMax(new Vector2(.03f, .07f));
            _imageRect.anchoredPosition = new Vector2(7, 0); //slight offset to the right cause cant do more that 0.0X precision on anchorMax/Min

            _text = GameObjectFactory.CreateSingleText(_imageHolder.transform, name, "0", Color.white);
            _text.fontSize = 24;
            _isActive = false;
            _lastCount = 0;
        }

        public void UpdateViewerCount(int count)
        {
            if (_lastCount == count) return;

            _lastCount = count;
            _text.text = count.ToString();
            OnViewerCountChange();
        }

        public void OnViewerCountChange()
        {
            //MaybeAnimation
        }

        public void SetAnchorMinMax(Vector2 anchor) => _imageRect.anchorMax = _imageRect.anchorMin = anchor;

        private void SetRectToDefault()
        {
            _imageHolder.transform.eulerAngles = new Vector3(0, 0, 260);
            _imageHolder.transform.localScale = Vector2.zero;
        }

        private void SetRectToFinal()
        {
            _imageHolder.transform.eulerAngles = new Vector3(0, 0, 0);
            _imageHolder.transform.localScale = Vector3.one;
        }

        public void Show()
        {
            if (_isActive) return;
            _imageHolder.SetActive(true);
            _currentRotAnimation?.Dispose();
            _currentScaleAnimation?.Dispose();
            SetRectToDefault();

            _isActive = true;
            _currentRotAnimation = AnimationManager.AddNewEulerAngleAnimation(_imageHolder, new Vector3(0, 0, 0), 1.5f, GetSecondDegreeAnimation());
            _currentScaleAnimation = AnimationManager.AddNewScaleAnimation(_imageHolder, Vector3.one, 1.5f, GetSecondDegreeAnimation(), sender => OnFinishAnimating());
        }

        public void Hide()
        {
            if (!_isActive) return;
            _currentRotAnimation?.Dispose();
            _currentScaleAnimation?.Dispose();
            SetRectToFinal();

            _isActive = false;
            _currentRotAnimation = AnimationManager.AddNewEulerAngleAnimation(_imageHolder, new Vector3(0, 0, -260), 1.5f, GetSecondDegreeAnimation());
            _currentScaleAnimation = AnimationManager.AddNewScaleAnimation(_imageHolder, Vector2.zero, 1.5f, GetSecondDegreeAnimation(), sender => { OnFinishAnimating(); _imageHolder.SetActive(false); });

        }

        public void OnFinishAnimating()
        {
            _currentRotAnimation = null;
            _currentScaleAnimation = null;
        }

        public void ToggleShow()
        {
            _isActive = !_isActive;
            _imageHolder.SetActive(_isActive);
        }

        public EasingHelper.SecondOrderDynamics GetSecondDegreeAnimation() => new(2.2f, .65f, 1f);
    }
}
