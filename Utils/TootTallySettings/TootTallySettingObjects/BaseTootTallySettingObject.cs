using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TootTally.Utils.TootTallySettings.TootTallySetting
{
    public abstract class BaseTootTallySettingObject
    {
        public string name;
        private TootTallySettingPage _page;
        public BaseTootTallySettingObject(string name, TootTallySettingPage page)
        {
            this.name = name;
            _page = page;
        }

        public void Remove()
        {
            _page.RemoveSettingObjectFromList(this);
            Dispose();
        }

        //All gameobjects should be deleted when Dispose is called
        protected abstract void Dispose();
    }
}
