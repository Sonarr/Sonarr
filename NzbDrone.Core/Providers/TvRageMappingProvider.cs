using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.TvRage;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class TvRageMappingProvider
    {
        private readonly SceneMappingProvider _sceneMappingProvider;
        private readonly TvRageProvider _tvRageProvider;
        private readonly IEpisodeService _episodeService;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TvRageMappingProvider(SceneMappingProvider sceneMappingProvider,
                                TvRageProvider tvRageProvider, IEpisodeService episodeService)
        {
            _sceneMappingProvider = sceneMappingProvider;
            _tvRageProvider = tvRageProvider;
            _episodeService = episodeService;
        }

        public TvRageMappingProvider()
        {
        }

        public Series FindMatchingTvRageSeries(Series series)
        {
            var firstEpisode = _episodeService.GetEpisode(series.OID, 1, 1);

            var cleanName = _sceneMappingProvider.GetCleanName(series.OID);
            var results = _tvRageProvider.SearchSeries(series.Title);
            var result = ProcessResults(results, series, cleanName, firstEpisode);

            if (result != null)
            {
                logger.Trace("TV Rage: {0} matches TVDB: {1}", result.Name, series.Title);
                series.TvRageId = result.ShowId;
                series.TvRageTitle = result.Name;
                series.UtcOffset = _tvRageProvider.GetSeries(result.ShowId).UtcOffset;
            }

            return series;
        }

        public TvRageSearchResult ProcessResults(IList<TvRageSearchResult> searchResults, Series series, string sceneCleanName, Episode firstEpisode)
        {
            foreach (var result in searchResults)
            {
                if (Parser.NormalizeTitle(result.Name).Equals(series.CleanTitle))
                    return result;

                if (!String.IsNullOrWhiteSpace(sceneCleanName) && Parser.NormalizeTitle(result.Name).Equals(sceneCleanName))
                    return result;

                if (series.FirstAired.HasValue && result.Started == series.FirstAired.Value)
                    return result;

                if (firstEpisode != null && firstEpisode.AirDate.HasValue && result.Started == firstEpisode.AirDate.Value)
                    return result;
            }

            return null;
        }
    }
}
