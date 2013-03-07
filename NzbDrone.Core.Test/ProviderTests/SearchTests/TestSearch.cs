using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Providers.Search;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests
{
    public class TestSearch : SearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TestSearch(ISeriesService seriesService, IEpisodeService episodeService, DownloadProvider downloadProvider,
                          IIndexerService indexerService, SceneMappingService sceneMappingService,
                          DownloadDirector downloadDirector, ISeriesRepository seriesRepository)
            : base(seriesRepository, episodeService, downloadProvider, indexerService, sceneMappingService,
                   downloadDirector)
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

            Parallel.ForEach(_indexerService.GetEnabledIndexers(), indexer =>
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

        public override bool IsEpisodeMatch(Series series, dynamic options, EpisodeParseResult episodeParseResult)
        {
            return true;
        }

    }
}
