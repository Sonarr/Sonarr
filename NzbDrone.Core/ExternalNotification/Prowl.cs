using System.Linq;
using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;
using Prowlin;

namespace NzbDrone.Core.ExternalNotification
{
    public class Prowl : ExternalNotificationBase
    {
        private readonly IConfigService _configService;
        private readonly ProwlProvider _prowlProvider;

        public Prowl(IConfigService configService, IExternalNotificationRepository repository, ProwlProvider prowlProvider, Logger logger)
            : base(repository, logger)
        {
            _configService = configService;
            _prowlProvider = prowlProvider;
        }

        public override string Name
        {
            get { return "Prowl"; }
        }

        protected override void OnGrab(string message)
        {
            const string title = "Episode Grabbed";

            var apiKeys = _configService.ProwlApiKeys;
            var priority = _configService.ProwlPriority;

            _prowlProvider.SendNotification(title, message, apiKeys, (NotificationPriority)priority);
        }

        protected override void OnDownload(string message, Series series)
        {
            const string title = "Episode Downloaded";

            var apiKeys = _configService.ProwlApiKeys;
            var priority = _configService.ProwlPriority;

            _prowlProvider.SendNotification(title, message, apiKeys, (NotificationPriority)priority);
        }
    }
}
