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
                var cbPlugin = BepInEx.Bootstrap.Chainloader.PluginInfos["CircularBreathing"];
                var cbConfig = cbPlugin.Instance.Config;
                return (bool) cbConfig["General", "Circular Breathing Enabled"].BoxedValue;
            }
        }
    }
}