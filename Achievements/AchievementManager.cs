using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TootTally.Replays.NewReplaySystem;
using TootTally.Utils.APIServices;
using TootTally.Utils.Helpers;
using TootTally.Utils;

namespace TootTally.Achievements
{
    //WIP UNUSED I GUESS
    public static class AchievementManager
    {
        private const string ACHIEVEMENT_FILE_NAME = "TootTallyAchievements.json";
        private static List<Achievement> _achievementList;
        private static bool _isInitialized;

        public static void Initialize()
        {
            if (_isInitialized) return;

            string achievementDir = Path.Combine(Paths.BepInExRootPath, "config/");
            if (File.Exists(achievementDir + ACHIEVEMENT_FILE_NAME))
                _achievementList = JsonConvert.DeserializeObject<List<Achievement>>(FileHelper.ReadJsonFromFile(achievementDir, ACHIEVEMENT_FILE_NAME));
            _isInitialized = _achievementList != null;
        }

        [Serializable]
        public class Achievement
        {
            public int AchievementId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        [Serializable]
        public class CompletedAchievement
        {
            public List<int> AchivementIds { get; set; }
        }
    }
}
