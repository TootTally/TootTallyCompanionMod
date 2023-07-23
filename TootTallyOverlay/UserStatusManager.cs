﻿using System;
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
        private static UserStatus _currentStatus, _lastStatus;
        private static int _heartbeatCount;
        private static bool _isInitialized;


        private void Awake()
        {
            _currentStatus = UserStatus.NotConnected;
            ResetTimer(false);
            _isInitialized = true;
            _heartbeatCount = 0;
        }

        private void Update()
        {
            if (_currentStatus == UserStatus.NotConnected || !_isInitialized) return;

            _timer -= Time.deltaTime;
            if (_isTimerReady && _timer < 0)
                OnTimerEnd();
        }

        public static void SetUserStatus(UserStatus newStatus)
        {
            if (!_isInitialized) return;

            _currentStatus = newStatus;
            OnTimerEnd();
        }

        private static void OnTimerEnd()
        {
            _isTimerReady = false;
            Plugin.Instance.StartCoroutine(TootTallyAPIService.SendUserStatus((int)_currentStatus, OnHeartBeatRequestResponse));
        }

        public static void OnHeartBeatRequestResponse()
        {
            ResetTimer();
            HandleHeartBeatCounter();
            TootTallyOverlayManager.UpdateUsers();
        }

        public static void HandleHeartBeatCounter()
        {
            if (ShouldResetCounter())
                _heartbeatCount = 0;
            else
            {
                _heartbeatCount++;
                if (_heartbeatCount >= 60)
                    SetUserStatus(UserStatus.Idle);
            }
            _lastStatus = _currentStatus;
        }

        public static void ResetTimer()
        {
            _timer = DEFAULT_TIMER_VALUE;
            _isTimerReady = true;
        }

        public static bool ShouldResetCounter() => _lastStatus != _currentStatus || _currentStatus == UserStatus.Playing || _currentStatus == UserStatus.WatchingReplay;


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
            Playing = 4,
            WatchingReplay = 5,
            Spectating = 6,
            NotConnected,
        }
    }
}
