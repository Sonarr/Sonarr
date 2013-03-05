using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Jobs.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class RecentBacklogSearchJob : IJob
    {
        private readonly IEpisodeService _episodeService;
        private readonly EpisodeSearchJob _episodeSearchJob;
        private readonly IConfigService _configService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RecentBacklogSearchJob(IEpisodeService episodeService, EpisodeSearchJob episodeSearchJob,
                                            IConfigService configService)
        {
            _episodeService = episodeService;
            _episodeSearchJob = episodeSearchJob;
            _configService = configService;
        }

        public string Name
        {
            get { return "Recent Backlog Search"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(1); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            var missingEpisodes = GetMissingForEnabledSeries();

            Logger.Debug("Processing missing episodes from the last 30 days, count: {0}", missingEpisodes.Count);
            foreach (var episode in missingEpisodes)
            {
                _episodeSearchJob.Start(notification,  new { EpisodeId = episode.Id });
            }
        }

        public List<Episode> GetMissingForEnabledSeries()
        {
            if (!_configService.EnableBacklogSearching)
            {
                Logger.Trace("Backlog searching is not enabled, only running for explicitly enabled series.");
                return _episodeService.EpisodesWithoutFiles(true).Where(e =>
                                                                                e.AirDate >= DateTime.Today.AddDays(-30) &&
                                                                                e.Series.BacklogSetting == BacklogSettingType.Enable &&
                                                                                e.Series.Monitored
                                                                            ).ToList();
            }

            else
            {
                Logger.Trace("Backlog searching is enabled, skipping explicitly disabled series.");
                return _episodeService.EpisodesWithoutFiles(true).Where(e =>
                                                                                e.AirDate >= DateTime.Today.AddDays(-30) &&
                                                                                e.Series.BacklogSetting != BacklogSettingType.Disable &&
                                                                                e.Series.Monitored
                                                                            ).ToList();
            }
        }
    }
}