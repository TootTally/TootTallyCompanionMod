using BepInEx.Configuration;
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
        void LoadModule();
        void UnloadModule();
    }
}
