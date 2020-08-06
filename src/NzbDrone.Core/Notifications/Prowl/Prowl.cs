using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class Prowl : NotificationBase<ProwlSettings>
    {
        private readonly IProwlProxy _prowlProxy;

        public Prowl(IProwlProxy prowlProxy)
        {
            _prowlProxy = prowlProxy;
        }

        public override string Link => "https://www.prowlapp.com/";
        public override string Name => "Prowl";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _prowlProxy.SendNotification(EPISODE_GRABBED_TITLE, grabMessage.Message, Settings.ApiKey, (ProwlPriority)Settings.Priority);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _prowlProxy.SendNotification(EPISODE_DOWNLOADED_TITLE, message.Message, Settings.ApiKey, (ProwlPriority)Settings.Priority);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            _prowlProxy.SendNotification(HEALTH_ISSUE_TITLE, message.Message, Settings.ApiKey, (ProwlPriority)Settings.Priority);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_prowlProxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
