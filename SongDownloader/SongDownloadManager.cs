using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTally.Utils.TootTallySettings;

namespace TootTally.SongDownloader
{
    public static class SongDownloadManager
    {
        private static bool _isInitialized;
        public static void Initialize()
        {
            if (_isInitialized) return;

            TootTallySettingsManager.AddNewPage(new SongDownloadPage());
            _isInitialized = true;
        }
    }
}
