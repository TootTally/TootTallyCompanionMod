using HarmonyLib;
using Steamworks;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTally.Graphics;
using TootTally.Utils;
using UnityEngine;
using UnityEngine.Networking.Match;

namespace TootTally.GameplayModifier
{
    public static class GameModifierManager
    {
        private static bool _isInitialized;
        private static Dictionary<GameModifiers.ModifierType, GameModifierBase> _gameModifierDict;
        private static List<GameModifierBase> _modifierTypesToRemove;
        private static Dictionary<string, GameModifiers.ModifierType> _stringModifierDict;
        private static string _modifiersBackup;

        [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.Start))]
        [HarmonyPostfix]
        static void OnLevelSelectControllerStartPostfix(LevelSelectController __instance)
        {
            if (!_isInitialized) Initialize();

            var hiddenBtn = GameObjectFactory.CreateCustomButton(__instance.fullpanel.transform, new Vector2(350, -150), new Vector2(32, 32), AssetManager.GetSprite("HD.png"), "HiddenButton", delegate { Toggle(GameModifiers.ModifierType.Hidden); }).gameObject;
            var rect = hiddenBtn.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0, 1);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            
            var flashlightBtn = GameObjectFactory.CreateCustomButton(__instance.fullpanel.transform, new Vector2(400, -150), new Vector2(32, 32), AssetManager.GetSprite("FL.png"), "FlashlightButton", delegate { Toggle(GameModifiers.ModifierType.Flashlight); }).gameObject;
            var rect2 = flashlightBtn.GetComponent<RectTransform>();
            rect2.pivot = new Vector2(0, 1);
            rect2.anchorMin = rect2.anchorMax = new Vector2(0, 1);
        }

        public static void Initialize()
        {
            if (_isInitialized) return;

            _gameModifierDict = new();
            _stringModifierDict = new()
            {
                {"HD", GameModifiers.ModifierType.Hidden },
                {"FL", GameModifiers.ModifierType.Flashlight }
            };
            _modifierTypesToRemove = new();
            _modifiersBackup = "None";
            _isInitialized = true;
        }

        private static bool Toggle(GameModifiers.ModifierType modifierType)
        {
            if (!_gameModifierDict.ContainsKey(modifierType))
            {
                Add(modifierType);
                PopUpNotifManager.DisplayNotif($"{modifierType} mod enabled.", GameTheme.themeColors.notification.defaultText);
                return true;
            }
            else
                Remove(modifierType);
            PopUpNotifManager.DisplayNotif($"{modifierType} mod disabled.", GameTheme.themeColors.notification.defaultText);
            return false;
        }


        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void InitializeModifers(GameController __instance)
        {
            if (!_isInitialized) return;

            TootTallyLogger.LogInfo("Active modifiers: " + GetModifiersString());
            foreach (GameModifierBase mod in _gameModifierDict.Values)
            {
                mod.Initialize(__instance);
            }
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        [HarmonyPostfix]
        public static void UpdateModifiers(GameController __instance)
        {
            if (!_isInitialized) return;

            foreach (GameModifierBase mod in _gameModifierDict.Values)
            {
                mod.Update(__instance);
            }
        }

        public static void Remove(GameModifiers.ModifierType modifierType)
        {
            _gameModifierDict.Remove(modifierType);
        }

        public static void ClearAllModifiers()
        {
            _modifierTypesToRemove.AddRange(_gameModifierDict.Values.ToArray());
            _modifierTypesToRemove.Do(mod => mod.Remove());
            _modifierTypesToRemove.Clear();
        }

        public static string GetModifiersString() => _gameModifierDict.Count > 0 ? _gameModifierDict.Values.Join(mod => mod.Name, ",") : "None";

        public static void Add(GameModifiers.ModifierType modifierType)
        {
            if (_gameModifierDict.ContainsKey(modifierType))
            {
                TootTallyLogger.LogInfo($"Modifier of type {modifierType} is already in the modifier list.");
                return;
            }
            switch (modifierType)
            {
                case GameModifiers.ModifierType.Hidden:
                    _gameModifierDict.Add(GameModifiers.ModifierType.Hidden, new GameModifiers.Hidden());
                    break;
                case GameModifiers.ModifierType.Flashlight:
                    _gameModifierDict.Add(GameModifiers.ModifierType.Flashlight, new GameModifiers.Flashlight());
                    break;
            };
        }

        public static void LoadModifiersFromReplayString(string replayModifierString)
        {
            _modifiersBackup = GetModifiersString();
            ClearAllModifiers();
            var replayModifierStringArray = replayModifierString.Split(',');
            if (replayModifierStringArray.Length <= 0 )
            {
                TootTallyLogger.LogInfo("No modifiers detected in replay.");
                return;
            }

            TootTallyLogger.LogInfo($"Loading {replayModifierString} modifiers.");
            foreach (string modName in replayModifierString.Split(','))
            {
                if (_stringModifierDict.ContainsKey(modName))
                    Add(_stringModifierDict[modName]);
            }
        }

        public static void LoadBackedupModifiers() => LoadModifiersFromReplayString(_modifiersBackup);

    }
}
