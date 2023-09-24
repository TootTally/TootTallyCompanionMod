using HarmonyLib;
using TootTally.Spectating;

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
            if (SpectatingManager.IsSpectating)
                UserStatusManager.SetUserStatus(UserStatusManager.UserStatus.Spectating);
            else
            {
                var status = Replays.ReplaySystemManager.wasPlayingReplay ? UserStatusManager.UserStatus.WatchingReplay : UserStatusManager.UserStatus.Playing;
                UserStatusManager.SetUserStatus(status);
            }

        }
    }
}
