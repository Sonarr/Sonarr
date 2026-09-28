using System;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Authentication
{
    public interface IOidcDiscoveryService
    {
        bool IsDiscoverable(string authority);
    }

    public class OidcDiscoveryService : IOidcDiscoveryService
    {
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public OidcDiscoveryService(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public bool IsDiscoverable(string authority)
        {
            if (authority.IsNullOrWhiteSpace())
            {
                return false;
            }

            try
            {
                var request = new HttpRequestBuilder(authority)
                    .Resource(".well-known/openid-configuration")
                    .Build();

                request.RequestTimeout = RequestTimeout;
                request.SuppressHttpError = true;

                var response = _httpClient.Get<OidcDiscoveryDocument>(request);

                if (response.HasHttpError)
                {
                    _logger.Debug("OIDC configuration request for {0} failed with status {1}", request.Url, response.StatusCode);

                    return false;
                }

                var document = response.Resource;

                if (document == null)
                {
                    _logger.Debug("OIDC configuration request for {0} returned an invalid response", request.Url);

                    return false;
                }

                if (document.AuthorizationEndpoint.IsNullOrWhiteSpace() || document.TokenEndpoint.IsNullOrWhiteSpace())
                {
                    _logger.Debug("OIDC configuration from {0} is missing the authorization or token endpoint", request.Url);

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Unable to retrieve the OIDC configuration from {0}", authority);

                return false;
            }
        }
    }

    public class OidcDiscoveryDocument
    {
        [JsonProperty("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }

        [JsonProperty("token_endpoint")]
        public string TokenEndpoint { get; set; }
    }
}
