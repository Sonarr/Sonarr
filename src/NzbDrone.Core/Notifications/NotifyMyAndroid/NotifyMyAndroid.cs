using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.NotifyMyAndroid
{
    public class NotifyMyAndroid : NotificationBase<NotifyMyAndroidSettings>
    {
        private readonly INotifyMyAndroidProxy _proxy;

        public NotifyMyAndroid(INotifyMyAndroidProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link => "https://www.notifymyandroid.com/";
        public override string Name => "Notify My Android";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _proxy.SendNotification(EPISODE_GRABBED_TITLE, grabMessage.Message, Settings.ApiKey, (NotifyMyAndroidPriority)Settings.Priority);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _proxy.SendNotification(EPISODE_DOWNLOADED_TITLE, message.Message, Settings.ApiKey, (NotifyMyAndroidPriority)Settings.Priority);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
