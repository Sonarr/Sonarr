using System.Linq;
using System;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Providers.Search;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class EpisodeSearchJob : IJob
    {
        private readonly EpisodeService _episodeService;
        private readonly UpgradePossibleSpecification _upgradePossibleSpecification;
        private readonly EpisodeSearch _episodeSearch;
        private readonly DailyEpisodeSearch _dailyEpisodeSearch;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public EpisodeSearchJob(EpisodeService episodeService, UpgradePossibleSpecification upgradePossibleSpecification,
                                EpisodeSearch episodeSearch, DailyEpisodeSearch dailyEpisodeSearch)
        {
            if(dailyEpisodeSearch == null) throw new ArgumentNullException("dailyEpisodeSearch");
            _episodeService = episodeService;
            _upgradePossibleSpecification = upgradePossibleSpecification;
            _episodeSearch = episodeSearch;
            _dailyEpisodeSearch = dailyEpisodeSearch;
        }

        public EpisodeSearchJob()
        {
            
        }

        public string Name
        {
            get { return "Episode Search"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            if (options == null || options.EpisodeId <= 0)
                throw new ArgumentException("options");

            Episode episode = _episodeService.GetEpisode(options.EpisodeId);

            if (episode == null)
            {
                logger.Error("Unable to find an episode {0} in database", options.EpisodeId);
                return;
            }

            if (!_upgradePossibleSpecification.IsSatisfiedBy(episode))
            {
                logger.Info("Search for {0} was aborted, file in disk meets or exceeds Profile's Cutoff", episode);
                notification.CurrentMessage = String.Format("Skipping search for {0}, the file you have is already at cutoff", episode);
                return;
            }

            if (episode.Series.SeriesType == SeriesType.Daily)
            {
                if (!episode.AirDate.HasValue)
                {
                    logger.Warn("AirDate is not Valid for: {0}", episode);
                    notification.CurrentMessage = String.Format("Search for {0} Failed, AirDate is invalid", episode);
                    return;
                }

                _dailyEpisodeSearch.Search(episode.Series, new { Episode = episode }, notification);
            }

            else
            {
                _episodeSearch.Search(episode.Series, new { Episode = episode }, notification);    
            }
        }
    }
}