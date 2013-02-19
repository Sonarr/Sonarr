using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Providers.Search;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests
{
    public class TestSearch : SearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TestSearch(SeriesProvider seriesProvider, EpisodeProvider episodeProvider, DownloadProvider downloadProvider, 
                          IndexerProvider indexerProvider, SceneMappingProvider sceneMappingProvider,
                          AllowedDownloadSpecification allowedDownloadSpecification, SearchHistoryProvider searchHistoryProvider)
                          : base(seriesProvider, episodeProvider, downloadProvider, indexerProvider, sceneMappingProvider, 
                                 allowedDownloadSpecification, searchHistoryProvider)
        {
        }

        public override List<EpisodeParseResult> PerformSearch(Series series, dynamic options, Model.Notification.ProgressNotification notification)
        {
            if (options.Episode == null)
                throw new ArgumentException("Episode is invalid");

            notification.CurrentMessage = "Looking for " + options.Episode;

            var reports = new List<EpisodeParseResult>();
            var title = GetSearchTitle(series);

            var seasonNumber = options.Episode.SeasonNumber;
            var episodeNumber = options.Episode.EpisodeNumber;

            Parallel.ForEach(_indexerProvider.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    reports.AddRange(indexer.FetchEpisode(title, seasonNumber, episodeNumber));
                }

                catch (Exception e)
                {
                    logger.ErrorException(String.Format("An error has occurred while searching for {0}-S{1:00}E{2:00} from: {3}",
                                                         series.Title, options.SeasonNumber, options.EpisodeNumber, indexer.Name), e);
                }
            });

            return reports;
        }

        public override SearchHistoryItem CheckReport(Series series, dynamic options, EpisodeParseResult episodeParseResult, Repository.Search.SearchHistoryItem item)
        {
            return item;
        }

        protected override void FinalizeSearch(Series series, dynamic options, bool reportsFound, Model.Notification.ProgressNotification notification)
        {
            logger.Warn("Unable to find {0} in any of indexers.", series.Title);
        }
    }
}
