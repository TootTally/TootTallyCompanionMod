using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TootTally.Utils.TootTallySettings
{
    public abstract class BaseTootTallySettingObject
    {
        public bool isDisposed;
        public string name;
        public bool isInitialized;

        protected TootTallySettingPage _page;
        public BaseTootTallySettingObject(string name, TootTallySettingPage page)
        {
            this.name = name;
            _page = page;
            if (TootTallySettingsManager.isInitialized)
                Initialize();
        }

        public virtual void Initialize()
        {
            isInitialized = true;
        }
        
        public void Remove()
        {
            Dispose();
            isDisposed = true;
            _page.RemoveSettingObjectFromList(this);
        }

        //All gameobjects should be deleted when Dispose is called
        public abstract void Dispose();
    }
}
