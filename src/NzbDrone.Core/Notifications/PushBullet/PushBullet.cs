using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common;
using NzbDrone.Core.Tv;

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

        public override void OnGrab(string message)
        {
            const string title = "Episode Grabbed";

            _proxy.SendNotification(title, message, Settings.ApiKey, Settings.DeviceId);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Episode Downloaded";

            _proxy.SendNotification(title, message.Message, Settings.ApiKey, Settings.DeviceId);
        }

        public override void AfterRename(Series series)
        {
        }

        public override IEnumerable<ValidationFailure> Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return failures;
        }
    }
}
