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
        private static float elapsedTime;
        private static bool _isTooting, _isReplayPlaying;
        private static int _replayIndex;


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
        [HarmonyPrefix]
        public static void RecordReplay(GameController __instance)
        {
            float timeDelta = Time.deltaTime * 1000 * 1000;
            float pointerPos = __instance.pointer.transform.localPosition.y * 1000f * 10f; //times 1000 * 10 and convert to int for 5 decimal precision
            bool isTooting = __instance.noteplaying;
            replayData.Add(new int[] { (int)timeDelta, (int)pointerPos, isTooting ? 1 : 0 });
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

        /*
           #region ReplayPlayer
           [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
           [HarmonyPrefix]
           public static void StartReplayPlayer(GameController __instance)
           {
               replayData.Clear();
               elapsedTime = 0;
               _isTooting = false;
               _isReplayPlaying = true;
               LoadReplay("TestUser - 1671128751");
               Plugin.LogInfo("Started replay");
           }

           [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.Start))]
           [HarmonyPostfix]
           public static void StopReplayPlayer(PointSceneController __instance)
           {
               _isReplayPlaying = false;
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

               string jsonFile = File.ReadAllText(replayDir + replayFileName);
               var replayJson = JSONObject.Parse(jsonFile);
               foreach (JSONArray jsonArray in replayJson["replaydata"])
                   replayData.Add(new int[] { jsonArray[0], jsonArray[1], jsonArray[2] });
               _replayIndex = 0;

           }


           [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
           [HarmonyPrefix]
           public static void PlaybackReplay(GameController __instance)
           {
               if (!__instance.controllermode) __instance.controllermode = true;

               elapsedTime += Time.deltaTime * 1000 * 1000; // timeDelta
               while(replayData.Count > 0 && _isReplayPlaying && elapsedTime >= replayData[_replayIndex][0])
               {
                   elapsedTime -= replayData[_replayIndex][0];
                   Vector3 pointerPosition = __instance.pointer.transform.localPosition;
                   pointerPosition.y = replayData[_replayIndex][1] / 1000f / 10f;
                   __instance.pointer.transform.localPosition = pointerPosition;
                   if ((replayData[_replayIndex][2] == 1 && !_isTooting) || (replayData[_replayIndex][2] == 0 && _isTooting)) //if tooting state changes
                       ToggleTooting(__instance);
                   _replayIndex++;
               }
           }

           private static void OnTootStateChange(GameController __instance)
           {
               __instance.setPuppetShake(_isTooting);
               __instance.noteplaying = _isTooting;

               if (_isTooting) __instance.playNote();
               else __instance.stopNote();
           }

           private static void ToggleTooting(GameController __instance)
           {
               _isTooting = !_isTooting;
               OnTootStateChange(__instance);
           }

           [HarmonyPatch(typeof(GameController), nameof(GameController.pauseRetryLevel))]
           [HarmonyPrefix]
           static void OnPauseStopReplay(GameController __instance)
           {
               __instance.controllermode = false;
               _isReplayPlaying = false;
               Plugin.LogInfo("Replay finished");
           }
           #endregion
           */
    }
}
