using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TootTally.Utils.Helpers;

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
            //if (File.Exists(achievementDir + ACHIEVEMENT_FILE_NAME))
                //_achievementList = JsonConvert.DeserializeObject<List<Achievement>>(FileHelper.ReadJsonFromFile(achievementDir, ACHIEVEMENT_FILE_NAME));
            _isInitialized = _achievementList != null;
            _achievementList = new List<Achievement>()
            {
                //Solo Achievement example
                new Achievement()
                {
                    AchievementId = 0,
                    Name = "Tootfinder",
                    Description = "Download and successfully install TootTally",
                    FlavorText = "You've found your way here, no turning back now!",
                    Value = 0 //or null
                },

                //Group Achievement example
                new AchievementGroup()
                {
                    AchievementId = -1, //or null

                    //Threshold values
                    Values = new List<float>()
                    {
                        5,30,100,250,500,1000
                    },

                    //Achievements 
                    Achievements = new List<Achievement>()
                    {
                        new Achievement()
                        {
                            AchievementId = 1,
                            Name = "Slide Beginner",
                            Description = "Play {value,0} rated charts",
                            FlavorText = "You're on the slide to stardom!",
                        },
                        new Achievement()
                        {
                            AchievementId = 2,
                            Name = "Brass Explorer",
                            Description = "Play {value,1} rated charts",
                            FlavorText = "Quite the Musical Dash you’re running!",
                        },
                        new Achievement()
                        {
                            AchievementId = 3,
                            Name = "Melodic Artisan",
                            Description = "Play {value,2} rated charts",
                            FlavorText = "The Harmony Hunter is in the making!",
                        },
                        new Achievement()
                        {
                            AchievementId = 4,
                            Name = "Trombone Virtuoso",
                            Description = "Play {value,3} rated charts",
                            FlavorText = "Even renowned Tromboners look in awe.",
                        },
                        new Achievement()
                        {
                            AchievementId = 5,
                            Name = "Rhythmic Conductor",
                            Description = "Play {value,4} rated charts",
                            FlavorText = "You’ve ascended to Rhythm Heaven!",
                        },
                        new Achievement()
                        {
                            AchievementId = 6,
                            Name = "Legendary Trombone Champion",
                            Description = "Play {value,5} rated charts",
                            FlavorText = "The Trombones salute you! I don’t know how, but they can!",
                        }
                    }
                }
            };

            FileHelper.WriteJsonToFile(achievementDir, ACHIEVEMENT_FILE_NAME, JsonConvert.SerializeObject(_achievementList));

        }

        [Serializable]
        public class Achievement
        {
            public int AchievementId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string FlavorText { get; set; }
            public float Value { get; set; }
        }

        [Serializable]
        public class AchievementGroup : Achievement
        {
            public List<Achievement> Achievements { get; set; }
            public List<float> Values { get; set; }
        }

        [Serializable]
        public class CompletedAchievement
        {
            public List<int> AchivementIds { get; set; }
        }
    }
}
