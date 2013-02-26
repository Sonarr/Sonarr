using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class SeasonSearchJob : IJob
    {
        private readonly SearchProvider _searchProvider;
        private readonly EpisodeSearchJob _episodeSearchJob;
        private readonly IEpisodeService _episodeService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SeasonSearchJob(SearchProvider searchProvider, EpisodeSearchJob episodeSearchJob,
                                IEpisodeService episodeService)
        {
            _searchProvider = searchProvider;
            _episodeSearchJob = episodeSearchJob;
            _episodeService = episodeService;
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

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            if (options == null || options.SeriesId <= 0)
                throw new ArgumentException("options");

            if (options.SeasonNumber < 0)
                throw new ArgumentException("options.SeasonNumber");

            //Perform a Partial Season Search - Because a full season search is a waste
            //3 searches should guarentee results, (24 eps) versus, a potential 4 to get the same eps.
            List<int> successes = _searchProvider.PartialSeasonSearch(notification, options.SeriesId, options.SeasonNumber);

            //This causes issues with Newznab
            //if (successes.Count == 0)
            //    return;

            Logger.Debug("Getting episodes from database for series: {0} and season: {1}", options.SeriesId, options.SeasonNumber);
            IList<Episode> episodes = _episodeService.GetEpisodesBySeason((int)options.SeriesId, (int)options.SeasonNumber);

            if (episodes == null || episodes.Count == 0)
            {
                Logger.Warn("No episodes in database found for series: {0} and season: {1}.", options.SeriesId, options.SeasonNumber);
                return;
            }

            if (episodes.Count == successes.Count)
                return;

            var missingEpisodes = episodes.Select(e => e.Id).Except(successes).ToList();

            foreach (var episode in episodes.Where(e => !e.Ignored && missingEpisodes.Contains(e.Id)).OrderBy(o => o.EpisodeNumber))
            {
                _episodeSearchJob.Start(notification, new { EpisodeId = episode.Id });
            }
        }
    }
}