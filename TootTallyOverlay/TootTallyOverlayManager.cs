using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TootTally.Graphics.Animation;
using TootTally.Utils.Helpers;
using TootTally.Graphics;
using TootTally.Utils;
using static TootTally.Utils.APIServices.SerializableClass;
using TMPro;
using System.Linq;

namespace TootTally.TootTallyOverlay
{
    public class TootTallyOverlayManager : MonoBehaviour
    {
        private static readonly List<KeyCode> _keyInputList = new() { KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.B, KeyCode.A };
        private static readonly List<KeyCode> _kenoKeys = new List<KeyCode>{KeyCode.UpArrow, KeyCode.UpArrow,
                                       KeyCode.DownArrow, KeyCode.DownArrow,
                                       KeyCode.LeftArrow, KeyCode.RightArrow,
                                       KeyCode.LeftArrow, KeyCode.RightArrow,
                                       KeyCode.B, KeyCode.A};
        private static int _kenoIndex;
        private static bool _isPanelActive;
        private static bool _isInitialized;
        private static bool _isUpdating;
        private static GameObject _overlayCanvas;
        private static CustomAnimation _panelAnimationFG, _panelAnimationBG;

        private static GameObject _overlayPanel;
        private static GameObject _overlayPanelContainer;

        private static RectTransform _containerRect;
        private static TMP_Text _titleText;

        private static List<GameObject> _userObjectList;

        private static bool _showAllSUsers, _showFriends;

        private static float _scrollAcceleration;


        private void Awake()
        {
            if (_isInitialized) return;
            Initialize();
            PopUpNotifManager.DisplayNotif("TromBuddies Panel Initialized!", GameTheme.themeColors.notification.defaultText);
        }

        private static void Initialize()
        {
            _kenoIndex = 0;
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
            gridLayoutGroup.cellSize = new Vector2(380, 120);
            gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;
            _overlayPanelContainer.transform.parent.gameObject.AddComponent<Mask>();
            GameObjectFactory.DestroyFromParent(_overlayPanelContainer.transform.parent.gameObject, "subtitle");
            GameObjectFactory.DestroyFromParent(_overlayPanelContainer.transform.parent.gameObject, "title");
            var text = GameObjectFactory.CreateSingleText(_overlayPanelContainer.transform, "title", "TromBuddies (EARLY ACCESS)", GameTheme.themeColors.leaderboard.text);
            _titleText = text.GetComponent<TMP_Text>();
            var layoutElement = text.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            text.raycastTarget = false;
            text.alignment = TMPro.TextAlignmentOptions.Top;
            text.rectTransform.anchoredPosition = new Vector2(0, 15);
            text.rectTransform.pivot = new Vector2(0, .5f);
            text.rectTransform.sizeDelta = new Vector2(1700, 800);
            text.fontSize = 60f;
            text.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            GameObjectFactory.CreateCustomButton(_overlayPanelContainer.transform.parent, Vector2.zero, new Vector2(60, 60), AssetManager.GetSprite("Close64.png"), "CloseTromBuddiesButton", TogglePanel);

            _overlayPanel.SetActive(false);
            _isPanelActive = false;
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized || Plugin.userInfo == null) return;

            if (Input.GetKeyDown(KeyCode.F2))
            {
                UserStatusManager.ResetTimerAndWakeUpIfIdle();
                TogglePanel();
            }

            if (!_isPanelActive) return;

            _keyInputList.ForEach(key =>
            {
                if (Input.GetKeyDown(key))
                    HandleKeyDown(key);
            });

            if (Input.mouseScrollDelta.y != 0)
                AddScrollAcceleration(Input.mouseScrollDelta.y * 2f);
            UpdateScrolling();
        }

        private static void HandleKeyDown(KeyCode keypressed)
        {
            if (_isUpdating)
            {
                PopUpNotifManager.DisplayNotif("Panel currently updating, be patient!", GameTheme.themeColors.notification.defaultText);
                return;
            }
            switch (keypressed)
            {
                case KeyCode.F3:
                    _showAllSUsers = !_showAllSUsers;
                    PopUpNotifManager.DisplayNotif(_showAllSUsers ? "Showing all users" : "Showing online users", GameTheme.themeColors.notification.defaultText);
                    UpdateUsers();
                    break;
                case KeyCode.F4:
                    _showFriends = !_showFriends;
                    PopUpNotifManager.DisplayNotif(_showFriends ? "Showing friends only" : "Showing non-friend users", GameTheme.themeColors.notification.defaultText);
                    UpdateUsers();
                    break;
                case KeyCode.F5:
                    PopUpNotifManager.DisplayNotif("Forcing refresh...", GameTheme.themeColors.notification.defaultText);
                    UpdateUsers();
                    break;
            }

            if (_kenoKeys.Contains(keypressed))
                if (_kenoIndex != -1 && _kenoKeys[_kenoIndex] == keypressed)
                {
                    _kenoIndex++;
                    if (_kenoIndex >= _kenoKeys.Count)
                        OnKenomiCodeEnter();
                }
                else
                    _kenoIndex = 0;
            else
                _kenoIndex = 0;
        }

        private static void OnKenomiCodeEnter()
        {
            _titleText.text = "BonerBuddies";
            PopUpNotifManager.DisplayNotif("Secret found... ☠", GameTheme.themeColors.notification.defaultText);
            _kenoIndex = -1;
        }

        private static void AddScrollAcceleration(float value)
        {
            _scrollAcceleration -= value / Time.deltaTime;
        }

        private static void UpdateScrolling()
        {
            _containerRect.anchoredPosition = new Vector2(_containerRect.anchoredPosition.x, Math.Max(_containerRect.anchoredPosition.y + (_scrollAcceleration * Time.deltaTime), 0));
            if (_containerRect.anchoredPosition.y <= 0)
                _scrollAcceleration = 0;
            else
                _scrollAcceleration -= (_scrollAcceleration * 10f) * Time.deltaTime; //Abitrary value just so it looks nice / feel nice
        }

        public static void TogglePanel()
        {
            _isPanelActive = !_isPanelActive;
            if (_overlayPanel != null)
            {
                _panelAnimationBG?.Dispose();
                _panelAnimationFG?.Dispose();
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
            {
                _isUpdating = true;
                if (_showFriends && _showAllSUsers)
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetFriendList(OnUpdateUsersResponse));
                else if (_showAllSUsers)
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetAllUsersUpToPageID(3, OnUpdateUsersResponse));
                else if (_showFriends)
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetOnlineFriends(OnUpdateUsersResponse));
                else
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetLatestOnlineUsers(OnUpdateUsersResponse));
            }



        }

        private static void OnUpdateUsersResponse(List<User> users)
        {
            ClearUsers();
            users.ForEach(user =>
            {
                _userObjectList.Add(GameObjectFactory.CreateUserCard(_overlayPanelContainer.transform, user, GetStatusString(user)));
            });
            _isUpdating = false;
        }

        private static string GetStatusString(User user)
        {
            switch (user.status)
            {
                case "Offline":
                    return $"<size=16><color=red>{user.status}</color></size>";

                case "Idle":
                    return $"<size=16><color=yellow>{user.status}</color></size>";

                default:
                    if (user.currently_playing != null)
                        return $"<size=16><color=green>{user.status}\n{user.currently_playing[0].short_name}</color></size>";
                    else
                        return $"<size=16><color=green>{user.status}</color></size>";
            }
        }


        public static void OnAddButtonPress(User user) =>
            Plugin.Instance.StartCoroutine(TootTallyAPIService.AddFriend(user.id, OnFriendResponse));
        public static void OnRemoveButtonPress(User user) =>
            Plugin.Instance.StartCoroutine(TootTallyAPIService.RemoveFriend(user.id, OnFriendResponse));
        public static void OpenUserProfile(int id) => Application.OpenURL($"https://toottally.com/profile/{id}");
        private static void OnFriendResponse(bool value)
        {
            if (value)
                UpdateUsers();
            PopUpNotifManager.DisplayNotif(value ? "Friend list updated." : "Action couldn't be done.", GameTheme.themeColors.notification.defaultText);
        }

        public static void ClearUsers()
        {
            _userObjectList.ForEach(DestroyImmediate);
            _userObjectList.Clear();
        }

        public static void UpdateTheme()
        {
            if (!_isInitialized) return;
            Dispose();
            Initialize();
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
