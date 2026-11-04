using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public class MDBListImport : HttpImportListBase<MDBListSettings>
    {
        public const string ClientId = "vqTzrsvdhYmunjzXeynn2zu2VtA4z0RdUl1OQ9c6";
        public const string VerificationUrl = "https://mdblist.com/oauth/device/";

        private const string DeviceCodeGrantType = "urn:ietf:params:oauth:grant-type:device_code";

        private readonly IImportListRepository _importListRepository;
        private readonly ICached<MDBListTokenResponse> _deviceTokenCache;

        public override string Name => _localizationService.GetLocalizedString("ImportListsMDBListSettingsName");
        public override ImportListType ListType => ImportListType.Other;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public MDBListImport(IImportListRepository importListRepository,
                             ICacheManager cacheManager,
                             IHttpClient httpClient,
                             IImportListStatusService importListStatusService,
                             IConfigService configService,
                             IParsingService parsingService,
                             ILocalizationService localizationService,
                             Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
        {
            _importListRepository = importListRepository;
            _deviceTokenCache = cacheManager.GetCache<MDBListTokenResponse>(GetType(), "deviceTokens");
        }

        public override ImportListFetchResult Fetch()
        {
            Settings.Validate().Filter("AccessToken").ThrowOnError();
            _logger.Trace($"Access token expires at {Settings.Expires}");

            if (Settings.RefreshToken.IsNotNullOrWhiteSpace() && Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }

            return FetchItems(g => g.GetListItems());
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new MDBListRequestGenerator
            {
                Settings = Settings
            };
        }

        public override IParseImportListResponse GetParser()
        {
            return new MDBListParser();
        }

        protected override ValidationFailure TestConnection()
        {
            var failure = base.TestConnection();

            if (failure is NzbDroneValidationFailure { IsWarning: true })
            {
                return new NzbDroneValidationFailure(string.Empty,
                    "No shows were returned from this MDBList list. Sonarr only imports the shows on a list, so movie lists belong in Radarr.")
                {
                    IsWarning = true
                };
            }

            return failure;
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                return new
                {
                    url = $"{Settings.BaseUrl.TrimEnd('/')}/oauth/device-authorization/",
                    method = "POST",

                    // jQuery appends a charset by default, which MDBList rejects
                    contentType = "application/x-www-form-urlencoded",
                    data = new Dictionary<string, string>
                    {
                        { "client_id", ClientId },
                        { "scope", "read" }
                    }
                };
            }
            else if (action == "continueOAuth")
            {
                var deviceCode = GetRequiredQuery(query, "device_code");

                var oauthUrl = new HttpRequestBuilder(VerificationUrl)
                    .AddQueryParam("user_code", GetRequiredQuery(query, "user_code"))
                    .AddQueryParam("forward", GetRequiredQuery(query, "callbackUrl"))
                    .Build()
                    .Url
                    .ToString();

                return new
                {
                    oauthUrl,
                    poll = true,
                    deviceCode
                };
            }
            else if (action == "pollOAuth")
            {
                var deviceCode = GetRequiredQuery(query, "deviceCode");
                var token = RequestDeviceToken(deviceCode);

                if (token == null || token.Error is "authorization_pending" or "slow_down")
                {
                    return new { success = false };
                }

                _deviceTokenCache.Set(deviceCode, token, TimeSpan.FromMinutes(10));

                return new { success = true };
            }
            else if (action == "getOAuthToken")
            {
                var deviceCode = GetRequiredQuery(query, "deviceCode");
                var token = _deviceTokenCache.Find(deviceCode);

                if (token == null)
                {
                    throw new NzbDroneClientException(HttpStatusCode.BadGateway, "MDBList sign in timed out, please try again");
                }

                _deviceTokenCache.Remove(deviceCode);

                if (token.AccessToken.IsNullOrWhiteSpace())
                {
                    throw new NzbDroneClientException(HttpStatusCode.BadGateway, "MDBList sign in failed: {0}", token.Error);
                }

                return new
                {
                    accessToken = token.AccessToken,
                    expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn),
                    refreshToken = token.RefreshToken,
                    authUser = GetUsername(token.AccessToken)
                };
            }

            return new { };
        }

        private static string GetRequiredQuery(IDictionary<string, string> query, string key)
        {
            if (!query.TryGetValue(key, out var value) || value.IsNullOrWhiteSpace())
            {
                throw new NzbDroneClientException(HttpStatusCode.BadRequest, "Missing required query parameter: {0}", key);
            }

            return value;
        }

        private MDBListTokenResponse RequestDeviceToken(string deviceCode)
        {
            var request = BuildTokenRequest()
                .AddFormParameter("grant_type", DeviceCodeGrantType)
                .AddFormParameter("device_code", deviceCode)
                .Build();

            return ExecuteTokenRequest(request);
        }

        private void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            var request = BuildTokenRequest()
                .AddFormParameter("grant_type", "refresh_token")
                .AddFormParameter("refresh_token", Settings.RefreshToken)
                .Build();

            var token = ExecuteTokenRequest(request);

            if (token == null || token.AccessToken.IsNullOrWhiteSpace())
            {
                _logger.Warn("Error refreshing MDBList access token: {0}", token?.Error);
                return;
            }

            Settings.AccessToken = token.AccessToken;
            Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
            Settings.RefreshToken = token.RefreshToken ?? Settings.RefreshToken;

            if (Definition.Id > 0)
            {
                _importListRepository.UpdateSettings((ImportListDefinition)Definition);
            }
        }

        private HttpRequestBuilder BuildTokenRequest()
        {
            return new HttpRequestBuilder($"{Settings.BaseUrl.TrimEnd('/')}/oauth/token/")
                .Post()
                .Accept(HttpAccept.Json)
                .AddFormParameter("client_id", ClientId);
        }

        private MDBListTokenResponse ExecuteTokenRequest(HttpRequest request)
        {
            request.SuppressHttpError = true;

            try
            {
                var response = _httpClient.Execute(request);

                if ((int)response.StatusCode >= 500)
                {
                    _logger.Warn("MDBList token request failed with {0}", response.StatusCode);
                    return null;
                }

                return Json.Deserialize<MDBListTokenResponse>(response.Content);
            }
            catch (Exception ex) when (ex is HttpException or WebException or JsonException)
            {
                _logger.Warn(ex, "MDBList token request failed");
                return null;
            }
        }

        private string GetUsername(string accessToken)
        {
            var request = new HttpRequestBuilder($"{Settings.BaseUrl.TrimEnd('/')}/user")
                .Accept(HttpAccept.Json)
                .Build();

            request.Headers.Add("Authorization", "Bearer " + accessToken);

            try
            {
                return _httpClient.Get<MDBListUserResponse>(request).Resource?.Username;
            }
            catch (HttpException)
            {
                _logger.Warn("Error fetching MDBList username");
            }

            return null;
        }
    }
}
