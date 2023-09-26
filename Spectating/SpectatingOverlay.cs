using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using TootTally.Graphics;
using TootTally.Utils;
using static TootTally.Spectating.SpectatingManager;

namespace TootTally.Spectating
{
    public static class SpectatingOverlay
    {
        private static GameObject _overlayCanvas;
        private static LoadingIcon _loadingIcon;
        private static SpectatingViewerIcon _viewerIcon;
        private static bool _isInitialized;
        private static SocketSpectatorInfo _spectatorInfo;
        private static UserState _currentUserState;

        public static void Initialize()
        {
            _overlayCanvas = new GameObject("TootTallySpectatorOverlayCanvas");
            _overlayCanvas.SetActive(true);
            GameObject.DontDestroyOnLoad(_overlayCanvas);
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

            _viewerIcon = GameObjectFactory.CreateDefaultViewerIcon(_overlayCanvas.transform, "ViewerIcon");

            _isInitialized = true;
        }

        public static void UpdateViewerList(SocketSpectatorInfo spectatorInfo)
        {
            _spectatorInfo = spectatorInfo;
            _viewerIcon?.UpdateViewerCount(spectatorInfo.count);
            UpdateViewIcon();
        }

        public static void SetCurrentUserState(UserState newUserState)
        {
            _currentUserState = newUserState;
            UpdateViewIcon();
        }

        public static void UpdateViewIcon()
        {
            if (!Plugin.Instance.ShowSpectatorCount.Value || _viewerIcon == null || _spectatorInfo == null) return;

            if (IsInLevelSelect && _spectatorInfo.count > 0)
            {
                _viewerIcon.SetAnchorMinMax(new Vector2(.92f, .97f));
                _viewerIcon.Show();
            }
            else if (IsInGameController && _spectatorInfo.count > 0)
            {
                _viewerIcon.SetAnchorMinMax(new Vector2(.03f, .07f));
                _viewerIcon.Show();
            }
            else _viewerIcon.Hide();
        }

        private static bool IsInGameController => _currentUserState == UserState.Playing || _currentUserState == UserState.Paused;
        private static bool IsInLevelSelect => _currentUserState == UserState.SelectingSong;

        public static void ShowViewerIcon() => _viewerIcon?.Show();
        public static void HideViewerIcon() => _viewerIcon?.Hide();

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
