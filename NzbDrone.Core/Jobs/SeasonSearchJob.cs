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

            if (_searchProvider.SeasonSearch(notification, targetId, secondaryTargetId))
                return;

            Logger.Debug("Getting episodes from database for series: {0} and season: {1}", targetId, secondaryTargetId);
            var episodes = _episodeProvider.GetEpisodesBySeason(targetId, secondaryTargetId);

            if (episodes == null || episodes.Count == 0)
            {
                Logger.Warn("No episodes in database found for series: {0} and season: {1}.", targetId, secondaryTargetId);
                return;
            }

            //Perform a Partial Season Search
            var addedSeries = _searchProvider.PartialSeasonSearch(notification, targetId, secondaryTargetId);

            //addedSeries.Distinct().ToList().Sort();
            //var episodeNumbers = episodes.Where(w => w.AirDate <= DateTime.Today.AddDays(1)).Select(s => s.EpisodeNumber).ToList();
            //episodeNumbers.Sort();

            //if (addedSeries.SequenceEqual(episodeNumbers))
            //    return;
            
            ////Get the list of episodes that weren't downloaded
            //var missingEpisodes = episodeNumbers.Except(addedSeries).ToList();

            //TODO: do one by one check only when max number of feeds have been returned by the indexer
            //Only process episodes that is in missing episodes (To ensure we double check if the episode is available)
            //foreach (var episode in episodes.Where(e => !e.Ignored && missingEpisodes.Contains(e.EpisodeNumber)).OrderBy(o => o.EpisodeNumber))
            //{
            //    _episodeSearchJob.Start(notification, episode.EpisodeId, 0);
            //}
        }
    }
}