using System;
using TootTally.Utils;

namespace TootTally.Compatibility
{
    public static class HoverTootCompatibility
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("org.crispykevin.hovertoot");
                return (bool)_enabled;
            }
        }

        public static bool DidToggleThisSong
        {
        get
            {
                Type hovertoot = Type.GetType("HoverToot.Plugin, HoverToot");
                if (hovertoot == null)
                {
                    TootTallyLogger.LogInfo("HoverToot.Plugin not found.");
                    return false;
                }
                var washoverused = hovertoot.GetMethod("get_DidToggleThisSong");
                return (bool) washoverused.Invoke(null, null);
            }
        }
    }
}