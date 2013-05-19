using NLog;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class Xbmc : NotificationWithSetting<XbmcSettings>
    {
        private readonly XbmcProvider _xbmcProvider;

        public Xbmc(XbmcProvider xbmcProvider, Logger logger)
        {
            _xbmcProvider = xbmcProvider;
        }

        public override string Name
        {
            get { return "XBMC"; }
        }

        public override void OnGrab(string message)
        {
            const string header = "NzbDrone [TV] - Grabbed";

            _xbmcProvider.Notify(Settings, header, message);
        }

        public override void OnDownload(string message, Series series)
        {
            const string header = "NzbDrone [TV] - Downloaded";

            _xbmcProvider.Notify(Settings, header, message);
            UpdateAndClean(series);
        }

        public override void AfterRename(Series series)
        {
            UpdateAndClean(series);
        }

        private void UpdateAndClean(Series series)
        {
            if (Settings.UpdateLibrary)
            {
                _xbmcProvider.Update(Settings, series);
            }

            if (Settings.CleanLibrary)
            {
                _xbmcProvider.Clean(Settings);
            }
        }
    }
}
