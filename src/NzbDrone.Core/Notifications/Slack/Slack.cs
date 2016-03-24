using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;


namespace NzbDrone.Core.Notifications.Slack
{
    public class Slack : NotificationBase<SlackSettings>
    {
        private readonly Logger _logger;
        private readonly ISlackService _service;

        public override bool SupportsOnUpgrade => false;

        public Slack(Logger logger, ISlackService service)
        {
            _logger = logger;
            _service = service;
        }

        public override string Link
        {
            get { return "https://github.com/Sonarr/Sonarr/wiki/Custom-Post-Processing-Scripts"; }
        }

        public override void OnGrab(GrabMessage message)
        {
            _logger.Trace("OnGrab: {0}", message);
            _service.OnGrab(message, Settings);

        }

        public override void OnDownload(DownloadMessage message)
        {
            _logger.Trace("OnDownload: {0}", message);
            _service.OnDownload(message, Settings);
        }

        public override void OnRename(Series series)
        {
            _logger.Trace("OnRename: {0}", series);          
            _service.OnRename(series, Settings);
        }

        public override string Name
        {
            get
            {
                return "Slack";
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_service.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
