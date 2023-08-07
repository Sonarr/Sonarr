using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MalImport : HttpImportListBase<MalListSettings>
    {
        public const string OAuthUrl = "https://myanimelist.net/v1/oauth2/authorize";
        public const string RedirectUri = "https://auth.servarr.com/v1/mal_sonarr/auth";
        public const string RenewUri = "https://auth.servarr.com/v1/mal_sonarr/renew";
        public const string ClientId = "402de1a90f8eb545f625739fa69ddb98";

        public static Dictionary<int, int> MalTvdbIds = new Dictionary<int, int>();

        public override string Name => "MyAnimeList";
        public override ImportListType ListType => ImportListType.Other;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        private static string _Codechallenge = "";
        private readonly IImportListRepository _importListRepository;

        // This constructor the first thing that is called when sonarr creates a button
        public MalImport(IImportListRepository netImportRepository, IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            if (MalTvdbIds.Count == 0)
            {
                MalTvdbIds = GetMalToTvdbIds();
            }

            _importListRepository = netImportRepository;
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }

            return FetchItems(g => g.GetListItems());
        }

        // This method is used for generating the access token.
        // In the MAL API instructions (https://myanimelist.net/blog.php?eid=835707)
        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                _Codechallenge = GenCodeChallenge();
                var request = new HttpRequestBuilder(OAuthUrl)
                    .AddQueryParam("response_type", "code")
                    .AddQueryParam("client_id", ClientId)
                    .AddQueryParam("code_challenge", _Codechallenge)
                    .AddQueryParam("state", query["callbackUrl"])
                    .AddQueryParam("redirect_uri", RedirectUri)
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
                    refreshToken = query["refresh_token"]
                };
            }

            return new { };
        }

        public override IParseImportListResponse GetParser()
        {
            return new MalParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new MalRequestGenerator()
            {
                Settings = Settings,
            };
        }

        private string GenCodeChallenge()
        {
            // For the sake of MAL, the code challenge is the same as the code verifier
            var validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-~._";

            var code = new char[128];

            for (var i = 0; i < code.Length; i++)
            {
                var selectedchar = validChars[RandomNumberGenerator.GetInt32(validChars.Length)];
                code[i] = selectedchar;
            }

            var codeChallenge = new string(code);

            return codeChallenge;
        }

        private Dictionary<int, int> GetMalToTvdbIds()
        {
            try
            {
                var request = new HttpRequestBuilder("https://raw.githubusercontent.com/Fribb/anime-lists/master/anime-list-mini.json")
                    .Build();
                var response = _httpClient.Get(request);
                var ids = Json.Deserialize<List<IDS>>(response.Content);
                var resultDict = new Dictionary<int, int>();

                foreach (var id in ids)
                {
                    resultDict.TryAdd(id.MalId, id.TvdbId);
                }

                return resultDict;
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex.Message);
                return null;
            }
        }

        private void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            Settings.Validate().Filter("RefreshToken").ThrowOnError();

            var httpReq = new HttpRequestBuilder(RenewUri)
                .AddQueryParam("refresh_token", Settings.RefreshToken)
                .Build();
            try
            {
                var httpResp = _httpClient.Get<MalAuthToken>(httpReq);

                if (httpResp?.Resource != null)
                {
                    var token = httpResp.Resource;
                    Settings.AccessToken = token.AccessToken;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                    Settings.RefreshToken = token.RefreshToken ?? Settings.RefreshToken;

                    if (Definition.Id > 0)
                    {
                        _importListRepository.UpdateSettings((ImportListDefinition)Definition);
                    }
                }
            }
            catch (HttpRequestException)
            {
                _logger.Error("Error trying to refresh MAL access token.");
            }
        }
    }
}
