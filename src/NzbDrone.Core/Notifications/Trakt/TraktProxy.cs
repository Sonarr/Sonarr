using System.Net.Http;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Trakt.Resource;

namespace NzbDrone.Core.Notifications.Trakt
{
    public interface ITraktProxy
    {
        string GetUserName(string accessToken);
        HttpRequest GetOAuthRequest(string callbackUrl);
        TraktAuthRefreshResource RefreshAuthToken(string refreshToken);
        void AddToCollection(TraktCollectShowsResource payload, string accessToken);
        void RemoveFromCollection(TraktCollectShowsResource payload, string accessToken);
        HttpRequest BuildTraktRequest(string resource, HttpMethod method, string accessToken);
    }

    public class TraktProxy : ITraktProxy
    {
        private const string URL = "https://api.trakt.tv";
        private const string OAuthUrl = "https://trakt.tv/oauth/authorize";
        private const string RedirectUri = "https://auth.servarr.com/v1/trakt_sonarr/auth";
        private const string RenewUri = "https://auth.servarr.com/v1/trakt_sonarr/renew";
        private const string ClientId = "d44ba57cab40c31eb3f797dcfccd203500796539125b333883ec1d94aa62ed4c";

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public TraktProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void AddToCollection(TraktCollectShowsResource payload, string accessToken)
        {
            var request = BuildTraktRequest("sync/collection", HttpMethod.Post, accessToken);

            request.Headers.ContentType = "application/json";
            request.SetContent(payload.ToJson());

            try
            {
                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, "Unable to post payload {0}", payload);
                throw new TraktException("Unable to post payload", ex);
            }
        }

        public void RemoveFromCollection(TraktCollectShowsResource payload, string accessToken)
        {
            var request = BuildTraktRequest("sync/collection/remove", HttpMethod.Post, accessToken);

            request.Headers.ContentType = "application/json";
            var temp = payload.ToJson();
            request.SetContent(payload.ToJson());

            try
            {
                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, "Unable to post payload {0}", payload);
                throw new TraktException("Unable to post payload", ex);
            }
        }

        public string GetUserName(string accessToken)
        {
            var request = BuildTraktRequest("users/settings", HttpMethod.Get, accessToken);

            try
            {
                var response = _httpClient.Get<TraktUserSettingsResource>(request);

                if (response != null && response.Resource != null)
                {
                    return response.Resource.User.Ids.Slug;
                }
            }
            catch (HttpException)
            {
                _logger.Warn($"Error refreshing trakt access token");
            }

            return null;
        }

        public HttpRequest GetOAuthRequest(string callbackUrl)
        {
            return new HttpRequestBuilder(OAuthUrl)
                            .AddQueryParam("client_id", ClientId)
                            .AddQueryParam("response_type", "code")
                            .AddQueryParam("redirect_uri", RedirectUri)
                            .AddQueryParam("state", callbackUrl)
                            .Build();
        }

        public TraktAuthRefreshResource RefreshAuthToken(string refreshToken)
        {
            var request = new HttpRequestBuilder(RenewUri)
                    .AddQueryParam("refresh_token", refreshToken)
                    .Build();

            return _httpClient.Get<TraktAuthRefreshResource>(request)?.Resource ?? null;
        }

        public HttpRequest BuildTraktRequest(string resource, HttpMethod method, string accessToken)
        {
            var request = new HttpRequestBuilder(URL).Resource(resource).Build();
            request.Method = method;

            request.Headers.Accept = HttpAccept.Json.Value;
            request.Headers.Add("trakt-api-version", "2");
            request.Headers.Add("trakt-api-key", ClientId);

            if (accessToken.IsNotNullOrWhiteSpace())
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            return request;
        }
    }
}
