using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.DecisionEngine;

namespace NzbDrone.Core.Providers.Search
{
    public class DailyEpisodeSearch : SearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public DailyEpisodeSearch(IEpisodeService episodeService, DownloadProvider downloadProvider, IIndexerService indexerService,
                             SceneMappingService sceneMappingService, DownloadDirector downloadDirector,
                             ISeriesRepository seriesRepository)
            : base(seriesRepository, episodeService, downloadProvider, indexerService, sceneMappingService,
                   downloadDirector)
        {
        }

        public DailyEpisodeSearch()
        {
        }

        public override List<EpisodeParseResult> PerformSearch(Series series, dynamic options, ProgressNotification notification)
        {
            if (options.Episode == null)
                throw new ArgumentException("Episode is invalid");

            notification.CurrentMessage = "Looking for " + options.Episode;

            var reports = new List<EpisodeParseResult>();
            var title = GetSearchTitle(series);

            Parallel.ForEach(_indexerService.GetEnabledIndexers(), indexer =>
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