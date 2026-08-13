using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource.Tmdb
{
    public interface ITmdbTranslationService
    {
        bool IsEnabled();
        void ApplyTranslations(Series series, List<Episode> episodes);
    }

    public class TmdbTranslationService : ITmdbTranslationService
    {
        private readonly ITmdbClient _tmdbClient;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public TmdbTranslationService(ITmdbClient tmdbClient,
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

        public void ApplyTranslations(Series series, List<Episode> episodes)
        {
            if (!IsEnabled())
            {
                return;
            }

            var languageCode = GetTargetLanguageCode();

            try
            {
                var tmdbId = series.TmdbId != 0 ? (int?)series.TmdbId : _tmdbClient.GetTmdbIdFromTvdbId(series.TvdbId).GetAwaiter().GetResult();
                if (tmdbId == null)
                {
                    _logger.Debug("No TMDB id found for tvdb {TvdbId}, skipping translations", series.TvdbId);
                    return;
                }

                var seriesTranslation = _tmdbClient.GetSeriesTranslation(tmdbId.Value, languageCode).GetAwaiter().GetResult();

                if (seriesTranslation?.Data != null)
                {
                    if (seriesTranslation.Data.Name.IsNotNullOrWhiteSpace())
                    {
                        series.Title = seriesTranslation.Data.Name;
                        series.CleanTitle = Parser.Parser.CleanSeriesTitle(series.Title);
                        series.SortTitle = SeriesTitleNormalizer.Normalize(series.Title, series.TvdbId);
                    }

                    if (seriesTranslation.Data.Overview.IsNotNullOrWhiteSpace())
                    {
                        series.Overview = seriesTranslation.Data.Overview;
                    }
                }

                foreach (var episode in episodes.Where(e => e.SeasonNumber > 0))
                {
                    var episodeTranslation = _tmdbClient.GetEpisodeTranslation(tmdbId.Value, episode.SeasonNumber, episode.EpisodeNumber, languageCode).GetAwaiter().GetResult();

                    if (episodeTranslation?.Data != null)
                    {
                        if (episodeTranslation.Data.Name.IsNotNullOrWhiteSpace())
                        {
                            episode.Title = episodeTranslation.Data.Name;
                        }

                        if (episodeTranslation.Data.Overview.IsNotNullOrWhiteSpace())
                        {
                            episode.Overview = episodeTranslation.Data.Overview;
                        }
                    }
                }

                _logger.Debug("Applied TMDB translations for series [{0}] {1}", series.TvdbId, series.Title);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to apply TMDB translations for series [{0}] {1}. Falling back to SkyHook data.", series.TvdbId, series.Title);
            }
        }
    }
}
