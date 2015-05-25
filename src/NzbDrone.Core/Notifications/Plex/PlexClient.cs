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

        public override string Link
        {
            get { return "http://www.plexapp.com/"; }
        }

        public override void OnGrab(string message)
        {
            const string header = "Sonarr [TV] - Grabbed";
            _plexClientService.Notify(Settings, header, message);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string header = "Sonarr [TV] - Downloaded";
            _plexClientService.Notify(Settings, header, message.Message);
        }

        public override void AfterRename(Series series)
        {
        }

        public override string Name
        {
            get
            {
                return "Plex Media Center";
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_plexClientService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
