using System;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class ExternalNotificationProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly XbmcProvider _xbmcProvider;

        public ExternalNotificationProvider(ConfigProvider configProvider, XbmcProvider xbmcProvider)
        {
            _configProvider = configProvider;
            _xbmcProvider = xbmcProvider;
        }

        public virtual void OnGrab(string message)
        {
            var header = "NzbDrone [TV] - Grabbed";

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcEnabled", false, true)))
            {
                if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnGrab", false, true)))
                {
                    Logger.Trace("Sending Notifcation to XBMC");
                    _xbmcProvider.Notify(header, message);
                    return;
                }
                Logger.Trace("XBMC NotifyOnGrab is not enabled");
            }

            Logger.Trace("XBMC Notifier is not enabled");
        }

        public virtual void OnDownload(EpisodeRenameModel erm)
        {
            var header = "NzbDrone [TV] - Downloaded";
            var message = EpisodeRenameHelper.GetNewName(erm);

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcEnabled", false, true)))
            {
                if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnDownload", false, true)))
                {
                    Logger.Trace("Sending Notifcation to XBMC");
                    _xbmcProvider.Notify(header, message);
                }

                if (Convert.ToBoolean(_configProvider.GetValue("XbmcUpdateOnDownload", false, true)))
                {
                    Logger.Trace("Sending Update Request to XBMC");
                    _xbmcProvider.Update(erm.EpisodeFile.SeriesId);
                }

                if (Convert.ToBoolean(_configProvider.GetValue("XbmcCleanOnDownload", false, true)))
                {
                    Logger.Trace("Sending Clean DB Request to XBMC");
                    _xbmcProvider.Clean();
                }
            }

            Logger.Trace("XBMC Notifier is not enabled");


            throw new NotImplementedException();
        }

        public virtual void OnRename(EpisodeRenameModel erm)
        {
            var header = "NzbDrone [TV] - Renamed";
            var message = EpisodeRenameHelper.GetNewName(erm);

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnRename", false, true)))
            {
                Logger.Trace("Sending Notifcation to XBMC");
                _xbmcProvider.Notify(header, message);
            }

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcUpdateOnRename", false, true)))
            {
                Logger.Trace("Sending Update Request to XBMC");
                _xbmcProvider.Update(erm.EpisodeFile.SeriesId);
            }

            if (Convert.ToBoolean(_configProvider.GetValue("XbmcCleanOnRename", false, true)))
            {
                Logger.Trace("Sending Clean DB Request to XBMC");
                _xbmcProvider.Clean();
            }


            throw new NotImplementedException();
        }
    }
}