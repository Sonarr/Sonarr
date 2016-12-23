using System.Collections.Generic;
using System.Net.Sockets;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexHomeTheater : NotificationBase<PlexHomeTheaterSettings>
    {
        private readonly IXbmcService _xbmcService;
        private readonly Logger _logger;

        public PlexHomeTheater(IXbmcService xbmcService, Logger logger)
        {
            _xbmcService = xbmcService;
            _logger = logger;
        }

        public override string Link => "https://plex.tv/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string header = "Sonarr - Grabbed";

            Notify(Settings, header, grabMessage.Message);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string header = "Sonarr - Downloaded";

            Notify(Settings, header, message.Message);
        }

        public override void OnRename(Series series)
        {
            
        }

        public override string Name => "Plex Home Theater";

        public override bool SupportsOnRename => false;

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_xbmcService.Test(Settings, "Success! PHT has been successfully configured!"));

            return new ValidationResult(failures);
        }

        private void Notify(XbmcSettings settings, string header, string message)
        {
            try
            {
                if (Settings.Notify)
                {
                    _xbmcService.Notify(Settings, header, message);
                }
            }
            catch (SocketException ex)
            {
                var logMessage = string.Format("Unable to connect to PHT Host: {0}:{1}", Settings.Host, Settings.Port);
                _logger.Debug(ex, logMessage);
            }
        }
    }
}
