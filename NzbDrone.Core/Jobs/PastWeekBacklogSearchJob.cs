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
    public class PastWeekBacklogSearchJob : IJob
    {
        private readonly IEpisodeService _episodeService;
        private readonly EpisodeSearchJob _episodeSearchJob;
        private readonly IConfigService _configService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public PastWeekBacklogSearchJob(IEpisodeService episodeService, EpisodeSearchJob episodeSearchJob,
                                            IConfigService configService)
        {
            _episodeService = episodeService;
            _episodeSearchJob = episodeSearchJob;
            _configService = configService;
        }

        public string Name
        {
            get { return "Past Week Backlog Search"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            var missingEpisodes = GetMissingForEnabledSeries();

            Logger.Debug("Processing missing episodes from the past week, count: {0}", missingEpisodes.Count);
            foreach (var episode in missingEpisodes)
            {
                _episodeSearchJob.Start(notification, new { EpisodeId = episode.Id });
            }
        }

        public List<Episode> GetMissingForEnabledSeries()
        {
            return _episodeService.EpisodesWithoutFiles(true).Where(e =>
                                                                                e.AirDate >= DateTime.Today.AddDays(-7) &&
                                                                                e.Series.Monitored
                                                                            ).ToList();
        }
    }
}