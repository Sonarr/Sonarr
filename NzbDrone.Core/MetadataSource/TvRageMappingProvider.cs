using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Providers;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.TvRage;

namespace NzbDrone.Core.MetadataSource
{
    public class TvRageMappingProvider
    {
        private readonly SceneMappingService _sceneMappingService;
        private readonly TvRageProxy _tvRageProxy;
        private readonly IEpisodeService _episodeService;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TvRageMappingProvider(SceneMappingService sceneMappingService,
                                TvRageProxy tvRageProxy, IEpisodeService episodeService)
        {
            _sceneMappingService = sceneMappingService;
            _tvRageProxy = tvRageProxy;
            _episodeService = episodeService;
        }

        public TvRageMappingProvider()
        {
        }

        public Series FindMatchingTvRageSeries(Series series)
        {
            var firstEpisode = _episodeService.GetEpisode(series.Id, 1, 1);

            var cleanName = _sceneMappingService.GetCleanName(series.Id);
            var results = _tvRageProxy.SearchSeries(series.Title);
            var result = ProcessResults(results, series, cleanName, firstEpisode);

            if (result != null)
            {
                logger.Trace("TV Rage: {0} matches TVDB: {1}", result.Name, series.Title);
                series.TvRageId = result.ShowId;
                series.TvRageTitle = result.Name;
                series.UtcOffset = _tvRageProxy.GetSeries(result.ShowId).UtcOffset;
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
