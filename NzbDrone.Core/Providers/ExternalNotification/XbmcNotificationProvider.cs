using System;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class XbmcNotificationProvider : ExternalNotificationProviderBase
    {
        private readonly XbmcProvider _xbmcProvider;

        public XbmcNotificationProvider(ConfigProvider configProvider, XbmcProvider xbmcProvider,
            ExternalNotificationProvider externalNotificationProvider)
            : base(configProvider, externalNotificationProvider)
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

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcEnabled", false)))
            {
                if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnGrab", false)))
                {
                    _logger.Trace("Sending Notification to XBMC");
                    _xbmcProvider.Notify(header, message);
                    return;
                }
                _logger.Trace("XBMC NotifyOnGrab is not enabled");
            }

            _logger.Trace("XBMC Notifier is not enabled");
        }

        public override void OnDownload(string message, Series series)
        {
            const string header = "NzbDrone [TV] - Downloaded";

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcEnabled", false)))
            {
                if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnDownload", false)))
                {
                    _logger.Trace("Sending Notification to XBMC");
                    _xbmcProvider.Notify(header, message);
                }

                if (Convert.ToBoolean(_configProvider.GetValue("XbmcUpdateOnDownload", false)))
                {
                    _logger.Trace("Sending Update Request to XBMC");
                    _xbmcProvider.Update(series);
                }

                if (Convert.ToBoolean(_configProvider.GetValue("XbmcCleanOnDownload", false)))
                {
                    _logger.Trace("Sending Clean DB Request to XBMC");
                    _xbmcProvider.Clean();
                }
            }

            _logger.Trace("XBMC Notifier is not enabled");
        }

        public override void OnRename(string message, Series series)
        {
            const string header = "NzbDrone [TV] - Renamed";

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnRename", false)))
            {
                _logger.Trace("Sending Notification to XBMC");
                _xbmcProvider.Notify(header, message);
            }

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcUpdateOnRename", false)))
            {
                _logger.Trace("Sending Update Request to XBMC");
                _xbmcProvider.Update(series);
            }

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcCleanOnRename", false)))
            {
                _logger.Trace("Sending Clean DB Request to XBMC");
                _xbmcProvider.Clean();
            }
        }
    }
}
