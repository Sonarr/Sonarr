using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Simkl
{
    public abstract class SimklImportBase<TSettings> : HttpImportListBase<TSettings>
    where TSettings : SimklSettingsBase<TSettings>, new()
    {
        public override ImportListType ListType => ImportListType.Simkl;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public const string OAuthUrl = "https://simkl.com/oauth/authorize";
        public const string RedirectUri = "https://auth.servarr.com/v1/simkl_sonarr/auth";
        public const string RenewUri = "https://auth.servarr.com/v1/simkl_sonarr/renew";
        public const string ClientId = "3281c139f576b2f59c1389b22337140b6b087ee17e000e89dbafdcf20af6dac7";

        private IImportListRepository _importListRepository;

        protected SimklImportBase(IImportListRepository netImportRepository,
                           IHttpClient httpClient,
                           IImportListStatusService importListStatusService,
                           IConfigService configService,
                           IParsingService parsingService,
                           ILocalizationService localizationService,
                           Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
        {
            _importListRepository = netImportRepository;
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            Settings.Validate().Filter("AccessToken", "RefreshToken").ThrowOnError();
            _logger.Trace($"Access token expires at {Settings.Expires}");

            // Simkl doesn't currently expire access tokens, but if they start lets be prepared
            if (Settings.RefreshToken.IsNotNullOrWhiteSpace() && Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }

            var lastFetch = _importListStatusService.GetLastSyncListInfo(Definition.Id);
            var lastActivity = GetLastActivity();

            // Check to see if user has any activity since last sync, if not return empty to avoid work
            if (lastFetch.HasValue && lastActivity < lastFetch.Value.AddHours(-2))
            {
                return Array.Empty<ImportListItemInfo>();
            }

            var generator = GetRequestGenerator();

            return FetchItems(g => g.GetListItems(), true);
        }

        public override IParseImportListResponse GetParser()
        {
            return new SimklParser();
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
                    authUser = GetUserId(query["access_token"])
                };
            }

            return new { };
        }

        private DateTime GetLastActivity()
        {
            var request = new HttpRequestBuilder(string.Format("{0}/sync/activities", Settings.BaseUrl)).Build();

            request.Headers.Add("simkl-api-key", ClientId);
            request.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);

            try
            {
                var response = _httpClient.Get<SimklSyncActivityResource>(request);

                if (response?.Resource != null)
                {
                    return response.Resource.TvShows.All;
                }
            }
            catch (HttpException)
            {
                _logger.Warn($"Error fetching user activity");
            }

            return DateTime.UtcNow;
        }

        private string GetUserId(string accessToken)
        {
            var request = new HttpRequestBuilder(string.Format("{0}/users/settings", Settings.BaseUrl))
                .Build();

            request.Headers.Add("simkl-api-key", ClientId);

            if (accessToken.IsNotNullOrWhiteSpace())
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            try
            {
                var response = _httpClient.Get<UserSettingsResponse>(request);

                if (response?.Resource != null)
                {
                    return response.Resource.Account.Id.ToString();
                }
            }
            catch (HttpException)
            {
                _logger.Warn($"Error refreshing simkl access token");
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

                if (response?.Resource != null)
                {
                    var token = response.Resource;
                    Settings.AccessToken = token.AccessToken;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                    Settings.RefreshToken = token.RefreshToken ?? Settings.RefreshToken;

                    if (Definition.Id > 0)
                    {
                        _importListRepository.UpdateSettings((ImportListDefinition)Definition);
                    }
                }
            }
            catch (HttpException)
            {
                _logger.Warn($"Error refreshing simkl access token");
            }
        }
    }
}
