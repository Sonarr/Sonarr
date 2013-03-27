using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    public class TestSearch : IndexerSearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TestSearch(IEpisodeService episodeService, IDownloadProvider downloadProvider,
                          IIndexerService indexerService, ISceneMappingService sceneMappingService,
                          IDownloadDirector downloadDirector, ISeriesRepository seriesRepository)
            : base(seriesRepository, episodeService, downloadProvider, indexerService, sceneMappingService,
                   downloadDirector)
        {
        }

        public override List<EpisodeParseResult> PerformSearch(Series series, List<Episode> episodes, Model.Notification.ProgressNotification notification)
        {
            var episode = episodes.Single();

            var reports = new List<EpisodeParseResult>();
            var title = GetSearchTitle(series);

            var seasonNumber = episode.SeasonNumber;
            var episodeNumber = episode.EpisodeNumber;

            Parallel.ForEach(_indexerService.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    reports.AddRange(indexer.FetchEpisode(title, seasonNumber, episodeNumber));
                }

                catch (Exception e)
                {
                    logger.ErrorException(String.Format("An error has occurred while searching for {0}-S{1:00}E{2:00} from: {3}",
                                                         series.Title, episode.SeasonNumber, episode.EpisodeNumber, indexer.Name), e);
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
