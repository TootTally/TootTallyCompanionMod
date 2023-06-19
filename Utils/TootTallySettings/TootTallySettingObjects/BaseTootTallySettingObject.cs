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
        private TootTallySettingPage _page;
        public BaseTootTallySettingObject(string name, TootTallySettingPage page)
        {
            this.name = name;
            _page = page;
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
