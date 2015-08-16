using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class PushBullet : NotificationBase<PushBulletSettings>
    {
        private readonly IPushBulletProxy _proxy;

        public PushBullet(IPushBulletProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link
        {
            get { return "https://www.pushbullet.com/"; }
        }

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string title = "Sonarr - Episode Grabbed";

            _proxy.SendNotification(title, grabMessage.Message, Settings);
        }

        public override void OnGrabMovie(GrabMovieMessage grabMessage)
        {
            const string title = "Sonarr - Movie Grabbed";

            _proxy.SendNotification(title, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Sonarr - Episode Downloaded";

            _proxy.SendNotification(title, message.Message, Settings);
        }

        public override void OnDownloadMovie(DownloadMovieMessage message)
        {
            const string title = "Sonarr - Movie Downloaded";

            _proxy.SendNotification(title, message.Message, Settings);
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
                return "Pushbullet";
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

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
