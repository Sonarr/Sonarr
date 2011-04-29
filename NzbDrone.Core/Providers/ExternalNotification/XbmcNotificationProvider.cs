using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class XbmcNotificationProvider : ExternalNotificationProviderBase
    {
        private readonly Logger _logger;
        private readonly XbmcProvider _xbmcProvider;

        public XbmcNotificationProvider(ConfigProvider configProvider, XbmcProvider xbmcProvider,
            ExternalNotificationProvider externalNotificationProvider) : base(configProvider, externalNotificationProvider)
        {
            _xbmcProvider = xbmcProvider;
            _logger = LogManager.GetLogger(GetType().ToString());
        }

        public override string Name
        {
            get { return "XBMC"; }
        }

        public override void OnGrab(string message)
        {
            var header = "NzbDrone [TV] - Grabbed";

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcEnabled", false, true)))
            {
                if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnGrab", false, true)))
                {
                    _logger.Trace("Sending Notifcation to XBMC");
                    _xbmcProvider.Notify(header, message);
                    return;
                }
                _logger.Trace("XBMC NotifyOnGrab is not enabled");
            }

            _logger.Trace("XBMC Notifier is not enabled");
        }

        public override void OnDownload(string message, int seriesId)
        {
            var header = "NzbDrone [TV] - Downloaded";

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcEnabled", false, true)))
            {
                if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnDownload", false, true)))
                {
                    _logger.Trace("Sending Notifcation to XBMC");
                    _xbmcProvider.Notify(header, message);
                }

                if (Convert.ToBoolean(_configProvider.GetValue("XbmcUpdateOnDownload", false, true)))
                {
                    _logger.Trace("Sending Update Request to XBMC");
                    _xbmcProvider.Update(seriesId);
                }

                if (Convert.ToBoolean(_configProvider.GetValue("XbmcCleanOnDownload", false, true)))
                {
                    _logger.Trace("Sending Clean DB Request to XBMC");
                    _xbmcProvider.Clean();
                }
            }

            _logger.Trace("XBMC Notifier is not enabled");
        }

        public override void OnRename(string message, int seriesId)
        {
            var header = "NzbDrone [TV] - Renamed";

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnRename", false, true)))
            {
                _logger.Trace("Sending Notifcation to XBMC");
                _xbmcProvider.Notify(header, message);
            }

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcUpdateOnRename", false, true)))
            {
                _logger.Trace("Sending Update Request to XBMC");
                _xbmcProvider.Update(seriesId);
            }

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcCleanOnRename", false, true)))
            {
                _logger.Trace("Sending Clean DB Request to XBMC");
                _xbmcProvider.Clean();
            }
        }
    }
}
