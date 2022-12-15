using BepInEx;
using HarmonyLib;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TootTally
{
    public static class ReplaySystemJson
    {
        private static List<int[]> replayData = new List<int[]>();


        #region ReplayRecorder
        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void StartReplayRecorder(GameController __instance)
        {
            replayData.Clear();
            Plugin.LogInfo("Started recording replay");
        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
        [HarmonyPostfix]
        public static void StopReplayRecorder(PointSceneController __instance)
        {
            SaveReplayToFile(__instance);
            Plugin.LogInfo("Replay recording finished");
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        [HarmonyPostfix]
        public static void RecordReplay(GameController __instance)
        {
            UInt32 timeDelta = (UInt32)Mathf.FloorToInt(Time.deltaTime * 1000);
            int pointerPos = (int)Math.Round(__instance.pointer.transform.localPosition.y * 100f); //times 100 and convert to int for 2 decimal precision
            bool isTooting = __instance.noteplaying;
            replayData.Add(new int[] { (int)timeDelta, pointerPos, isTooting ? 1 : 0 });
        }


        public static void SaveReplayToFile(PointSceneController __instance)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            // Create Replays directory in case it doesn't exist
            if (!Directory.Exists(replayDir)) Directory.CreateDirectory(replayDir);

            string username = "TestUser";
            DateTimeOffset currentDateTime = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            string currentDateTimeUnix = currentDateTime.ToUnixTimeSeconds().ToString();
            string replayFilename = $"{username} - {currentDateTimeUnix}";

            var replayJson = new JSONObject();
            replayJson["username"] = username;
            replayJson["date"] = currentDateTimeUnix;
            var replayFrameData = new JSONArray();
            OptimizeReplayData(ref replayData);
            replayData.ForEach(frame =>
            {
                var frameDataJsonArray = new JSONArray();
                foreach (var frameData in frame)
                    frameDataJsonArray.Add(frameData);
                replayFrameData.Add(frameDataJsonArray);
            });
            replayJson["replaydata"] = replayFrameData;

            File.WriteAllText(replayDir + replayFilename, replayJson.ToString());
        }

        public static void OptimizeReplayData(ref List<int[]> rawReplayData)
        {
            Plugin.LogInfo("Optimizing Replay...");

            //Look for matching position && tooting values and merge their deltas into one frame
            for (int i = 0; i < rawReplayData.Count - 1; i++)
            {
                for (int j = i + 1; j < rawReplayData.Count && rawReplayData[i][1] == rawReplayData[j][1] && rawReplayData[i][2] == rawReplayData[j][2];)
                {
                    rawReplayData[i][0] += rawReplayData[j][0];
                    rawReplayData.Remove(rawReplayData[j]);
                }
            }
        }
        #endregion

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void StartReplayPlayer(GameController __instance)
        {
            LoadReplay("TestUser - 1671112263");
            Plugin.LogInfo("Started replay");

        }

        [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
        [HarmonyPostfix]
        public static void StopReplayPlayer(PointSceneController __instance)
        {
            Plugin.LogInfo("Replay finished");
        }

        public static void LoadReplay(string replayFileName)
        {
            string replayDir = Path.Combine(Paths.BepInExRootPath, "Replays/");
            if (!Directory.Exists(replayDir))
            {
                Plugin.LogInfo("Replay folder not found");
                return;
            }

            if (!File.Exists(replayDir + replayFileName))
            {
                Plugin.LogInfo("Replay File not found");
                return;
            }

        }


        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        [HarmonyPostfix]
        public static void PlaybackReplay(GameController __instance)
        {
            UInt32 timeDelta = (UInt32)Mathf.FloorToInt(Time.deltaTime * 1000);
            int pointerPos = (int)Math.Round(__instance.pointer.transform.localPosition.y * 100f); //times 100 and convert to int for 2 decimal precision
            bool isTooting = __instance.noteplaying;
            replayData.Add(new int[] { (int)timeDelta, pointerPos, isTooting ? 1 : 0 });
        }

    }
}
