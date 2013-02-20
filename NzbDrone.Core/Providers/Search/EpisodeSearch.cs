using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.Providers.Search
{
    public class EpisodeSearch : SearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public EpisodeSearch(ISeriesService seriesService, EpisodeService episodeService, DownloadProvider downloadProvider, IndexerProvider indexerProvider,
                             SceneMappingProvider sceneMappingProvider, AllowedDownloadSpecification allowedDownloadSpecification,
                             SearchHistoryProvider searchHistoryProvider, ISeriesRepository seriesRepository)
                        : base(seriesService,seriesRepository, episodeService, downloadProvider, indexerProvider, sceneMappingProvider, 
                               allowedDownloadSpecification, searchHistoryProvider)
            {
        }

        public EpisodeSearch()
        {
        }

        public override List<EpisodeParseResult> PerformSearch(Series series, dynamic options, ProgressNotification notification)
        {
            //Todo: Daily and Anime or separate them out?
            //Todo: Epsiodes that use scene numbering

            if (options.Episode == null)
                throw new ArgumentException("Episode is invalid");

            notification.CurrentMessage = "Looking for " + options.Episode;

            var reports = new List<EpisodeParseResult>();
            var title = GetSearchTitle(series);

            var seasonNumber = options.Episode.SeasonNumber;
            var episodeNumber = options.Episode.EpisodeNumber;

            if (series.UseSceneNumbering)
            {
                if(options.Episode.SceneSeasonNumber > 0 && options.Episode.SceneEpisodeNumber > 0)
                {
                    logger.Trace("Using Scene Numbering for: {0}", options.Episode);
                    seasonNumber = options.Episode.SceneSeasonNumber;
                    episodeNumber = options.Episode.SceneEpisodeNumber;
                }
            }

            Parallel.ForEach(_indexerProvider.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    reports.AddRange(indexer.FetchEpisode(title, seasonNumber, episodeNumber));
                }

                catch (Exception e)
                {
                    logger.ErrorException(String.Format("An error has occurred while searching for {0}-S{1:00}E{2:00} from: {3}",
                                                         series.Title, options.Episode.SeasonNumber, options.Episode.EpisodeNumber, indexer.Name), e);
                }
            });

            return reports;
        }

        public override SearchHistoryItem CheckReport(Series series, dynamic options, EpisodeParseResult episodeParseResult,
                                                                SearchHistoryItem item)
        {
            if(series.UseSceneNumbering && options.Episode.SeasonNumber > 0 && options.Episode.EpisodeNumber > 0)
            {
                if (options.Episode.SceneSeasonNumber != episodeParseResult.SeasonNumber)
                {
                    logger.Trace("Season number does not match searched season number, skipping.");
                    item.SearchError = ReportRejectionType.WrongSeason;

                    return item;
                }

                if (!episodeParseResult.EpisodeNumbers.Contains(options.Episode.SceneEpisodeNumber))
                {
                    logger.Trace("Episode number does not match searched episode number, skipping.");
                    item.SearchError = ReportRejectionType.WrongEpisode;

                    return item;
                }

                return item;
            }

            if(options.Episode.SeasonNumber != episodeParseResult.SeasonNumber)
            {
                logger.Trace("Season number does not match searched season number, skipping.");
                item.SearchError = ReportRejectionType.WrongSeason;

                return item;
            }

            if (!episodeParseResult.EpisodeNumbers.Contains(options.Episode.EpisodeNumber))
            {
                logger.Trace("Episode number does not match searched episode number, skipping.");
                item.SearchError = ReportRejectionType.WrongEpisode;

                return item;
            }

            return item;
        }

        protected override void FinalizeSearch(Series series, dynamic options, Boolean reportsFound, ProgressNotification notification)
        {
            logger.Warn("Unable to find {0} in any of indexers.", options.Episode);

            notification.CurrentMessage = reportsFound ? String.Format("Sorry, couldn't find {0}, that matches your preferences.", options.Episode)
                                                        : String.Format("Sorry, couldn't find {0} in any of indexers.", options.Episode);
        }
    }
}
