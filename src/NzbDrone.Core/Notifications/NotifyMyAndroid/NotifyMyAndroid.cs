
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.NotifyMyAndroid
{
    public class NotifyMyAndroid : NotificationBase<NotifyMyAndroidSettings>
    {
        private readonly INotifyMyAndroidProxy _proxy;

        public NotifyMyAndroid(INotifyMyAndroidProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link => "http://www.notifymyandroid.com/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string title = "Episode Grabbed";

            _proxy.SendNotification(title, grabMessage.Message, Settings.ApiKey, (NotifyMyAndroidPriority)Settings.Priority);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Episode Downloaded";

            _proxy.SendNotification(title, message.Message, Settings.ApiKey, (NotifyMyAndroidPriority)Settings.Priority);
        }

        public override void OnRename(Series series)
        {
        }

        public override string Name => "Notify My Android";

        public override bool SupportsOnRename => false;

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
