using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public abstract class TraktImportBase<TSettings> : HttpImportListBase<TSettings>
    where TSettings : TraktSettingsBase<TSettings>, new()
    {
        public override ImportListType ListType => ImportListType.Trakt;

        public const string OAuthUrl = "https://trakt.tv/oauth/authorize";
        public const string RedirectUri = "https://auth.servarr.com/v1/trakt_sonarr/auth";
        public const string RenewUri = "https://auth.servarr.com/v1/trakt_sonarr/renew";
        public const string ClientId = "d44ba57cab40c31eb3f797dcfccd203500796539125b333883ec1d94aa62ed4c";

        private IImportListRepository _importListRepository;

        protected TraktImportBase(IImportListRepository netImportRepository,
                           IHttpClient httpClient,
                           IImportListStatusService importListStatusService,
                           IConfigService configService,
                           IParsingService parsingService,
                           Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _importListRepository = netImportRepository;
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            Settings.Validate().Filter("AccessToken", "RefreshToken").ThrowOnError();
            _logger.Trace($"Access token expires at {Settings.Expires}");

            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }

            var generator = GetRequestGenerator();
            return FetchItems(g => g.GetListItems(), true);
        }

        public override IParseImportListResponse GetParser()
        {
            return new TraktParser();
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = new HttpRequestBuilder(OAuthUrl)
                    .AddQueryParam("client_id", ClientId)
                    .AddQueryParam("response_type", "code")
                    .AddQueryParam("redirect_uri", RedirectUri)
                    .AddQueryParam("state", query["callbackUrl"])
                    .Build();

                return new
                {
                    OauthUrl = request.Url.ToString()
                };
            }
            else if (action == "getOAuthToken")
            {
                return new
                {
                    accessToken = query["access_token"],
                    expires = DateTime.UtcNow.AddSeconds(int.Parse(query["expires_in"])),
                    refreshToken = query["refresh_token"],
                    authUser = GetUserName(query["access_token"])
                };
            }

            return new { };
        }

        private string GetUserName(string accessToken)
        {
            var request = new HttpRequestBuilder(string.Format("{0}/users/settings", Settings.BaseUrl))
                .Build();

            request.Headers.Add("trakt-api-version", "2");
            request.Headers.Add("trakt-api-key", ClientId);

            if (accessToken.IsNotNullOrWhiteSpace())
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            try
            {
                var response = _httpClient.Get<UserSettingsResponse>(request);

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

        private void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            Settings.Validate().Filter("RefreshToken").ThrowOnError();

            var request = new HttpRequestBuilder(RenewUri)
                .AddQueryParam("refresh_token", Settings.RefreshToken)
                .Build();

            try
            {
                var response = _httpClient.Get<RefreshRequestResponse>(request);

                if (response != null && response.Resource != null)
                {
                    var token = response.Resource;
                    Settings.AccessToken = token.AccessToken;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                    Settings.RefreshToken = token.RefreshToken != null ? token.RefreshToken : Settings.RefreshToken;

                    if (Definition.Id > 0)
                    {
                        _importListRepository.UpdateSettings((ImportListDefinition)Definition);
                    }
                }
            }
            catch (HttpException)
            {
                _logger.Warn($"Error refreshing trakt access token");
            }
        }
    }
}
