using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.DecisionEngine;

namespace NzbDrone.Core.Providers.Search
{
    public class PartialSeasonSearch : SearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PartialSeasonSearch(IEpisodeService episodeService, DownloadProvider downloadProvider, IIndexerService indexerService,
                             SceneMappingService sceneMappingService, DownloadDirector downloadDirector,
                             ISeriesRepository seriesRepository)
            : base(seriesRepository, episodeService, downloadProvider, indexerService, sceneMappingService,
                   downloadDirector)
        {
        }

        public PartialSeasonSearch()
        {
        }

        public override List<EpisodeParseResult> PerformSearch(Series series, dynamic options, ProgressNotification notification)
        {
            if (options.SeasonNumber == null || options.SeasonNumber < 0)
                throw new ArgumentException("SeasonNumber is invalid");

            if (options.Episodes == null)
                throw new ArgumentException("Episodes were not provided");

            List<Episode> episodes = options.Episodes;

            if (!episodes.Any())
                throw new ArgumentException("Episodes were not provided");

            notification.CurrentMessage = String.Format("Looking for {0} - Season {1}", series.Title, options.SeasonNumber);

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
                            reports.AddRange(indexer.FetchPartialSeason(title, options.SeasonNumber, prefix));
                        }
                    }

                    catch (Exception e)
                    {
                        logger.ErrorException(
                                                String.Format(
                                                            "An error has occurred while searching for {0} Season {1:00} Prefix: {2} from: {3}",
                                                            series.Title, options.SeasonNumber, prefix, indexer.Name),
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
