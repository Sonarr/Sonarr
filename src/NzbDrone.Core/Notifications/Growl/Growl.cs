using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Growl
{
    public class Growl : NotificationBase<GrowlSettings>
    {
        private readonly IGrowlService _growlService;

        public Growl(IGrowlService growlService)
        {
            _growlService = growlService;
        }

        public override string Link
        {
            get { return "http://growl.info/"; }
        }

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string title = "Episode Grabbed";

            _growlService.SendNotification(title, grabMessage.Message, "GRAB", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnGrabMovie(GrabMovieMessage grabMessage)
        {
            const string title = "Movie Grabbed";

            _growlService.SendNotification(title, grabMessage.Message, "GRAB", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Episode Downloaded";

            _growlService.SendNotification(title, message.Message, "DOWNLOAD", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnDownloadMovie(DownloadMovieMessage message)
        {
            const string title = "Movie Downloaded";

            _growlService.SendNotification(title, message.Message, "DOWNLOAD", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnRename(Series series)
        {
        }

        public override void OnRenameMovie(Movie movie)
        {
        }

        public override bool SupportsOnRenameMovie
        {
            get
            {
                return false;
            }
        }

        public override string Name
        {
            get
            {
                return "Growl";
            }
        }

        public override bool SupportsOnRename
        {
            get
            {
                return false;
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_growlService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
