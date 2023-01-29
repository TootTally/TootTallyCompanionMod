using System;
using System.Collections;
using HarmonyLib;
using TootTally.Discord.Core;

namespace TootTally.Discord
{
    public static class DiscordRPC
    {
        public static string[] Statuses = { "Main Menu", "Choosing a song", "Tooting up a storm", "Watching a replay", "Celebrating a successful play" };
        private const long clientId = 1067808791330029589;
        private static ActivityManager _actMan;
        private static Core.Discord _discord;
        private static Activity _act;
        private static string _username;

        private static void InitRPC()
        {
            try
            {
                _discord = new Core.Discord(clientId, (ulong)CreateFlags.NoRequireDiscord);
                _discord.SetLogHook(LogLevel.Error, (level, message) => Plugin.LogError($"[{level.ToString()}] {message}"));
                _actMan = _discord.GetActivityManager();
            }
            catch (Exception) { }

        }

        private static void SetActivity(GameStatus status)
        {
            _act = new Activity
            {
                Details = Statuses[((int)status)],
                Assets = { LargeImage = "toottallylogo", LargeText = $"{_username}" },
            };
            return;
        }

        private static void SetActivity(GameStatus status, string message)
        {
            _act = new Activity
            {
                State = message,
                Details = Statuses[((int)status)],
                Assets = { LargeImage = "toottallylogo", LargeText = $"{_username}" },
            };
        }

        private static void SetActivity(GameStatus status, long startTime, string songName, string artist)
        {
            _act = new Activity
            {
                State = $"{artist} - {songName}",
                Details = Statuses[((int)status)],
                Timestamps = { Start = startTime },
                Assets = { LargeImage = "toottallylogo", LargeText = $"{_username}" },
            };
        }

        [HarmonyPatch(typeof(SaveSlotController), nameof(SaveSlotController.Start))]
        [HarmonyPostfix]
        public static void InitializeOnStartup()
        {
            if (_discord == null)
            {
                InitRPC();
                _username = "Picking a save...";
            }

            SetActivity(GameStatus.MainMenu);
        }

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        public static void SetHomeScreenRP()
        {
            if (_discord == null) InitRPC();
            SetActivity(GameStatus.MainMenu);
        }

        [HarmonyPatch(typeof(CharSelectController), nameof(CharSelectController.Start))]
        [HarmonyPostfix]
        public static void SetCharScreenRP()
        {
            if (_discord == null) InitRPC();
            _username = Plugin.userInfo.username;
            SetActivity(GameStatus.MainMenu);
        }

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        public static void SetLevelSelectRP()
        {
            if (_discord == null) InitRPC();
            _username = Plugin.userInfo.username;
            SetActivity(GameStatus.LevelSelect);
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.startSong))]
        [HarmonyPostfix]
        public static void SetPlayingRP()
        {
            if (_discord == null) InitRPC();
            GameStatus status = GameStatus.InGame;
            if (Replays.ReplaySystemManager.wasPlayingReplay) status = GameStatus.InReplay;
            SetActivity(status, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), GlobalVariables.chosen_track_data.trackname_long, GlobalVariables.chosen_track_data.artist);
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
        [HarmonyPostfix]
        public static void SetPointScreenRP()
        {
            if (_discord == null) InitRPC();
            SetActivity(GameStatus.PointScreen);
        }

        [HarmonyPatch(typeof(Plugin), nameof(Plugin.Update))]
        [HarmonyPostfix]
        public static void RunDiscordCallbacks()
        {
            if (_discord != null)
            {
                _actMan.UpdateActivity(_act, (result) =>
                {
                    if (result != Result.Ok)
                        Plugin.LogInfo("Discord: Something went wrong: " + result.ToString());
                });
                try
                {
                    _discord.RunCallbacks();
                }
                catch (Exception)
                {
                    _discord.Dispose();
                    _discord = null;
                }
            }
        }
    }
}