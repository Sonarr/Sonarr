using System;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class Xbmc : ExternalNotificationBase
    {
        private readonly XbmcProvider _xbmcProvider;

        public Xbmc(IConfigService configService, XbmcProvider xbmcProvider)
            : base(configService)
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

            if (_configService.XbmcNotifyOnGrab)
            {
                _logger.Trace("Sending Notification to XBMC");
                _xbmcProvider.Notify(header, message);
            }
        }

        public override void OnDownload(string message, Series series)
        {
            const string header = "NzbDrone [TV] - Downloaded";

            if (_configService.XbmcNotifyOnDownload)
            {
                _logger.Trace("Sending Notification to XBMC");
                _xbmcProvider.Notify(header, message);
            }

            UpdateAndClean(series);
        }

        public override void AfterRename(string message, Series series)
        {
            UpdateAndClean(series);
        }

        private void UpdateAndClean(Series series)
        {
            if (_configService.XbmcUpdateLibrary)
            {
                _logger.Trace("Sending Update Request to XBMC");
                _xbmcProvider.Update(series);
            }

            if (_configService.XbmcCleanLibrary)
            {
                _logger.Trace("Sending Clean DB Request to XBMC");
                _xbmcProvider.Clean();
            }
        }
    }
}
