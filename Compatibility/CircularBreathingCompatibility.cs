using System;

namespace TootTally.Compatibility
{
    public static class CircularBreathingCompatibility
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("CircularBreathing");
                return (bool)_enabled;
            }
        }

        public static bool IsActivated
        {
            get
            {
                Type cb = Type.GetType("CircularBreathing.Plugin, CircularBreathing");
                if (cb == null)
                {
                    Plugin.Instance.Log("CircularBreathing.Plugin not found.");
                    return false;
                }
                var cbActivated = cb.GetMethod("get_circularBreathingEnabled");
                var cbConfigEntry = (BepInEx.Configuration.ConfigEntry<bool>) cbActivated.Invoke(null, null);
                return cbConfigEntry.Value;
            }
        }
    }
}