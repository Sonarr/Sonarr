using System;
using System.Collections.Generic;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource
{
    public class MetadataSourceSelector : IProvideSeriesInfo, ISearchForNewSeries
    {
        private readonly IConfigService _configService;
        private readonly ITvdbMetadataSource _tvdbMetadataSource;
        private readonly ITmdbMetadataSource _tmdbMetadataSource;

        public MetadataSourceSelector(IConfigService configService,
                                      ITvdbMetadataSource tvdbMetadataSource,
                                      ITmdbMetadataSource tmdbMetadataSource)
        {
            _configService = configService;
            _tvdbMetadataSource = tvdbMetadataSource;
            _tmdbMetadataSource = tmdbMetadataSource;
        }

        public Tuple<Series, List<Episode>> GetSeriesInfo(int tvdbSeriesId, int tmdbSeriesId = 0)
        {
            if (_configService.MetadataSource == MetadataSourceType.Tmdb)
            {
                return _tmdbMetadataSource.GetSeriesInfo(tvdbSeriesId, tmdbSeriesId);
            }

            return _tvdbMetadataSource.GetSeriesInfo(tvdbSeriesId, tmdbSeriesId);
        }

        public List<Series> SearchForNewSeries(string title)
        {
            if (_configService.MetadataSource == MetadataSourceType.Tmdb)
            {
                return _tmdbMetadataSource.SearchForNewSeries(title);
            }

            return _tvdbMetadataSource.SearchForNewSeries(title);
        }

        public List<Series> SearchForNewSeriesByImdbId(string imdbId)
        {
            if (_configService.MetadataSource == MetadataSourceType.Tmdb)
            {
                return _tmdbMetadataSource.SearchForNewSeriesByImdbId(imdbId);
            }

            return _tvdbMetadataSource.SearchForNewSeriesByImdbId(imdbId);
        }

        public List<Series> SearchForNewSeriesByAniListId(int aniListId)
        {
            return _tvdbMetadataSource.SearchForNewSeriesByAniListId(aniListId);
        }

        public List<Series> SearchForNewSeriesByTmdbId(int tmdbId)
        {
            if (_configService.MetadataSource == MetadataSourceType.Tmdb)
            {
                return _tmdbMetadataSource.SearchForNewSeriesByTmdbId(tmdbId);
            }

            return _tvdbMetadataSource.SearchForNewSeriesByTmdbId(tmdbId);
        }

        public List<Series> SearchForNewSeriesByMyAnimeListId(int malId)
        {
            return _tvdbMetadataSource.SearchForNewSeriesByMyAnimeListId(malId);
        }
    }
}
