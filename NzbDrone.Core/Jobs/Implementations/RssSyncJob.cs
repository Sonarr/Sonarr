using System;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class RssSyncJob : IJob
    {
        private readonly IRssSyncService _rssSyncServiceService;
        private readonly IConfigService _configService;


        public RssSyncJob(IRssSyncService rssSyncServiceService, IConfigService configService)
        {
            _rssSyncServiceService = rssSyncServiceService;
            _configService = configService;
        }

        public string Name
        {
            get { return "RSS Sync"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromMinutes(_configService.RssSyncInterval); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            _rssSyncServiceService.Sync();
        }
    }
}