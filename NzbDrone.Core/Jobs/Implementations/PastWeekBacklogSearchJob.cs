using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class PastWeekBacklogSearchJob : IJob
    {
        private readonly IEpisodeService _episodeService;
        private readonly EpisodeSearchJob _episodeSearchJob;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public PastWeekBacklogSearchJob(IEpisodeService episodeService, EpisodeSearchJob episodeSearchJob)
        {
            _episodeService = episodeService;
            _episodeSearchJob = episodeSearchJob;
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