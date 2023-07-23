using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTally.Discord.Core;
using TootTally.Discord;

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
        public static void SetLevelSelectUserStatus()
        {
            UserStatusManager.SetUserStatus(UserStatusManager.UserStatus.BrowsingSongs);
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.startSong))]
        [HarmonyPostfix]
        public static void SetPlayingUserStatus()
        {
            var status = Replays.ReplaySystemManager.wasPlayingReplay ? UserStatusManager.UserStatus.WatchingReplay : UserStatusManager.UserStatus.Playing; //For some reasons this is inverted
            UserStatusManager.SetUserStatus(status);
        }
    }
}
