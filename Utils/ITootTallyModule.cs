using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

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
