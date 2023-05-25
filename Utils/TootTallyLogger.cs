using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mono.Security.X509.X520;

namespace TootTally.Utils
{
    public static class TootTallyLogger
    {
        private const string TOOTTALLY_LOG_FOLDER = "Logs";
        private const string TOOTTALLY_LOG_FILE_NAME = "TootTally.log";

        public static void Initialize()
        {
            var folderPath = Path.Combine(Paths.BepInExRootPath, TOOTTALLY_LOG_FOLDER);
            var logFilePath = Path.Combine(folderPath, TOOTTALLY_LOG_FILE_NAME);
            if (!Directory.Exists(folderPath))
            {
                LogInfo("Couldn't find logs folder, generating folder...");
                Directory.CreateDirectory(folderPath);
            }
            AddLoggerToListener(Plugin.GetLogger());
        }

        internal static void LogInfo(string msg)
        {
            Plugin.GetLogger().LogInfo(msg);
        }

        internal static void DebugModeLog(string msg)
        {
            if (Plugin.Instance.DebugMode.Value)
                Plugin.GetLogger().LogDebug(msg);
        }

        internal static void LogError(string msg)
        {
            Plugin.GetLogger().LogError(msg);
        }

        internal static void LogWarning(string msg)
        {
            Plugin.GetLogger().LogWarning(msg);
        }

        internal static void AddLoggerToListener(ManualLogSource logger)
        {
            ClearOrCreateLogFile(logger.SourceName);
            logger.LogEvent += OnLogEvent;
        }

        internal static void RemoveLoggerFromListener(ManualLogSource logger)
        {
            logger.LogEvent -= OnLogEvent;
        }

        private static void OnLogEvent(object sender, LogEventArgs e)
        {
            var filePath = Path.Combine(Paths.BepInExRootPath, TOOTTALLY_LOG_FOLDER, TOOTTALLY_LOG_FILE_NAME);
            if (!File.Exists(filePath))
                File.Create(filePath);

            File.AppendAllText(filePath, $"[{DateTime.Now:HH:mm:ss}]_({e.Level})_[{e.Source.SourceName}]: {e.Data}\n");
            if ((sender as ManualLogSource) != Plugin.GetLogger())
            {
                var sourceFilePath = Path.Combine(Paths.BepInExRootPath, TOOTTALLY_LOG_FOLDER, e.Source.SourceName + ".log");
                File.AppendAllText(sourceFilePath, $"[{DateTime.Now:HH:mm:ss}]_({e.Level})_[{e.Source.SourceName}]: {e.Data}\n");
            }
        }

        public static void LogInfo(ManualLogSource logger, string msg)
        {
            logger.LogInfo(msg);
        }
        public static void DebugModeLog(ManualLogSource logger, string msg)
        {
            if (Plugin.Instance.DebugMode.Value)
                logger.LogDebug(msg);
        }
        public static void LogError(ManualLogSource logger, string msg)
        {
            logger.LogError(msg);
        }

        public static void LogWarning(ManualLogSource logger, string msg)
        {
            logger.LogWarning(msg);
        }

        public static void ClearOrCreateLogFile(string logFileName)
        {
            var sourceFilePath = Path.Combine(Paths.BepInExRootPath, TOOTTALLY_LOG_FOLDER, logFileName + ".log");
            if (File.Exists(sourceFilePath))
                File.Delete(sourceFilePath);
            File.Create(sourceFilePath);
        }

    }
}
