using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Utils.Helpers;
using UnityEngine;

namespace TootTally
{
    public static class TootTallySettings
    {
        private const string MAIN_MENU_PATH = "MainCanvas/MainMenu";
        private static GameObject _mainMenu;
        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        static public void OnHomeControllerStartAddSettingsPage(HomeController __instance)
        {
            _mainMenu = GameObject.Find("MainCanvas/MainMenu");
            GameObjectFactory.CreateCustomButton(_mainMenu.transform, new Vector2(-1860, -415), new Vector2(60, 250), "<", "TTSettingsButton", delegate
            {
                AnimationManager.AddNewPositionAnimation(_mainMenu, new Vector2(1940, 0), 1.5f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            });
        }
    }
}
