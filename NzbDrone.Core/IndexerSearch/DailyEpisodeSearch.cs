using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class DailyEpisodeSearch : IndexerSearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public DailyEpisodeSearch(IEpisodeService episodeService, DownloadProvider downloadProvider, IIndexerService indexerService,
                             ISceneMappingService sceneMappingService, IDownloadDirector downloadDirector,
                             ISeriesRepository seriesRepository)
            : base(seriesRepository, episodeService, downloadProvider, indexerService, sceneMappingService,
                   downloadDirector)
        {
        }

        public DailyEpisodeSearch()
        {
        }

        public override List<EpisodeParseResult> PerformSearch(Series series, List<Episode> episodes, ProgressNotification notification)
        {
            var episode = episodes.Single();

            notification.CurrentMessage = "Looking for " + episode;

            var reports = new List<EpisodeParseResult>();
            var title = GetSearchTitle(series);

            Parallel.ForEach(_indexerService.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    reports.AddRange(indexer.FetchDailyEpisode(title, episode.AirDate.Value));
                }

                catch (Exception e)
                {
                    logger.ErrorException(String.Format("An error has occurred while searching for {0} - {1:yyyy-MM-dd} from: {2}",
                                                         series.Title, episode.AirDate, indexer.Name), e);
                }
            });

            return reports;
        }

        public override bool IsEpisodeMatch(Series series, dynamic options, EpisodeParseResult episodeParseResult)
        {
            Episode episode = options.Episode;

            if (!episodeParseResult.AirDate.HasValue || episodeParseResult.AirDate.Value != episode.AirDate.Value)
            {
                logger.Trace("Episode AirDate does not match searched episode number, skipping.");
                return false;
            }

            return true;
        }

    }
}