using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.AniList
{
    public abstract class AniListImportBase<TSettings> : HttpImportListBase<TSettings>
    where TSettings : AniListSettingsBase<TSettings>, new()
    {
        public override ImportListType ListType => ImportListType.Other;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);

        public const string OAuthUrl = "https://anilist.co/api/v2/oauth/authorize";
        public const string RedirectUri = "https://auth.servarr.com/v1/anilist_sonarr/auth";
        public const string RenewUri = "https://auth.servarr.com/v1/anilist_sonarr/renew";

        public const string ClientId = "13780";

        protected IImportListRepository _importListRepository;

        protected AniListImportBase(IImportListRepository netImportRepository,
                            IHttpClient httpClient,
                            IImportListStatusService importListStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _importListRepository = netImportRepository;
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
                };
            }

            return new { };
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            CheckToken();
            return base.Fetch();
        }

        protected void CheckToken()
        {
            Settings.Validate().Filter("AccessToken", "RefreshToken").ThrowOnError();
            _logger.Trace("Access token expires at {0}", Settings.Expires);

            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
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
                        Settings.RefreshToken = token.RefreshToken ?? Settings.RefreshToken;

                        if (Definition.Id > 0)
                        {
                            _importListRepository.UpdateSettings((ImportListDefinition)Definition);
                        }
                    }
                }
                catch (HttpException)
                {
                    _logger.Warn($"Error refreshing access token");
                }
            }
        }
    }
}
