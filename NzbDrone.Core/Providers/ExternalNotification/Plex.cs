using System;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class Plex : ExternalNotificationBase
    {
        private readonly PlexProvider _plexProvider;

        public Plex(ConfigProvider configProvider, PlexProvider plexProvider)
            : base(configProvider)
        {
            _plexProvider = plexProvider;
        }

        public override string Name
        {
            get { return "Plex"; }
        }

        public override void OnGrab(string message)
        {
            const string header = "NzbDrone [TV] - Grabbed";

            if (_configProvider.PlexNotifyOnGrab)
            {
                _logger.Trace("Sending Notification to Plex Clients");
                _plexProvider.Notify(header, message);
            }
        }

        public override void OnDownload(string message, Series series)
        {
            const string header = "NzbDrone [TV] - Downloaded";

            if (_configProvider.PlexNotifyOnDownload)
            {
                _logger.Trace("Sending Notification to Plex Clients");
                _plexProvider.Notify(header, message);
            }

            UpdateIfEnabled();
        }

        public override void OnRename(string message, Series series)
        {

        }

        public override void AfterRename(string message, Series series)
        {
            UpdateIfEnabled();
        }

        private void UpdateIfEnabled()
        {
            if (_configProvider.PlexUpdateLibrary)
            {
                _logger.Trace("Sending Update Request to Plex Server");
                _plexProvider.UpdateLibrary();
            }
        }
    }
}
