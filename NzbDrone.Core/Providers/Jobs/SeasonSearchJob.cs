using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    public class SeasonSearchJob : IJob
    {
        private readonly SearchProvider _searchProvider;
        private readonly EpisodeSearchJob _episodeSearchJob;
        private readonly EpisodeProvider _episodeProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SeasonSearchJob(SearchProvider searchProvider, EpisodeSearchJob episodeSearchJob,
                                EpisodeProvider episodeProvider)
        {
            _searchProvider = searchProvider;
            _episodeSearchJob = episodeSearchJob;
            _episodeProvider = episodeProvider;
        }

        public SeasonSearchJob()
        {
            
        }

        public string Name
        {
            get { return "Season Search"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            if (secondaryTargetId <= 0)
                throw new ArgumentOutOfRangeException("secondaryTargetId");

            if (_searchProvider.SeasonSearch(notification, targetId, secondaryTargetId))
                return;

            Logger.Debug("Getting episodes from database for series: {0} and season: {1}", targetId, secondaryTargetId);
            var episodes = _episodeProvider.GetEpisodesBySeason(targetId, secondaryTargetId);

            if (episodes == null || episodes.Count == 0)
            {
                Logger.Warn("No episodes in database found for series: {0} and season: {1}.", targetId, secondaryTargetId);
                return;
            }

            foreach (var episode in episodes.Where(e => !e.Ignored))
            {
                _episodeSearchJob.Start(notification, episode.EpisodeId, 0);
            }
        }
    }
}