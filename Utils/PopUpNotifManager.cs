using System.Collections.Concurrent;
using System.Collections.Generic;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils
{
    public class PopUpNotifManager : MonoBehaviour
    {
        private static List<PopUpNotif> _activeNotificationList;
        private static List<PopUpNotif> _toRemoveNotificationList;
        private static ConcurrentQueue<PopUpNotifData> _pendingNotifications;
        private static GameObject _notifCanvas;
        private static bool IsInitialized;

        private void Awake()
        {
            if (IsInitialized) return;

            _notifCanvas = new GameObject("NotifCanvas");
            Canvas canvas = _notifCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1;
            CanvasScaler scaler = _notifCanvas.AddComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject.DontDestroyOnLoad(_notifCanvas);
            _pendingNotifications = new ConcurrentQueue<PopUpNotifData>();
            _activeNotificationList = new List<PopUpNotif>();
            _toRemoveNotificationList = new List<PopUpNotif>();
            IsInitialized = true;
        }

        public static void DisplayNotif(string message, Color textColor, float lifespan = 6f)
        {
            if (!IsInitialized || !Plugin.Instance.ShouldDisplayToasts.Value) return;

            _pendingNotifications.Enqueue(new PopUpNotifData(message, textColor, lifespan));
        }

        public static void DisplayNotif(string message) => DisplayNotif(message, GameTheme.themeColors.notification.defaultText);

        private static void OnNotifCountChangeSetNewPosition()
        {
            int count = 0;
            for (int i = _activeNotificationList.Count - 1; i >= 0; i--)
            {
                _activeNotificationList[i].SetTransitionToNewPosition(new Vector2(695, -400 + (215 * count)));
                count++;
            }
        }

        private void Update()
        {
            if (!IsInitialized) return;

            while (_pendingNotifications != null && _pendingNotifications.Count > 0 && _pendingNotifications.TryDequeue(out PopUpNotifData notifData))
            {
                var notif = GameObjectFactory.CreateNotif(_notifCanvas.transform, "Notification", notifData.message, notifData.textColor);
                notif.Initialize(notifData.lifespan, new Vector2(695, -400));
                notif.gameObject.SetActive(true);
                _activeNotificationList.Add(notif);
                OnNotifCountChangeSetNewPosition();
            }

            _activeNotificationList?.ForEach(notif => notif.Update());
            
            if (_toRemoveNotificationList != null && _toRemoveNotificationList.Count > 0)
            {
                foreach (PopUpNotif notif in _toRemoveNotificationList)
                {
                    _activeNotificationList.Remove(notif);
                    GameObject.Destroy(notif.gameObject);
                }
                OnNotifCountChangeSetNewPosition();
                _toRemoveNotificationList.Clear();
            }

        }

        public static void QueueToRemovedFromList(PopUpNotif notif) => _toRemoveNotificationList.Add(notif);

        private class PopUpNotifData
        {
            public PopUpNotifData(string message, Color textColor, float lifespan)
            {
                this.message = message;
                this.textColor = textColor;
                this.lifespan = lifespan;
            }

            public string message { get; set; }
            public Color textColor { get; set; }
            public float lifespan { get; set; }
        }
    }
}
