using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTally.Graphics;
using TootTally.Utils;
using UnityEngine;

namespace TootTally.TootTallyOverlay
{
    public class UserStatusManager : MonoBehaviour
    {
        private const float DEFAULT_TIMER_VALUE = 5f;
        private static float _timer;
        private static bool _isTimerReady;
        private static UserStatus _currentStatus;



        private void Awake()
        {
            _currentStatus = UserStatus.NotConnected;
            ResetTimer(false);
        }

        private void Update()
        {
            if (_currentStatus == UserStatus.NotConnected) return;

            _timer -= Time.deltaTime;
            if (_isTimerReady && _timer < 0)
                OnTimerEnd();
        }

        public static void SetUserStatus(UserStatus newStatus)
        {
            _currentStatus = newStatus;
            OnTimerEnd();
        }

        private static void OnTimerEnd()
        {
            _isTimerReady = false;
            Plugin.Instance.StartCoroutine(TootTallyAPIService.SendUserStatus((int)_currentStatus, ResetTimer));
            //PopUpNotifManager.DisplayNotif("Timer ended, sending heartbeat...", GameTheme.themeColors.notification.defaultText);
            //ResetTimer();
        }

        public static void ResetTimer()
        {
            _timer = DEFAULT_TIMER_VALUE;
            _isTimerReady = true;
        }
        public static void ResetTimer(bool autoStart)
        {
            _timer = DEFAULT_TIMER_VALUE;
            _isTimerReady = autoStart;
        }

        public static void StartTimer() => _isTimerReady = true;


        public enum UserStatus
        {
            Online = 0, //if none of the under, default to this
            Idle = 1,
            MainMenu = 2,
            BrowsingSongs = 3,
            NotConnected = 4,
        }
    }
}
