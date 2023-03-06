using System;

namespace TootTally.Compatibility
{
    public static class AutoTootCompatibility
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("AutoToot");
                return (bool)_enabled;
            }
        }

        public static bool WasAutoUsed
        {
            get
            {
                Type autotoot = Type.GetType("AutoToot.Plugin, AutoToot");
                if (autotoot == null)
                {
                    Plugin.Instance.Log("AutoToot.Plugin not found.");
                    return false;
                }
                var wasautoused = autotoot.GetMethod("get_WasAutoUsed");
                return (bool) wasautoused.Invoke(null, null);
            }
        }
    }
}