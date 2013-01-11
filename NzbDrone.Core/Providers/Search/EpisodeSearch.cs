using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.Providers.Search
{
    public class EpisodeSearch : SearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public EpisodeSearch(EpisodeProvider episodeProvider, DownloadProvider downloadProvider,
                             SeriesProvider seriesProvider, IndexerProvider indexerProvider,
                             SceneMappingProvider sceneMappingProvider, UpgradePossibleSpecification upgradePossibleSpecification,
                             AllowedDownloadSpecification allowedDownloadSpecification, SearchHistoryProvider searchHistoryProvider)
                        : base(episodeProvider, downloadProvider, seriesProvider, indexerProvider, sceneMappingProvider, 
                                upgradePossibleSpecification, allowedDownloadSpecification, searchHistoryProvider)
            {
        }

        protected override List<EpisodeParseResult> Search(Series series, dynamic options)
        {
            if (options == null)
                throw new ArgumentNullException(options);

            if (options.SeasonNumber < 0)
                throw new ArgumentException("SeasonNumber is invalid");

            if (options.EpisodeNumber < 0)
                throw new ArgumentException("EpisodeNumber is invalid");

            var reports = new List<EpisodeParseResult>();
            var title = GetSeriesTitle(series);

            Parallel.ForEach(_indexerProvider.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    reports.AddRange(indexer.FetchEpisode(title, options.SeasonNumber, options.EpisodeNumber));
                }

                catch (Exception e)
                {
                    logger.ErrorException(String.Format("An error has occurred while searching for {0}-S{1:00}E{2:00} from: {3}",
                                                         series.Title, options.SeasonNumber, options.EpisodeNumber, indexer.Name), e);
                }
            });

            return reports;
        }

        protected override SearchHistoryItem CheckEpisode(Series series, List<Episode> episodes, EpisodeParseResult episodeParseResult,
                                                          SearchHistoryItem item)
        {
            throw new NotImplementedException();
        }
    }
}
