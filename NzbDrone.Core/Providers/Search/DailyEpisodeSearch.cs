using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.Providers.Search
{
    public class DailyEpisodeSearch : SearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public DailyEpisodeSearch(SeriesProvider seriesProvider, EpisodeProvider episodeProvider, DownloadProvider downloadProvider, IndexerProvider indexerProvider,
                             SceneMappingProvider sceneMappingProvider, AllowedDownloadSpecification allowedDownloadSpecification,
                             SearchHistoryProvider searchHistoryProvider)
                        : base(seriesProvider, episodeProvider, downloadProvider, indexerProvider, sceneMappingProvider, 
                               allowedDownloadSpecification, searchHistoryProvider)
            {
        }

        public override List<EpisodeParseResult> PerformSearch(Series series, dynamic options, ProgressNotification notification)
        {
            if (options.Episode == null)
                throw new ArgumentException("Episode is invalid");

            notification.CurrentMessage = "Looking for " + options.Episode;

            var reports = new List<EpisodeParseResult>();
            var title = GetSearchTitle(series);

            Parallel.ForEach(_indexerProvider.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    reports.AddRange(indexer.FetchDailyEpisode(title, options.Episode.AirDate));
                }

                catch (Exception e)
                {
                    logger.ErrorException(String.Format("An error has occurred while searching for {0} - {1:yyyy-MM-dd} from: {2}",
                                                         series.Title, options.Episode.AirDate, indexer.Name), e);
                }
            });

            return reports;
        }

        public override SearchHistoryItem CheckReport(Series series, dynamic options, EpisodeParseResult episodeParseResult,
                                                                SearchHistoryItem item)
        {
            Episode episode = options.Episode;

            if (!episodeParseResult.AirDate.HasValue || episodeParseResult.AirDate.Value != episode.AirDate.Value)
            {
                logger.Trace("Episode AirDate does not match searched episode number, skipping.");
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