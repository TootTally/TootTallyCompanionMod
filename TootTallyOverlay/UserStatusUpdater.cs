using HarmonyLib;
using TootTally.Replays;

namespace TootTally.TootTallyOverlay
{
    public static class UserStatusUpdater
    {

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        public static void SetHomeScreenUserStatus()
        {
            UserStatusManager.SetUserStatus(UserStatusManager.UserStatus.MainMenu);
           
        }

        [HarmonyPatch(typeof(CharSelectController), nameof(CharSelectController.Start))]
        [HarmonyPostfix]
        public static void SetCharScreenUserStatus()
        {
            UserStatusManager.SetUserStatus(UserStatusManager.UserStatus.MainMenu);
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void SetLevelSelectUserStatusOnAdvanceSongs()
        {
            UserStatusManager.SetUserStatus(UserStatusManager.UserStatus.BrowsingSongs);
            if (SpectatingManager.IsHost)
                SpectatingManager.SendUserStateToSocket(SpectatingManager.UserState.SelectingSong);
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPostfix]
        public static void SetLevelSelectUserStatus()
        {
            UserStatusManager.ResetTimerAndWakeUpIfIdle();
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.startSong))]
        [HarmonyPostfix]
        public static void SetPlayingUserStatus()
        {
            var status = Replays.ReplaySystemManager.wasPlayingReplay ? UserStatusManager.UserStatus.WatchingReplay : UserStatusManager.UserStatus.Playing;
            UserStatusManager.SetUserStatus(status);
            if (SpectatingManager.IsHost)
                SpectatingManager.SendUserStateToSocket(SpectatingManager.UserState.Playing);
        }

        [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.showPausePanel))]
        [HarmonyPostfix]
        public static void OnResumeSetUserStatus()
        {
            if (SpectatingManager.IsHost)
                SpectatingManager.SendUserStateToSocket(SpectatingManager.UserState.Paused);
        }

        [HarmonyPatch(typeof(PauseCanvasController), nameof(PauseCanvasController.resumeFromPause))]
        [HarmonyPostfix]
        public static void OnPauseSetUserStatus()
        {
            if (SpectatingManager.IsHost)
                SpectatingManager.SendUserStateToSocket(SpectatingManager.UserState.Playing);
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.pauseQuitLevel))]
        [HarmonyPostfix]
        public static void OnQuitSetUserStatus()
        {
            if (SpectatingManager.IsHost)
                SpectatingManager.SendUserStateToSocket(SpectatingManager.UserState.Quitting);
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.pauseRetryLevel))]
        [HarmonyPostfix]
        public static void OnRetryingSetUserStatus()
        {
            if (SpectatingManager.IsHost)
                SpectatingManager.SendUserStateToSocket(SpectatingManager.UserState.Restarting);
        }
    }
}
