using NLog;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class Xbmc : NotificationBase<XbmcSettings>
    {
        private readonly IXbmcService _xbmcProvider;

        public Xbmc(IXbmcService xbmcProvider, Logger logger)
        {
            _xbmcProvider = xbmcProvider;
        }

        public override string Name
        {
            get { return "XBMC"; }
        }

        public override string ImplementationName
        {
            get { return "XBMC"; }
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

        public override void OnDownload(string message, Series series)
        {
            const string header = "NzbDrone [TV] - Downloaded";

            if (Settings.Notify)
            {
                _xbmcProvider.Notify(Settings, header, message);
            }

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
