using BepInEx.Configuration;
using BepInEx.Logging;

namespace TootTally.Utils
{
    public interface ITootTallyModule
    {
        string Name { get; set; }
        bool IsConfigInitialized { get; set; }
        ConfigEntry<bool> ModuleConfigEnabled { get; set; }
        ManualLogSource GetLogger { get; }
        void LoadModule();
        void UnloadModule();
    }
}
