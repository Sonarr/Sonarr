using System;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class Xbmc : ExternalNotificationBase
    {
        private readonly XbmcProvider _xbmcProvider;

        public Xbmc(ConfigProvider configProvider, XbmcProvider xbmcProvider)
            : base(configProvider)
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

            if (_configProvider.XbmcNotifyOnGrab)
            {
                _logger.Trace("Sending Notification to XBMC");
                _xbmcProvider.Notify(header, message);
            }
        }

        public override void OnDownload(string message, Series series)
        {
            const string header = "NzbDrone [TV] - Downloaded";

            if (_configProvider.XbmcNotifyOnDownload)
            {
                _logger.Trace("Sending Notification to XBMC");
                _xbmcProvider.Notify(header, message);
            }

            UpdateAndClean(series);
        }

        public override void OnRename(string message, Series series)
        {
            
        }

        public override void AfterRename(string message, Series series)
        {
            UpdateAndClean(series);
        }

        private void UpdateAndClean(Series series)
        {
            if (_configProvider.XbmcUpdateLibrary)
            {
                _logger.Trace("Sending Update Request to XBMC");
                _xbmcProvider.Update(series);
            }

            if (_configProvider.XbmcCleanLibrary)
            {
                _logger.Trace("Sending Clean DB Request to XBMC");
                _xbmcProvider.Clean();
            }
        }
    }
}
