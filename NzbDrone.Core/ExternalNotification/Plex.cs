using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ExternalNotification
{
    public class Plex : ExternalNotificationBase
    {
        private readonly IConfigService _configService;
        private readonly PlexProvider _plexProvider;

        public Plex(IConfigService configService, IExternalNotificationRepository repository, PlexProvider plexProvider, Logger logger)
            : base(repository, logger)
        {
            _configService = configService;
            _plexProvider = plexProvider;
        }

        public override string Name
        {
            get { return "Plex"; }
        }

        protected override void OnGrab(string message)
        {
            const string header = "NzbDrone [TV] - Grabbed";
            _plexProvider.Notify(header, message);
        }

        protected override void OnDownload(string message, Series series)
        {
            const string header = "NzbDrone [TV] - Downloaded";
            _plexProvider.Notify(header, message);
            UpdateIfEnabled();
        }

        protected override void AfterRename( Series series)
        {
            UpdateIfEnabled();
        }

        private void UpdateIfEnabled()
        {
            if (_configService.PlexUpdateLibrary)
            {
                _plexProvider.UpdateLibrary();
            }
        }
    }
}
