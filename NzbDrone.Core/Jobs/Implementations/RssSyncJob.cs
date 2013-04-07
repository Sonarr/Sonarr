using System;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class RssSyncJob : IJob
    {
        private readonly ISyncRss _syncRssService;
        private readonly IConfigService _configService;


        public RssSyncJob(ISyncRss syncRssService, IConfigService configService)
        {
            _syncRssService = syncRssService;
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
            _syncRssService.Sync();
        }
    }
}