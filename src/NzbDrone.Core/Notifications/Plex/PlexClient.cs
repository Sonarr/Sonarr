using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexClient : NotificationBase<PlexClientSettings>
    {
        private readonly IPlexClientService _plexClientService;

        public PlexClient(IPlexClientService plexClientService)
        {
            _plexClientService = plexClientService;
        }

        public override string Link => "http://www.plexapp.com/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string header = "Sonarr [TV] - Grabbed";
            _plexClientService.Notify(Settings, header, grabMessage.Message);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string header = "Sonarr [TV] - Downloaded";
            _plexClientService.Notify(Settings, header, message.Message);
        }

        public override void OnRename(Series series)
        {
        }

        public override string Name => "Plex Media Center";

        public override bool SupportsOnRename => false;

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_plexClientService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
