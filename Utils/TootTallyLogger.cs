using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TootTally.Utils
{
    public static class TootTallyLogger
    {
        private const string TOOTTALLY_LOG_PATH = "Logs/TootTally.log";

        public static void LogInfo(string msg)
        {
            Plugin.GetLogger().LogInfo(msg);
        }
        public static void DebugModeLog(string msg)
        {
            if (Plugin.Instance.DebugMode.Value)
                Plugin.GetLogger().LogInfo(msg);
        }

        public static void LogError(string msg)
        {
            Plugin.GetLogger().LogError(msg);
        }
        public static void LogWarning(string msg)
        {
            Plugin.GetLogger().LogWarning(msg);
        }
    }
}
