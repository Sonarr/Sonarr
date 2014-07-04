using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common;
using NzbDrone.Core.Tv;
using Prowlin;

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

        public override void OnGrab(string message)
        {
            const string title = "Episode Grabbed";

            _prowlService.SendNotification(title, message, Settings.ApiKey, (NotificationPriority)Settings.Priority);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Episode Downloaded";

            _prowlService.SendNotification(title, message.Message, Settings.ApiKey, (NotificationPriority)Settings.Priority);
        }

        public override void AfterRename(Series series)
        {
        }

        public override IEnumerable<ValidationFailure> Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_prowlService.Test(Settings));

            return failures;
        }
    }
}
