using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
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

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            if (secondaryTargetId < 0)
                throw new ArgumentOutOfRangeException("secondaryTargetId");

            //Perform a Partial Season Search - Because a full season search is a waste
            //3 searches should guarentee results, (24 eps) versus, a potential 4 to get the same eps.
            var successes = _searchProvider.PartialSeasonSearch(notification, targetId, secondaryTargetId);

            if (successes.Count == 0)
                return;

            Logger.Debug("Getting episodes from database for series: {0} and season: {1}", targetId, secondaryTargetId);
            var episodes = _episodeProvider.GetEpisodesBySeason(targetId, secondaryTargetId);

            if (episodes == null || episodes.Count == 0)
            {
                Logger.Warn("No episodes in database found for series: {0} and season: {1}.", targetId, secondaryTargetId);
                return;
            }

            if (episodes.Count == successes.Count)
                return;

            var missingEpisodes = episodes.Select(e => e.EpisodeNumber).Except(successes).ToList();

            foreach (var episode in episodes.Where(e => !e.Ignored && missingEpisodes.Contains(e.EpisodeNumber)).OrderBy(o => o.EpisodeNumber))
            {
                _episodeSearchJob.Start(notification, episode.EpisodeId, 0);
            }
        }
    }
}