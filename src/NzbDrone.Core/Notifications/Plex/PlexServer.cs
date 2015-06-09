using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexServer : NotificationBase<PlexServerSettings>
    {
        private readonly IPlexServerService _plexServerService;

        public PlexServer(IPlexServerService plexServerService)
        {
            _plexServerService = plexServerService;
        }

        public override string Link
        {
            get { return "http://www.plexapp.com/"; }
        }

        public override void OnGrab(string message)
        {
        }

        public override void OnDownload(DownloadMessage message)
        {
            UpdateIfEnabled(message.Series);
        }

        public override void AfterRename(Series series)
        {
            UpdateIfEnabled(series);
        }

        private void UpdateIfEnabled(Series series)
        {
            if (Settings.UpdateLibrary)
            {
                _plexServerService.UpdateLibrary(series, Settings);
            }
        }

        public override string Name
        {
            get
            {
                return "Plex Media Server";
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_plexServerService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
