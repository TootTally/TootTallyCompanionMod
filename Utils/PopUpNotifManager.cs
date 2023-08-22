using System.Collections.Generic;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Utils
{
    public class PopUpNotifManager : MonoBehaviour
    {
        private static List<PopUpNotif> _toAddNotificationList;
        private static List<PopUpNotif> _activeNotificationList;
        private static List<PopUpNotif> _toRemoveNotificationList;
        private static GameObject _notifCanvas;
        private static bool IsInitialized;

        private void Awake()
        {
            if (IsInitialized) return;

            _notifCanvas = new GameObject("NotifCanvas");
            Canvas canvas = _notifCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = _notifCanvas.AddComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject.DontDestroyOnLoad(_notifCanvas);
            _toAddNotificationList = new List<PopUpNotif>();
            _activeNotificationList = new List<PopUpNotif>();
            _toRemoveNotificationList = new List<PopUpNotif>();
            IsInitialized = true;
        }

        public static void DisplayNotif(string message, Color textColor, float lifespan = 6f)
        {
            if (!IsInitialized) return;

            if (Plugin.Instance.ShouldDisplayToasts.Value)
            {
                _notifCanvas.SetActive(false);
                _notifCanvas.SetActive(true);//reset the order to make sure its on top
                PopUpNotif notif = GameObjectFactory.CreateNotif(_notifCanvas.transform, "Notification", message, textColor);
                notif.Initialize(lifespan, new Vector2(695, -400));
                _toAddNotificationList.Add(notif);
            }
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

            if (_toAddNotificationList != null && _toAddNotificationList.Count > 0)
            {
                _activeNotificationList.AddRange(_toAddNotificationList);
                OnNotifCountChangeSetNewPosition();
                _toAddNotificationList.Clear();
            }

            if (_activeNotificationList != null)
                _activeNotificationList.ForEach(notif => notif.Update());
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
    }
}
