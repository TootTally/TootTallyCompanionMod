using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using TootTally.Graphics;
using TootTally.Utils;

namespace TootTally.Spectating
{
    public static class SpectatingOverlay
    {
        private static GameObject _overlayCanvas;
        private static LoadingIcon _loadingIcon;
        private static bool _isInitialized;
        public static void Initialize()
        {
            _overlayCanvas = new GameObject("TootTallySpectatorOverlayCanvas");
            _overlayCanvas.SetActive(true);
            Canvas canvas = _overlayCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 2;
            CanvasScaler scaler = _overlayCanvas.AddComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            _loadingIcon = GameObjectFactory.CreateLoadingIcon(_overlayCanvas.transform, Vector2.zero, new Vector2(128, 128), AssetManager.GetSprite("icon.png"), false, "SpectatorLoadingSwirly");
            var rect = _loadingIcon.iconHolder.GetComponent<RectTransform>();
            rect.anchorMax = rect.anchorMin = new Vector2(.9f, .1f);
            _isInitialized = true;
        }

        public static void Update()
        {

        }

        public static void ShowLoadingIcon()
        {
            _loadingIcon?.StartRecursiveAnimation();
            _loadingIcon?.Show();
        }

        public static void HideLoadingIcon()
        {
            _loadingIcon?.Hide();
            _loadingIcon?.StopRecursiveAnimation(true);
        }
        public static bool IsLoadingIconVisible() => _loadingIcon.IsVisible();
    }
}
