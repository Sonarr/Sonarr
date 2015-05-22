using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexServer : NotificationBase<PlexServerSettings>
    {
        private readonly IPlexService _plexService;

        public PlexServer(IPlexService plexService)
        {
            _plexService = plexService;
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
            UpdateIfEnabled();
        }

        public override void AfterRename(Series series)
        {
            UpdateIfEnabled();
        }

        private void UpdateIfEnabled()
        {
            if (Settings.UpdateLibrary)
            {
                _plexService.UpdateLibrary(Settings);
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

            failures.AddIfNotNull(_plexService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
