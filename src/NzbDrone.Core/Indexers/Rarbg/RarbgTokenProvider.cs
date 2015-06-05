using System;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Indexers.Rarbg
{
    public interface IRarbgTokenProvider
    {
        string GetToken(RarbgSettings settings);
    }

    public class RarbgTokenProvider : IRarbgTokenProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly ICached<string> _tokenCache;
        private readonly Logger _logger;

        public RarbgTokenProvider(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
        {
            _httpClient = httpClient;
            _tokenCache = cacheManager.GetCache<string>(GetType());
            _logger = logger;
        }

        public string GetToken(RarbgSettings settings)
        {
            return _tokenCache.Get(settings.BaseUrl, () =>
                {
                    var url = settings.BaseUrl.Trim('/') + "/pubapi_v2.php?get_token=get_token";

                    var response = _httpClient.Get<JObject>(new HttpRequest(url, HttpAccept.Json));

                    return response.Resource["token"].ToString();
                }, TimeSpan.FromMinutes(14.0));
        }
    }
}
