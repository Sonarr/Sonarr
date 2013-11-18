using System.Linq;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class Xbmc : NotificationBase<XbmcSettings>
    {
        private readonly IXbmcService _xbmcProvider;

        public Xbmc(IXbmcService xbmcProvider)
        {
            _xbmcProvider = xbmcProvider;
        }

        public override string Link
        {
            get { return "http://xbmc.org/"; }
        }

        public override void OnGrab(string message)
        {
            const string header = "NzbDrone [TV] - Grabbed";

            if (Settings.Notify)
            {
                _xbmcProvider.Notify(Settings, header, message);
            }
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string header = "NzbDrone [TV] - Downloaded";

            if (Settings.Notify)
            {
                _xbmcProvider.Notify(Settings, header, message.Message);
            }

            UpdateAndClean(message.Series, message.OldFiles.Any());
        }

        public override void AfterRename(Series series)
        {
            UpdateAndClean(series);
        }

        private void UpdateAndClean(Series series, bool clean = true)
        {
            if (Settings.UpdateLibrary)
            {
                _xbmcProvider.Update(Settings, series);
            }

            if (clean && Settings.CleanLibrary)
            {
                _xbmcProvider.Clean(Settings);
            }
        }
    }
}
