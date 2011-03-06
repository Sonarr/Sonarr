using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class ExternalNotificationProvider : IExtenalNotificationProvider
    {
        private readonly IConfigProvider _configProvider;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ExternalNotificationProvider(IConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        #region IExternalNotificationProvider Members
        public void OnGrab(string message)
        {
            var header = "NzbDrone [TV] - Grabbed";

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcEnabled", false, true)))
            {
                if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnGrab", false, true)))
                {
                    Logger.Trace("Sending Notifcation to XBMC");

                    return;
                }
                Logger.Trace("XBMC NotifyOnGrab is not enabled");
            }

            Logger.Trace("XBMC Notifier is not enabled");
        }

        public void OnDownload(EpisodeRenameModel erm)
        {
            var header = "NzbDrone [TV] - Downloaded";
            var message = EpisodeRenameHelper.GetNewName(erm);

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcEnabled", false, true)))
            {
                if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnDownload", false, true)))
                {
                    Logger.Trace("Sending Notifcation to XBMC");
                    //Send to XBMC
                }

                if (Convert.ToBoolean(_configProvider.GetValue("XbmcUpdateOnDownload", false, true)))
                {
                    Logger.Trace("Sending Update Request to XBMC");
                    //Send to XBMC
                    //Send SeriesID
                }

                if (Convert.ToBoolean(_configProvider.GetValue("XbmcCleanOnDownload", false, true)))
                {
                    Logger.Trace("Sending Clean DB Request to XBMC");
                    //Send to XBMC
                }
            }

            Logger.Trace("XBMC Notifier is not enabled");


            throw new NotImplementedException();
        }

        public void OnRename(EpisodeRenameModel erm)
        {
            var header = "NzbDrone [TV] - Renamed";
            var message = EpisodeRenameHelper.GetNewName(erm);

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnRename", false, true)))
            {
                Logger.Trace("Sending Notifcation to XBMC");
                //Send to XBMC
            }

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcUpdateOnRename", false, true)))
            {
                Logger.Trace("Sending Update Request to XBMC");
                //Send to XBMC
                //Send SeriesID
            }

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcCleanOnRename", false, true)))
            {
                Logger.Trace("Sending Clean DB Request to XBMC");
                //Send to XBMC
            }


            throw new NotImplementedException();
        }
        #endregion
    }
}
