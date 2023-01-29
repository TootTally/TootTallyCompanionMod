using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SimpleJSON;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TootTally.Replays;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Playables;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using TootTally.Graphics;
using TootTally.Utils;

namespace TootTally.CustomLeaderboard
{
    public static class GlobalLeaderboardManager
    {
        private static bool _hasLeaderboardFinishedLoading;

        private static GlobalLeaderboard globalLeaderboard;

        #region HarmonyPatches

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void OnLevelSelectControllerStartPostfix(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            _hasLeaderboardFinishedLoading = false;
            globalLeaderboard = new GlobalLeaderboard();
            globalLeaderboard.Initialize(__instance);

            globalLeaderboard.UpdateLeaderboard(___alltrackslist, OnUpdateLeaderboardCallback);
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.populateScores))]
        [HarmonyPrefix]
        static bool DontPopulateBaseGameLeaderboard() => false;

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Update))]
        [HarmonyPostfix]
        static void UpdateLoadingSwirlyAnimationOnLevelSelectControllerUpdatePostfix()
        {
            if (globalLeaderboard == null) return;

            if (!_hasLeaderboardFinishedLoading)
                globalLeaderboard.UpdateLoadingSwirlyAnimation();

            globalLeaderboard.UpdateRaycastHitList();

            if (globalLeaderboard.IsMouseOver() && Input.mouseScrollDelta.y != 0)
                globalLeaderboard.AddScrollAcceleration(Input.mouseScrollDelta.y);

            if (globalLeaderboard.IsScrollAccelerationNotNull())
                globalLeaderboard.UpdateScrolling();

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
        static bool OnClickNextSkipIfScrollWheelUsed() => ShouldScrollSongs(); //NO SCROLLING WOO
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPrev))]
        [HarmonyPrefix]
        static bool OnClickBackSkipIfScrollWheelUsed() => ShouldScrollSongs(); //NO SCROLLING WOO
        private static bool ShouldScrollSongs() => !globalLeaderboard.IsMouseOver();
        #endregion

        #region update
        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.advanceSongs))]
        [HarmonyPostfix]
        static void UpdateLeaderboardOnAdvanceSongsPostfix(List<SingleTrackData> ___alltrackslist, LevelSelectController __instance)
        {
            if (_hasLeaderboardFinishedLoading)
            {
                _hasLeaderboardFinishedLoading = false;
                globalLeaderboard.ShowLoadingSwirly();
                globalLeaderboard.HideSlider();
                globalLeaderboard.HideErrorText();
                globalLeaderboard.ClearLeaderboard();
            }

            if (__instance.randomizing) return; //Do nothing if randomizing

            globalLeaderboard.UpdateLeaderboard(___alltrackslist, OnUpdateLeaderboardCallback);
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
                    Plugin.LogError("Unexpected Error during leaderboard Update request");
                    break;
            }

            _hasLeaderboardFinishedLoading = true;
            globalLeaderboard.HideLoadingSwirly();
        }
        #endregion
    }
}
