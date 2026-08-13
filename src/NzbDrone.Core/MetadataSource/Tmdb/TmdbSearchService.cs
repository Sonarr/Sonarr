using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource.Tmdb
{
    public interface ITmdbSearchService
    {
        bool IsEnabled();
        Task<List<Series>> Search(string title);
        Task<Series> GetSeriesByTmdbId(int tmdbId);
    }

    public class TmdbSearchService : ITmdbSearchService
    {
        private readonly ITmdbClient _tmdbClient;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public TmdbSearchService(ITmdbClient tmdbClient,
                                 IConfigService configService,
                                 Logger logger)
        {
            _tmdbClient = tmdbClient;
            _configService = configService;
            _logger = logger;
        }

        public bool IsEnabled()
        {
            return _configService.TmdbEnabled &&
                   _configService.MetadataLanguage != (int)Language.English &&
                   !string.IsNullOrWhiteSpace(_configService.TmdbApiKey);
        }

        private string GetTargetLanguageCode()
        {
            var language = Language.FindById(_configService.MetadataLanguage);
            var isoLanguage = IsoLanguages.Get(language);
            return isoLanguage?.TwoLetterCode ?? "de";
        }

        public async Task<Series> GetSeriesByTmdbId(int tmdbId)
        {
            var languageCode = GetTargetLanguageCode();
            var tvdbId = await _tmdbClient.GetTvdbIdFromTmdbId(tmdbId);
            var translation = await _tmdbClient.GetSeriesTranslation(tmdbId, languageCode);

            var series = new Series
            {
                TvdbId = tvdbId ?? 0,
                TmdbId = tmdbId,
                Title = translation?.Data?.Name ?? "TMDB " + tmdbId,
                Overview = translation?.Data?.Overview,
                Images = new List<MediaCover.MediaCover>(),
                Seasons = new List<Season>()
            };

            series.CleanTitle = Parser.Parser.CleanSeriesTitle(series.Title);
            series.SortTitle = SeriesTitleNormalizer.Normalize(series.Title, series.TvdbId);

            return series;
        }

        public async Task<List<Series>> Search(string title)
        {
            var languageCode = GetTargetLanguageCode();

            // TMDB's search does not accept years in parentheses like "1883 (2021)".
            // Strip " (2021)", "(2021)", and trailing/leading whitespace before querying.
            var cleanTitle = System.Text.RegularExpressions.Regex.Replace(title, @"\s*\(\d{4}\)\s*$", string.Empty).Trim();

            if (cleanTitle != title)
            {
                _logger.Debug("TMDB search: normalized title '{0}' -> '{1}'", title, cleanTitle);
            }

            var results = await _tmdbClient.Search(cleanTitle, languageCode);

            var seriesList = new List<Series>();

            foreach (var result in results)
            {
                var tvdbId = await _tmdbClient.GetTvdbIdFromTmdbId(result.Id);

                if (tvdbId == null || tvdbId == 0)
                {
                    _logger.Debug("Skipping TMDB result {Id} ({Name}) - no TVDB id found", result.Id, result.Name);
                    continue;
                }

                var series = new Series
                {
                    TvdbId = tvdbId.Value,
                    TmdbId = result.Id,
                    Title = result.Name,
                    Overview = result.Overview,
                    Images = new List<MediaCover.MediaCover>(),
                    Seasons = new List<Season>()
                };

                series.CleanTitle = Parser.Parser.CleanSeriesTitle(series.Title);
                series.SortTitle = SeriesTitleNormalizer.Normalize(series.Title, series.TvdbId);

                seriesList.Add(series);
            }

            _logger.Debug("TMDB search for '{0}' returned {1} results", title, seriesList.Count);

            return seriesList;
        }
    }
}
