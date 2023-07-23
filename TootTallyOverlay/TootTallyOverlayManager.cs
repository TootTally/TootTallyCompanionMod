using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using TootTally.Graphics.Animation;
using TootTally.Utils.Helpers;
using TootTally.Graphics;
using TootTally.Utils;
using System.Runtime.CompilerServices;
using static TootTally.Utils.APIServices.SerializableClass;

namespace TootTally.TootTallyOverlay
{
    public class TootTallyOverlayManager : MonoBehaviour
    {
        private static bool _isPanelActive;
        private static bool _isInitialized;
        private static GameObject _overlayCanvas;
        private static CustomAnimation _panelAnimationFG, _panelAnimationBG;

        private static GameObject _overlayPanel;
        private static GameObject _overlayPanelContainer;

        private static RectTransform _containerRect;

        private static List<GameObject> _userObjectList;

        private static bool _showAllSUsers;

        private void Awake()
        {
            if (_isInitialized) return;

            _overlayCanvas = new GameObject("TootTallyOverlayCanvas");
            Canvas canvas = _overlayCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = _overlayCanvas.AddComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _userObjectList = new List<GameObject>();

            GameObject.DontDestroyOnLoad(_overlayCanvas);

            _overlayPanel = GameObjectFactory.CreateOverlayPanel(_overlayCanvas.transform, Vector2.zero, new Vector2(1700, 900), 20f, "BonerBuddiesOverlayPanel");
            _overlayPanelContainer = _overlayPanel.transform.Find("FSLatencyPanel/LatencyFG/MainPage").gameObject;
            _overlayPanel.transform.Find("FSLatencyPanel/LatencyFG").localScale = Vector2.zero;
            _overlayPanel.transform.Find("FSLatencyPanel/LatencyBG").localScale = Vector2.zero;
            _containerRect = _overlayPanelContainer.GetComponent<RectTransform>();
            _containerRect.anchoredPosition = Vector2.zero;
            _containerRect.sizeDelta = new Vector2(1700, 700);
            GameObject.DestroyImmediate(_overlayPanelContainer.GetComponent<VerticalLayoutGroup>());
            var gridLayoutGroup = _overlayPanelContainer.AddComponent<GridLayoutGroup>();
            gridLayoutGroup.padding = new RectOffset(20, 20, 20, 20);
            gridLayoutGroup.spacing = new Vector2(5, 5);
            gridLayoutGroup.cellSize = new Vector2(240, 80);
            gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            _overlayPanelContainer.transform.parent.gameObject.AddComponent<Mask>();
            GameObjectFactory.DestroyFromParent(_overlayPanelContainer.transform.parent.gameObject, "subtitle");
            GameObjectFactory.DestroyFromParent(_overlayPanelContainer.transform.parent.gameObject, "title");
            var text = GameObjectFactory.CreateSingleText(_overlayPanelContainer.transform.parent, "title", "BonerBuddies (BETA TEST)", GameTheme.themeColors.leaderboard.text);
            text.raycastTarget = false;
            text.alignment = TMPro.TextAlignmentOptions.Top;
            text.rectTransform.pivot = new Vector2(0, .5f);
            text.rectTransform.sizeDelta = new Vector2(1700, 800);
            text.fontSize = 60f;
            text.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            _overlayPanel.SetActive(false);
            _isPanelActive = false;
            _isInitialized = true;
            PopUpNotifManager.DisplayNotif("BonerBuddies Panel Initialized!", GameTheme.themeColors.notification.defaultText);
        }

        private void Update()
        {
            if (!_isInitialized) return;

            if (Input.GetKeyDown(KeyCode.F2))
                TogglePanel();

            if (Input.GetKeyDown(KeyCode.F3))
            {
                _showAllSUsers = !_showAllSUsers;
                UpdateUsers();
            }

            if (Input.GetKeyDown(KeyCode.F5))
                UpdateUsers();

            if (_isPanelActive && Input.mouseScrollDelta.y != 0)
                _containerRect.anchoredPosition = new Vector2(_containerRect.anchoredPosition.x, _containerRect.anchoredPosition.y + Input.mouseScrollDelta.y * 35f);
        }

        public static void TogglePanel()
        {
            _isPanelActive = !_isPanelActive;
            if (_overlayPanel != null)
            {
                if (_panelAnimationBG != null)
                    _panelAnimationBG.Dispose();
                if (_panelAnimationFG != null)
                    _panelAnimationFG.Dispose();
                var targetVector = _isPanelActive ? Vector2.one : Vector2.zero;
                var animationTime = _isPanelActive ? 1f : 0.45f;
                var secondDegreeAnimationFG = _isPanelActive ? new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f) : new EasingHelper.SecondOrderDynamics(3.2f, 1f, 0.25f);
                var secondDegreeAnimationBG = _isPanelActive ? new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f) : new EasingHelper.SecondOrderDynamics(3.2f, 1f, 0.25f);
                _panelAnimationFG = AnimationManager.AddNewScaleAnimation(_overlayPanel.transform.Find("FSLatencyPanel/LatencyFG").gameObject, targetVector, animationTime, secondDegreeAnimationFG);
                _panelAnimationBG = AnimationManager.AddNewScaleAnimation(_overlayPanel.transform.Find("FSLatencyPanel/LatencyBG").gameObject, targetVector, animationTime, secondDegreeAnimationBG, (sender) =>
                {
                    if (!_isPanelActive)
                        _overlayPanel.SetActive(_isPanelActive);
                });
                if (_isPanelActive)
                {
                    _overlayPanel.SetActive(_isPanelActive);
                    UpdateUsers();
                }
                else
                    ClearUsers();
            }
        }

        public static void UpdateUsers()
        {
            if (_isPanelActive)
                if (_showAllSUsers)
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetAllUsers(OnUpdateUsersResponses));
                else
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetLatestOnlineUsers(OnUpdateUsersResponses));
        }

        private static void OnUpdateUsersResponses(List<User> users)
        {
            ClearUsers();
            users.ForEach(user =>
            {
                _userObjectList.Add(GameObjectFactory.CreateCustomButton(_overlayPanelContainer.transform, Vector2.zero, new Vector2(30, 60), $"{user.username}\n{GetStatusString(user.status)}", $"{user.username}Button", () => PopUpNotifManager.DisplayNotif($"id: {user.id}\nname: {user.username}\ntt: {user.tt}\n#{user.rank}\nstatus: {user.status}", GameTheme.themeColors.notification.defaultText)).gameObject);
            });
        }

        private static string GetStatusString(string status)
        {
            switch (status)
            {
                case "Offline":
                    return $"<size=16><color=red>{status}</color></size>";

                case "Idle":
                    return $"<size=16><color=yellow>{status}</color></size>";

                default:
                    return $"<size=16><color=green>{status}</color></size>";
            }
        }

        public static void ClearUsers()
        {
            _userObjectList.ForEach(DestroyImmediate);
            _userObjectList.Clear();
        }

        public static void Dispose()
        {
            if (!_isInitialized) return; //just in case too

            GameObject.DestroyImmediate(_overlayCanvas);
            GameObject.DestroyImmediate(_overlayPanel);
            _isInitialized = false;
        }
    }
}
