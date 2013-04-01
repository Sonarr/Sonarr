using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.DataAugmentation;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class PartialSeasonSearch : IndexerSearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PartialSeasonSearch(IEpisodeService episodeService, IDownloadProvider downloadProvider, IIndexerService indexerService,
                             ISceneMappingService sceneMappingService, IDownloadDirector downloadDirector,
                             ISeriesRepository seriesRepository)
            : base(seriesRepository, episodeService, downloadProvider, indexerService, sceneMappingService,
                   downloadDirector)
        {
        }

        public PartialSeasonSearch()
        {
        }

        public override List<EpisodeParseResult> PerformSearch(Series series, List<Episode> episodes, ProgressNotification notification)
        {
            var seasons = episodes.Select(c => c.SeasonNumber).Distinct().ToList();

            if (seasons.Count > 1)
            {
                throw new ArgumentOutOfRangeException("episodes", "episode list contains episodes from more than one season");
            }

            var seasonNumber = seasons[0];
            notification.CurrentMessage = String.Format("Looking for {0} - Season {1}", series.Title, seasonNumber);

            var reports = new List<EpisodeParseResult>();
            object reportsLock = new object();

            var title = GetSearchTitle(series);
            var prefixes = GetEpisodeNumberPrefixes(episodes.Select(e => e.EpisodeNumber));

            foreach (var p in prefixes)
            {
                var prefix = p;

                Parallel.ForEach(_indexerService.GetEnabledIndexers(), indexer =>
                {
                    try
                    {
                        lock (reportsLock)
                        {
                            reports.AddRange(indexer.FetchPartialSeason(title, seasonNumber, prefix));
                        }
                    }

                    catch (Exception e)
                    {
                        logger.ErrorException(
                                                String.Format(
                                                            "An error has occurred while searching for {0} Season {1:00} Prefix: {2} from: {3}",
                                                            series.Title, seasonNumber, prefix, indexer.Name),
                                                e);
                    }
                });
            }

            return reports;
        }

        public override bool IsEpisodeMatch(Series series, dynamic options, EpisodeParseResult episodeParseResult)
        {
            if (options.SeasonNumber != episodeParseResult.SeasonNumber)
            {
                logger.Trace("Season number does not match searched season number, skipping.");
                return false;
            }

            return true;
        }


        private List<int> GetEpisodeNumberPrefixes(IEnumerable<int> episodeNumbers)
        {
            var results = new List<int>();

            foreach (var i in episodeNumbers)
            {
                results.Add(i / 10);
            }

            return results.Distinct().ToList();
        }
    }
}
