using System.Collections.Generic;
using HarmonyLib;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

namespace TootTally.Utils
{
    public static class PopUpNotifManager
    {
        private static List<PopUpNotif> _toAddNotificationList;
        private static List<PopUpNotif> _activeNotificationList;
        private static List<PopUpNotif> _toRemoveNotificationList;
        private static GameObject _notifCanvas;
        private static bool IsInitialized;


        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        public static void Initialize(HomeController __instance)
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
            if (Plugin.Instance.ShouldDisplayToasts.Value)
            {
                PopUpNotif notif = GameObjectFactory.CreateNotif(_notifCanvas.transform, "Notification", message, textColor);
                notif.Initialize(lifespan, new Vector2(695, -400));
                _toAddNotificationList.Add(notif);
            }
        }

        private static void OnNotifCountChangeSetNewPosition()
        {
            int count = 0;
            for (int i = _activeNotificationList.Count - 1; i >= 0; i--)
            {
                _activeNotificationList[i].SetTransitionToNewPosition(new Vector2(695, -400 + (215 * count)));
                count++;
            }
        }

        [HarmonyPatch(typeof(Plugin), nameof(Plugin.Update))]
        [HarmonyPostfix]
        public static void Update()
        {
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
