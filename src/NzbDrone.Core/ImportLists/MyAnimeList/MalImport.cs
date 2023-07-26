using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MalImport : HttpImportListBase<MalListSettings>
    {
        public const string OAuthUrl = "https://myanimelist.net/v1/oauth2/authorize";
        public const string OAuthTokenUrl = "https://myanimelist.net/v1/oauth2/token";
        public const string RedirectUri = "http://localhost:8989/oauth.html";

        public static Dictionary<int, int> MalTvdbIds = new Dictionary<int, int>();

        private static string _Codechallenge = "";

        public override string Name => "MyAnimeList";
        public override ImportListType ListType => ImportListType.Other;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromSeconds(10);  // change this later

        // This constructor the first thing that is called when sonarr creates a button
        public MalImport(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            if (MalTvdbIds.Count == 0)
            {
                MalTvdbIds = GetMalToTvdbIds();
            }
        }

        // This method should refresh (dunno what that means) the token
        // This method also fetches the anime from mal?
        public override IList<ImportListItemInfo> Fetch()
        {
            //_importListStatusService;
            return FetchItems(g => g.GetListItems());
        }

        // This method is used for generating the access token.
        // In the MAL API instructions (https://myanimelist.net/blog.php?eid=835707)
        // How can I call this function 3 times so I don't have to use helper functions?
        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                // The workaround
                // Have mal redirect, then make copy and paste the returned stuff into a text box for sonarr to use.
                // Create those boxes in MalListSettings
                _Codechallenge = GenCodeChallenge();
                var request = new HttpRequestBuilder(OAuthUrl)
                    .AddQueryParam("response_type", "code")
                    .AddQueryParam("client_id", Settings.ClientId)
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
                var jsonResult = Json.Deserialize<MalAuthToken>(GetAuthToken(query["code"]));
                return new
                {
                    accessToken = jsonResult.AccessToken,
                    refreshToken = jsonResult.RefreshToken,
                    expires = DateTime.UtcNow.AddSeconds(int.Parse(jsonResult.ExpiresIn))
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

        private class IDS
        {
            [JsonProperty("mal_id")]
            public int MalId { get; set; }

            [JsonProperty("thetvdb_id")]
            public int TvdbId { get; set; }
        }

        private class MalAuthToken
        {
            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public string ExpiresIn { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }
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

        private string GetAuthToken(string authcode)
        {
            try
            {
                var req = new HttpRequestBuilder(OAuthTokenUrl).Post()
                    .AddFormParameter("client_id", Settings.ClientId)
                    .AddFormParameter("client_secret", Settings.ClientSecret)
                    .AddFormParameter("code", authcode)
                    .AddFormParameter("code_verifier", _Codechallenge)
                    .AddFormParameter("grant_type", "authorization_code")
                    .AddFormParameter("redirect_uri", "http://localhost:8989/oauth.html")
                    .Build();
                var response = _httpClient.Post(req);
                return response.Content;
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex.Message);
                return null;
            }
        }
    }
}
