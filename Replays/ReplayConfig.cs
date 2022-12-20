using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TootTally.Replays
{
    public static class ReplayConfig
    {
        private const string CONFIG_NAME = "ReplayConfig.cfg";
        private static ConfigFile _config;
        private static Options _options;
        public static ConfigEntry<string>[] ConfigEntryReplayFileNameArray;
        static ReplayConfig()
        {
            string configPath = Path.Combine(Paths.BepInExRootPath, "config/");
            _config = new ConfigFile(configPath + CONFIG_NAME, true);
            ConfigEntryReplayFileNameArray = new ConfigEntry<string>[5];
        }
        public static void ReadConfig(string songNameAndHash)
        {
            _options = new Options()
            {
                ReplayFileName1 = _config.Bind(songNameAndHash, nameof(_options.ReplayFileName1), "NA"),
                ReplayFileName2 = _config.Bind(songNameAndHash, nameof(_options.ReplayFileName2), "NA"),
                ReplayFileName3 = _config.Bind(songNameAndHash, nameof(_options.ReplayFileName3), "NA"),
                ReplayFileName4 = _config.Bind(songNameAndHash, nameof(_options.ReplayFileName4), "NA"),
                ReplayFileName5 = _config.Bind(songNameAndHash, nameof(_options.ReplayFileName5), "NA"),
            };
            ConfigEntryReplayFileNameArray[0] = _options.ReplayFileName1;
            ConfigEntryReplayFileNameArray[1] = _options.ReplayFileName2;
            ConfigEntryReplayFileNameArray[2] = _options.ReplayFileName3;
            ConfigEntryReplayFileNameArray[3] = _options.ReplayFileName4;
            ConfigEntryReplayFileNameArray[4] = _options.ReplayFileName5;
        }

        public static void SaveToConfig(string songFileName)
        {
            for (int i = ConfigEntryReplayFileNameArray.Length - 1; i > 0; i--)
            {
                ConfigEntryReplayFileNameArray[i].Value = ConfigEntryReplayFileNameArray[i - 1].Value;
            }
            ConfigEntryReplayFileNameArray[0].Value = songFileName;
        }



        private class Options
        {
            public ConfigEntry<string> ReplayFileName1 { get; set; }
            public ConfigEntry<string> ReplayFileName2 { get; set; }
            public ConfigEntry<string> ReplayFileName3 { get; set; }
            public ConfigEntry<string> ReplayFileName4 { get; set; }
            public ConfigEntry<string> ReplayFileName5 { get; set; }
        }
    }

}
