using System.Linq;
using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ExternalNotification
{
    public class Growl : ExternalNotificationBase
    {
        private readonly IConfigService _configService;
        private readonly GrowlProvider _growlProvider;

        public Growl(IExternalNotificationRepository repository, IConfigService configService, GrowlProvider growlProvider, Logger logger)
            : base(repository, logger)
        {
            _configService = configService;
            _growlProvider = growlProvider;
        }

        public override string Name
        {
            get { return "Growl"; }
        }

        protected override void OnGrab(string message)
        {
            const string title = "Episode Grabbed";

            var growlHost = _configService.GrowlHost.Split(':');
            var host = growlHost[0];
            var port = Convert.ToInt32(growlHost[1]);

            _growlProvider.SendNotification(title, message, "GRAB", host, port, _configService.GrowlPassword);
        }

        protected override void OnDownload(string message, Series series)
        {
            const string title = "Episode Downloaded";

            var growlHost = _configService.GrowlHost.Split(':');
            var host = growlHost[0];
            var port = Convert.ToInt32(growlHost[1]);

            _growlProvider.SendNotification(title, message, "DOWNLOAD", host, port, _configService.GrowlPassword);
        }
    }
}
