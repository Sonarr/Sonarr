using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.Tvdb.Resource;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource.Tvdb
{
    public interface ITvdbApiClient
    {
        List<TvdbEpisodeResource> GetEpisodesByOrdering(int tvdbSeriesId, EpisodeOrderType orderType, string language = "eng");
        List<string> GetAvailableOrderings(int tvdbSeriesId);
    }

    public class TvdbApiClient : ITvdbApiClient
    {
        private const string BaseUrl = "https://api4.thetvdb.com/v4";

        private readonly IHttpClient _httpClient;
        private readonly IConfigService _configService;
        private readonly Logger _logger;
        private readonly ICached<string> _tokenCache;
        private readonly ICached<List<string>> _orderingsCache;
        private readonly ICached<List<TvdbEpisodeResource>> _episodeCache;

        public TvdbApiClient(IHttpClient httpClient,
                             IConfigService configService,
                             ICacheManager cacheManager,
                             Logger logger)
        {
            _httpClient = httpClient;
            _configService = configService;
            _logger = logger;
            _tokenCache = cacheManager.GetRollingCache<string>(GetType(), "tvdbToken", TimeSpan.FromHours(20));
            _orderingsCache = cacheManager.GetRollingCache<List<string>>(GetType(), "tvdbOrderings", TimeSpan.FromHours(24));
            _episodeCache = cacheManager.GetRollingCache<List<TvdbEpisodeResource>>(GetType(), "tvdbEpisodes", TimeSpan.FromHours(24));
        }

        public List<string> GetAvailableOrderings(int tvdbSeriesId)
        {
            var cacheKey = tvdbSeriesId.ToString();

            return _orderingsCache.Get(cacheKey, () => FetchAvailableOrderings(tvdbSeriesId));
        }

        public List<TvdbEpisodeResource> GetEpisodesByOrdering(int tvdbSeriesId, EpisodeOrderType orderType, string language = "eng")
        {
            var seasonType = MapOrderTypeToSeasonType(orderType);
            var cacheKey = string.Format("{0}_{1}_{2}", tvdbSeriesId, seasonType, language);

            return _episodeCache.Get(cacheKey, () => FetchAllEpisodesByOrdering(tvdbSeriesId, seasonType, language));
        }

        private List<string> FetchAvailableOrderings(int tvdbSeriesId)
        {
            _logger.Debug("Fetching available orderings from TVDB for series {0}", tvdbSeriesId);

            var request = BuildRequest(string.Format("series/{0}/extended", tvdbSeriesId));
            var response = _httpClient.Get<TvdbResponseResource<TvdbSeriesExtendedResource>>(request);

            if (response.HasHttpError)
            {
                _logger.Warn("Failed to fetch extended series info from TVDB for series {0}: HTTP {1}", tvdbSeriesId, response.StatusCode);
                return new List<string>();
            }

            var data = response.Resource?.Data;
            if (data?.Seasons == null)
            {
                return new List<string>();
            }

            var seasonTypes = data.Seasons
                .Where(s => s.Type != null)
                .Select(s => s.Type.Type)
                .Distinct()
                .ToList();

            _logger.Debug("TVDB series {0} has orderings: {1}", tvdbSeriesId, string.Join(", ", seasonTypes));

            return seasonTypes;
        }

        private List<TvdbEpisodeResource> FetchAllEpisodesByOrdering(int tvdbSeriesId, string seasonType, string language)
        {
            _logger.Debug("Fetching episodes from TVDB for series {0} with ordering {1}", tvdbSeriesId, seasonType);

            var allEpisodes = new List<TvdbEpisodeResource>();
            var page = 0;

            while (true)
            {
                var url = string.Format("series/{0}/episodes/{1}/{2}", tvdbSeriesId, seasonType, language);
                var request = BuildRequest(url);

                if (page > 0)
                {
                    request.Url = request.Url.AddQueryParam("page", page.ToString());
                }

                var response = _httpClient.Get<TvdbResponseResource<TvdbEpisodePageResource>>(request);

                if (response.HasHttpError)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.Warn("TVDB series {0} does not have ordering type {1}", tvdbSeriesId, seasonType);
                        return new List<TvdbEpisodeResource>();
                    }

                    _logger.Warn("Failed to fetch episodes from TVDB for series {0}: HTTP {1}", tvdbSeriesId, response.StatusCode);
                    return allEpisodes;
                }

                var data = response.Resource?.Data;
                if (data?.Episodes == null || data.Episodes.Count == 0)
                {
                    break;
                }

                allEpisodes.AddRange(data.Episodes);

                // TVDB API v4 returns up to 500 episodes per page.
                // If we got fewer, we've reached the last page.
                if (data.Episodes.Count < 500)
                {
                    break;
                }

                page++;
            }

            _logger.Debug("Fetched {0} episodes from TVDB for series {1} ordering {2}", allEpisodes.Count, tvdbSeriesId, seasonType);

            return allEpisodes;
        }

        private HttpRequest BuildRequest(string resource)
        {
            var token = GetAuthToken();

            var request = new HttpRequest(string.Format("{0}/{1}", BaseUrl, resource), HttpAccept.Json);
            request.Headers.Add("Authorization", string.Format("Bearer {0}", token));
            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            return request;
        }

        private string GetAuthToken()
        {
            return _tokenCache.Get("token", () => FetchAuthToken());
        }

        private string FetchAuthToken()
        {
            var apiKey = _configService.TvdbApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("TVDB API key is not configured. Set it in Settings > General.");
            }

            _logger.Debug("Authenticating with TVDB API v4");

            var loginBody = new TvdbLoginResource { ApiKey = apiKey };

            var request = new HttpRequest(string.Format("{0}/login", BaseUrl), HttpAccept.Json);
            request.Method = HttpMethod.Post;
            request.Headers.ContentType = "application/json";
            request.SetContent(loginBody.ToJson());
            request.SuppressHttpError = true;

            var response = _httpClient.Post<TvdbResponseResource<TvdbTokenResource>>(request);

            if (response.HasHttpError)
            {
                throw new InvalidOperationException(
                    string.Format("Failed to authenticate with TVDB API: HTTP {0}. Check your API key.", response.StatusCode));
            }

            var token = response.Resource?.Data?.Token;

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("TVDB API returned empty token. Check your API key.");
            }

            _logger.Debug("Successfully authenticated with TVDB API v4");

            return token;
        }

        public static string MapOrderTypeToSeasonType(EpisodeOrderType orderType)
        {
            switch (orderType)
            {
                case EpisodeOrderType.Default:
                    return "official";
                case EpisodeOrderType.Dvd:
                    return "dvd";
                case EpisodeOrderType.Absolute:
                    return "absolute";
                case EpisodeOrderType.Alternate:
                    return "alternate";
                case EpisodeOrderType.AltDvd:
                    return "altdvd";
                case EpisodeOrderType.Regional:
                    return "regional";
                default:
                    return "official";
            }
        }

        public static EpisodeOrderType MapSeasonTypeToOrderType(string seasonType)
        {
            switch (seasonType)
            {
                case "official":
                    return EpisodeOrderType.Default;
                case "dvd":
                    return EpisodeOrderType.Dvd;
                case "absolute":
                    return EpisodeOrderType.Absolute;
                case "alternate":
                    return EpisodeOrderType.Alternate;
                case "altdvd":
                    return EpisodeOrderType.AltDvd;
                case "regional":
                    return EpisodeOrderType.Regional;
                default:
                    return EpisodeOrderType.Default;
            }
        }
    }
}
