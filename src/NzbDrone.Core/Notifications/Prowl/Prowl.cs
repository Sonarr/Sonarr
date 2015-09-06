using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using Prowlin;
using System;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class Prowl : NotificationBase<ProwlSettings>
    {
        private readonly IProwlService _prowlService;

        public Prowl(IProwlService prowlService)
        {
            _prowlService = prowlService;
        }

        public override string Link
        {
            get { return "http://www.prowlapp.com/"; }
        }

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string title = "Episode Grabbed";

            _prowlService.SendNotification(title, grabMessage.Message, Settings.ApiKey, (NotificationPriority)Settings.Priority);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Episode Downloaded";

            _prowlService.SendNotification(title, message.Message, Settings.ApiKey, (NotificationPriority)Settings.Priority);
        }

        public override void OnRename(Series series)
        {
        }

        public override void OnSystemUpdateAvailable(UpdatePackage package)
        {
            const string subject = "New System Update";
            var body = string.Format("New update is available - {0} - {1}", package.Version.ToString(), package.Url.ToString());

            _prowlService.SendNotification(subject, body, Settings.ApiKey, (NotificationPriority)Settings.Priority);
        }

        public override string Name
        {
            get
            {
                return "Prowl";
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

            failures.AddIfNotNull(_prowlService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
