using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MetadataSource.Tmdb
{
    public interface ITmdbClient
    {
        Task<TmdbTranslation> GetSeriesTranslation(int tmdbId, string language);
        Task<List<TmdbTranslation>> GetSeriesTranslations(int tmdbId);
        Task<TmdbTranslation> GetEpisodeTranslation(int tmdbId, int seasonNumber, int episodeNumber, string language);
        Task<List<TmdbSearchResult>> Search(string query, string language);
        Task<int?> GetTvdbIdFromTmdbId(int tmdbId);
        Task<int?> GetTmdbIdFromTvdbId(int tvdbId);
    }

    public class TmdbClient : ITmdbClient
    {
        private const string BaseUrl = "https://api.themoviedb.org/3";
        private const int MaxConcurrency = 5;

        private readonly IHttpClient _httpClient;
        private readonly IConfigService _configService;
        private readonly Logger _logger;
        private readonly SemaphoreSlim _throttle;
        private readonly ICached<int?> _tvdbIdCache;
        private readonly ICached<int?> _tmdbIdCache;

        public TmdbClient(IHttpClient httpClient,
                          IConfigService configService,
                          ICacheManager cacheManager,
                          Logger logger)
        {
            _httpClient = httpClient;
            _configService = configService;
            _logger = logger;
            _throttle = new SemaphoreSlim(MaxConcurrency);
            _tvdbIdCache = cacheManager.GetCache<int?>(GetType(), "tmdb-tvdb-ids");
            _tmdbIdCache = cacheManager.GetCache<int?>(GetType(), "tvdb-tmdb-ids");
        }

        private string ApiKey => _configService.TmdbApiKey;

        private bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);

        private HttpRequest BuildRequest(string path, string language = null)
        {
            // TheMovieDB supports two auth modes:
            //  - v3 API-Key (hex string, e.g. abc123...)  → send as "api_key" query parameter
            //  - v4 Read Access Token (starts with "eyJ")  → send as "Authorization: Bearer <token>" header
            var isV4Token = ApiKey.StartsWith("eyJ", StringComparison.OrdinalIgnoreCase);

            var separator = path.Contains('?') ? "&" : "?";
            var url = $"{BaseUrl}{path}";

            if (!isV4Token)
            {
                url += separator + "api_key=" + Uri.EscapeDataString(ApiKey);
            }

            if (language.IsNotNullOrWhiteSpace())
            {
                url += (url.Contains('?') ? "&" : "?") + "language=" + NormalizeLanguage(language);
            }

            var request = new HttpRequest(url);
            request.SuppressHttpError = true;
            request.LogHttpError = false;

            if (isV4Token)
            {
                request.Headers.Add("Authorization", $"Bearer {ApiKey}");
            }

            return request;
        }

        public async Task<TmdbTranslation> GetSeriesTranslation(int tmdbId, string language)
        {
            var translations = await GetSeriesTranslations(tmdbId);
            var normalized = NormalizeLanguage(language);
            return translations?.FirstOrDefault(t => t.Iso639_1 == normalized);
        }

        private static string NormalizeLanguage(string language)
        {
            // Sonarr's language list uses ISO 639-1 two-letter codes; TMDB uses its own
            // labels in some cases. Map the most common differences so any selected
            // language works with the TMDB search/translations endpoints.
            return language switch
            {
                // Hebrew: Sonarr may expose the legacy code
                "he" => "he",
                "iw" => "he",

                // Indonesian: legacy ISO code
                "in" => "id",

                // Yiddish: legacy code
                "ji" => "yi",

                // Chinese: TMDB uses plain "zh" for the search language
                "zh" => "zh",

                // Norwegian: both codes map to TMDB's "nb"/"no" as provided
                "no" => "no",
                "nb" => "nb",

                // Portuguese: plain "pt" works and returns generic/pt-BR where available
                "pt" => "pt",

                // Greek, Czech, etc. are fine as-is
                _ => language
            };
        }

        public async Task<List<TmdbTranslation>> GetSeriesTranslations(int tmdbId)
        {
            return await ExecuteThrottledAsync(async () =>
            {
                if (!HasApiKey)
                {
                    return new List<TmdbTranslation>();
                }

                var request = BuildRequest($"/tv/{tmdbId}/translations");
                var response = await Task.Run(() => _httpClient.Get<TmdbTranslationsResponse>(request));

                if (response.HasHttpError)
                {
                    _logger.Warn("TMDB translations error for {Id}: {Status}", tmdbId, response.StatusCode);
                    return new List<TmdbTranslation>();
                }

                return response.Resource?.Translations ?? new List<TmdbTranslation>();
            });
        }

        public async Task<TmdbTranslation> GetEpisodeTranslation(int tmdbId, int seasonNumber, int episodeNumber, string language)
        {
            return await ExecuteThrottledAsync(async () =>
            {
                if (!HasApiKey)
                {
                    return null;
                }

                var request = BuildRequest($"/tv/{tmdbId}/season/{seasonNumber}/episode/{episodeNumber}/translations");
                var response = await Task.Run(() => _httpClient.Get<TmdbTranslationsResponse>(request));

                if (response.HasHttpError)
                {
                    _logger.Debug("TMDB episode translations error for {Id} S{Season}E{Episode}: {Status}", tmdbId, seasonNumber, episodeNumber, response.StatusCode);
                    return null;
                }

                return response.Resource?.Translations?.FirstOrDefault(t => t.Iso639_1 == NormalizeLanguage(language));
            });
        }

        public async Task<List<TmdbSearchResult>> Search(string query, string language)
        {
            return await ExecuteThrottledAsync(async () =>
            {
                if (!HasApiKey)
                {
                    return new List<TmdbSearchResult>();
                }

                var request = BuildRequest($"/search/tv?query={Uri.EscapeDataString(query)}", language);
                var response = await Task.Run(() => _httpClient.Get<TmdbSearchResponse>(request));

                if (response.HasHttpError)
                {
                    _logger.Warn("TMDB search error: {Status} - {Content}", response.StatusCode, response.Content);
                    return new List<TmdbSearchResult>();
                }

                _logger.Debug("TMDB search response URL: {Url} Status: {Status} ResourceNull: {Null} ContentLength: {Len}", request.Url, response.StatusCode, response.Resource == null, response.Content?.Length ?? 0);

                if (response.Resource == null)
                {
                    _logger.Warn("TMDB search response was not parsed. Content: {Content}", response.Content);
                    return new List<TmdbSearchResult>();
                }

                return response.Resource.Results ?? new List<TmdbSearchResult>();
            });
        }

        public async Task<int?> GetTmdbIdFromTvdbId(int tvdbId)
        {
            var cached = _tmdbIdCache.Find(tvdbId.ToString());
            if (cached.HasValue)
            {
                return cached;
            }

            return await ExecuteThrottledAsync(async () =>
            {
                if (!HasApiKey)
                {
                    return null;
                }

                var request = BuildRequest($"/find/{tvdbId}?external_source=tvdb_id");
                var response = await Task.Run(() => _httpClient.Get<TmdbFindResponse>(request));

                if (response.HasHttpError)
                {
                    _logger.Warn("TMDB find error for {TvdbId}: {Status} - {Content}", tvdbId, response.StatusCode, response.Content);
                    return null;
                }

                var tmdbIdValue = response.Resource?.TvResults?.FirstOrDefault()?.Id;
                if (tmdbIdValue.HasValue)
                {
                    _tmdbIdCache.Set(tvdbId.ToString(), tmdbIdValue, TimeSpan.FromDays(7));
                }

                return tmdbIdValue;
            });
        }

        public async Task<int?> GetTvdbIdFromTmdbId(int tmdbId)
        {
            var cached = _tvdbIdCache.Find(tmdbId.ToString());
            if (cached.HasValue)
            {
                return cached;
            }

            return await ExecuteThrottledAsync(async () =>
            {
                if (!HasApiKey)
                {
                    return null;
                }

                var request = BuildRequest($"/tv/{tmdbId}/external_ids");
                var response = await Task.Run(() => _httpClient.Get<TmdbExternalIdsResponse>(request));

                if (response.HasHttpError)
                {
                    _logger.Warn("TMDB external_ids error for {TmdbId}: {Status}", tmdbId, response.StatusCode);
                    return null;
                }

                var tvdbId = response.Resource?.TvdbId;
                if (tvdbId.HasValue)
                {
                    _tvdbIdCache.Set(tmdbId.ToString(), tvdbId, TimeSpan.FromDays(7));
                }

                return tvdbId;
            });
        }

        private async Task<T> ExecuteThrottledAsync<T>(Func<Task<T>> action)
        {
            await _throttle.WaitAsync();
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "TMDB API call failed");
                return default;
            }
            finally
            {
                _throttle.Release();
            }
        }
    }
}
