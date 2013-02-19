using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.Providers.Search
{
    public class PartialSeasonSearch : SearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PartialSeasonSearch(SeriesProvider seriesProvider, EpisodeProvider episodeProvider, DownloadProvider downloadProvider, IndexerProvider indexerProvider,
                             SceneMappingProvider sceneMappingProvider, AllowedDownloadSpecification allowedDownloadSpecification,
                             SearchHistoryProvider searchHistoryProvider)
                        : base(seriesProvider, episodeProvider, downloadProvider, indexerProvider, sceneMappingProvider, 
                               allowedDownloadSpecification, searchHistoryProvider)
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

            foreach(var p in prefixes)
            {
                var prefix = p;

                Parallel.ForEach(_indexerProvider.GetEnabledIndexers(), indexer =>
                {
                    try
                    {
                        lock(reportsLock)
                        {
                            reports.AddRange(indexer.FetchPartialSeason(title, options.SeasonNumber, prefix));
                        }
                    }

                    catch(Exception e)
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

        public override SearchHistoryItem CheckReport(Series series, dynamic options, EpisodeParseResult episodeParseResult,
                                                                SearchHistoryItem item)
        {
            if(options.SeasonNumber != episodeParseResult.SeasonNumber)
            {
                logger.Trace("Season number does not match searched season number, skipping.");
                item.SearchError = ReportRejectionType.WrongSeason;

                return item;
            }

            return item;
        }

        protected override void FinalizeSearch(Series series, dynamic options, Boolean reportsFound, ProgressNotification notification)
        {
            logger.Warn("Unable to find {0} - Season {1} in any of indexers.", series.Title, options.SeasonNumber);

            notification.CurrentMessage = reportsFound ? String.Format("Sorry, couldn't find {0} Season {1:00}, that matches your preferences.", series.Title, options.SeasonNumber)
                                                        : String.Format("Sorry, couldn't find {0} Season {1:00} in any of indexers.", series.Title, options.SeasonNumber);
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
