using HarmonyLib;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTally.Utils;
using UnityEngine.Networking.Match;

namespace TootTally.GameplayModifier
{
    public static class GameModifierManager
    {
        private static bool _isInitialized;
        private static List<GameModifierBase> _gameModifierList;


        public static void Initialize()
        {
            if (_isInitialized) return;

            _gameModifierList = new List<GameModifierBase>();
            _isInitialized = true;
        }

        public static void InitializeModifers(GameController __instance)
        {
            if (!_isInitialized) return;

            foreach (GameModifierBase mod in _gameModifierList)
            {
                mod.Initialize(__instance);
            }
        }

        public static void UpdateModifiers(GameController __instance)
        {
            if (!_isInitialized) return;

            foreach (GameModifierBase mod in _gameModifierList)
            {
                mod.Update(__instance);
            }
        }

        public static void Remove(GameModifierBase modifier)
        {
            _gameModifierList.Remove(modifier);
        }

        public static string GetModifiersString() => _gameModifierList.Count > 0 ? _gameModifierList.Join(mod => mod.Name, ",") : "None";

        public static GameModifierBase Add(GameModifierBase modifier)
        {
            if (_gameModifierList.Any(mod => mod.GetType() == modifier.GetType()))
            {
                TootTallyLogger.LogInfo($"Modifier of type {modifier.Name} is already in the modifier list.");
                return null;
            }
            _gameModifierList.Add(modifier); return modifier;
        }
    }
}
