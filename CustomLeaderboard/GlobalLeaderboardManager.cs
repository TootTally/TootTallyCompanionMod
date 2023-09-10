using System.Collections.Generic;
using HarmonyLib;
using TootTally.Graphics;
using TootTally.TootTallyOverlay;
using TootTally.Utils;
using UnityEngine;

namespace TootTally.CustomLeaderboard
{
    public static class GlobalLeaderboardManager
    {
        private static bool _hasLeaderboardFinishedLoading;

        private static bool _hasGreetedUser;
        private static GlobalLeaderboard globalLeaderboard;

        #region HarmonyPatches

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void OnLevelSelectControllerStartPostfix(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            _hasLeaderboardFinishedLoading = false;
            if (!Plugin.Instance.ShowLeaderboard.Value) return;

            globalLeaderboard = new GlobalLeaderboard();
            globalLeaderboard.Initialize(__instance);

            globalLeaderboard.UpdateLeaderboard(__instance, ___alltrackslist, OnUpdateLeaderboardCallback);


            if (Plugin.userInfo != null && !_hasGreetedUser)
            {
                _hasGreetedUser = true;
                if (Plugin.userInfo.username != "Guest")
                    PopUpNotifManager.DisplayNotif($"Welcome, {Plugin.userInfo.username}!", GameTheme.themeColors.notification.defaultText, 9f);
                else
                    PopUpNotifManager.DisplayNotif($"Login on TootTally\n<size=16>Put the APIKey in your config file\nto be able to submit scores</size>", GameTheme.themeColors.notification.warningText, 9f);
            }
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.populateScores))]
        [HarmonyPrefix]
        static bool DontPopulateBaseGameLeaderboard() => false;

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Update))]
        [HarmonyPostfix]
        static void UpdateLoadingSwirlyAnimationOnLevelSelectControllerUpdatePostfix()
        {
            if (globalLeaderboard == null) return;

            if (_hasLeaderboardFinishedLoading)
                globalLeaderboard.UpdateStarRatingAnimation();
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.sortTracks))]
        [HarmonyPostfix]
        static void OnTrackSortReloadLeaderboard(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            if (globalLeaderboard != null && globalLeaderboard.HasLeaderboard)
                UpdateLeaderboardOnAdvanceSongsPostfix(___alltrackslist, __instance);
        }


        [HarmonyPatch(typeof(LeaderboardManager), nameof(LeaderboardManager.clickTab))]
        [HarmonyPrefix]
        static bool OverwriteClickLeaderboardTabs(object[] __args)
        {
            if (globalLeaderboard != null && globalLeaderboard.HasLeaderboard)
            {
                int tabIndex = (int)__args[0];
                if (tabIndex == 0)
                    if (Plugin.userInfo.id != 0)
                        globalLeaderboard.OpenUserProfile();
                    else
                        globalLeaderboard.OpenLoginPage();
                else if (tabIndex == 1)
                    globalLeaderboard.OpenSongLeaderboard();
                else if (tabIndex == 2)
                    globalLeaderboard.ScrollToLocalScore();
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickNext))]
        [HarmonyPrefix]
        private static bool OnClickNextSkipIfScrollWheelUsed() => ShouldScrollSongs(); //NO SCROLLING WOO
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPrev))]
        [HarmonyPrefix]
        private static bool OnClickBackSkipIfScrollWheelUsed() => ShouldScrollSongs(); //NO SCROLLING WOO
        private static bool ShouldScrollSongs() => globalLeaderboard == null || (!globalLeaderboard.IsMouseOver || Input.mouseScrollDelta.y == 0f) && !TootTallyOverlayManager.IsPanelActive; //scroll songs if mouse isn't over the leaderboard and you aren't using mousewheel

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.showButtonsAfterRandomizing))]
        [HarmonyPostfix]

        private static void HideTurboButtonAfterRandomizing(LevelSelectController __instance)
        {
            if (globalLeaderboard == null) return;
            __instance.btnturbo.SetActive(false);
            __instance.btnpractice.SetActive(false);
        }
        #endregion

        #region update
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPostfix]
        static void UpdateLeaderboardOnAdvanceSongsPostfix(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            if (_hasLeaderboardFinishedLoading)
            {
                _hasLeaderboardFinishedLoading = false;
                globalLeaderboard?.ShowLoadingSwirly();
                globalLeaderboard?.HideSlider();
                globalLeaderboard?.HideErrorText();
                globalLeaderboard?.ClearLeaderboard();
            }

            if (__instance.randomizing) return; //Do nothing if randomizing

            globalLeaderboard?.UpdateLeaderboard(__instance, ___alltrackslist, OnUpdateLeaderboardCallback);
        }

        private static void OnUpdateLeaderboardCallback(GlobalLeaderboard.LeaderboardState state)
        {
            switch (state)
            {
                case GlobalLeaderboard.LeaderboardState.None:
                    break;

                case GlobalLeaderboard.LeaderboardState.ErrorNoSongHashFound:
                case GlobalLeaderboard.LeaderboardState.ErrorNoLeaderboardFound:
                    globalLeaderboard.ShowErrorText();
                    break;

                case GlobalLeaderboard.LeaderboardState.ReadyToRefresh:
                    globalLeaderboard.RefreshLeaderboard();
                    break;

                case GlobalLeaderboard.LeaderboardState.ErrorUnexpected:
                    TootTallyLogger.LogError("Unexpected Error during leaderboard Update request");
                    break;
            }

            if (state != GlobalLeaderboard.LeaderboardState.SongDataLoaded || state != GlobalLeaderboard.LeaderboardState.SongDataMissing)
            {
                _hasLeaderboardFinishedLoading = true;
                globalLeaderboard.HideLoadingSwirly();
            }

        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPlay))]
        [HarmonyPostfix]
        public static void OnLevelSelectControllerClickPlaySendToSocket()
        {
            globalLeaderboard.SendSongInfoToSocket();
        }

        #endregion
    }
}
