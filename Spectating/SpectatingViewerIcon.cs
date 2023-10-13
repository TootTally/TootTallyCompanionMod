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
        private GameObject _bubble;
        private TMP_Text _bubbleText;
        private bool _isActive;
        private int _lastCount;
        public bool isInitialized;

        public SpectatingViewerIcon(Transform canvasTransform, Vector2 position, Vector2 size, string name)
        {
            _imageHolder = GameObjectFactory.CreateImageHolder(canvasTransform, position, size, AssetManager.GetSprite("SpectatorIcon.png"), name);
            _imageHolder.GetComponent<Image>().color = new Color(1, 1, 1, 0.8f);
            _imageHolder.AddComponent<Outline>();
            _imageHolder.SetActive(false);

            var bHandler = _imageHolder.AddComponent<BubblePopupHandler>();
            _bubble = GameObjectFactory.CreateBubble(new Vector2(300, 300), "ViewerListBubble", "PlaceHolder", new Vector2(0, 0), 6, true, 18);
            bHandler.Initialize(_bubble, false);
            _bubbleText = _bubble.transform.Find("Window Body/BubbleText").GetComponent<TMP_Text>();
            _bubbleText.lineSpacing = 40f;

            _imageRect = _imageHolder.GetComponent<RectTransform>();
            SetAnchorMinMax(new Vector2(.03f, .07f));
            _imageRect.anchoredPosition = new Vector2(7, 0); //slight offset to the right cause cant do more that 0.0X precision on anchorMax/Min

            _text = GameObjectFactory.CreateSingleText(_imageHolder.transform, name, "0", Color.white);
            _text.fontSize = 24;
            _isActive = false;
            _lastCount = 0;
        }

        public void UpdateViewerList(SocketSpectatorInfo specInfo)
        {
            if (specInfo == null)
            {
                _lastCount = 0;
                return;
            }

            if (_lastCount != specInfo.count)
            {
                _lastCount = specInfo.count;
                _text.text = specInfo.count.ToString();
                OnViewerCountChange();
            }

            _bubbleText.text = GetBubbleStringFromSpecInfo(specInfo);
        }

        public void OnViewerCountChange()
        {
            //MaybeAnimation
        }

        private string GetBubbleStringFromSpecInfo(SocketSpectatorInfo specInfo) => string.Concat(specInfo.spectators.Select(name => name + "\n"));

        public void SetAnchorMinMax(Vector2 anchor) => _imageRect.anchorMax = _imageRect.anchorMin = anchor;
        public void SetBubblePivot(Vector2 pivot) => _bubble.GetComponent<RectTransform>().pivot = pivot;

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
