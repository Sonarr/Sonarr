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
using HttpClient = System.Net.Http.HttpClient;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MalImport : HttpImportListBase<MalListSettings>
    {
        public const string OAuthUrl = "https://myanimelist.net/v1/oauth2/authorize";
        public const string OAuthTokenUrl = "https://myanimelist.net/v1/oauth2/token";
        public const string ClientId = "4d057302cee543511828cac37864235a";
        public const string ClientSecret = "756e293edefff82edf6245fb02e23aacfa58c602e4c49f51acf5aa9161685f6f";
        public const string RedirectUri = "http://localhost:8989/oauth.html";

        public static string MalIdConversions = "";
        public static Dictionary<int, int> Maltotvdb = new Dictionary<int, int>();

        private string _Codechallenge = "";

        public override string Name => "MyAnimeList";
        public override ImportListType ListType => ImportListType.Other;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromSeconds(10);  // change this later

        // This constructor the first thing that is called when sonarr creates a button
        public MalImport(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _Codechallenge = GeneratePKCEforMal();
            LoadMalIDConversionJson();
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
        // we need to create an app entry for this.
        // This doesn't work. 2 requests need to be made, one to get the auth code, then the second to get the json response
        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                // The workaround
                // Have mal redirect, then make copy and paste the returned stuff into a text box for sonarr to use.
                // Create those boxes in MalListSettings
                var request = new HttpRequestBuilder(OAuthUrl)
                    .AddQueryParam("response_type", "code")
                    .AddQueryParam("client_id", ClientId)
                    .AddQueryParam("code_challenge", _Codechallenge)
                    .AddQueryParam("state", query["callbackUrl"])
                    .AddQueryParam("redirect_uri", "http://localhost:8989/api/v3/importlist/action/ligma")
                    .Build();

                return new
                {
                    OauthUrl = request.Url.ToString()
                };
            }
            else if (action == "getOAuthToken")
            {
                var request = new HttpRequestBuilder(OAuthTokenUrl)
                    .AddQueryParam("response_type", "code")
                    .AddQueryParam("client_id", ClientId)
                    .AddQueryParam("client_secret", ClientSecret)
                    .AddQueryParam("code", query["code"])
                    .AddQueryParam("code_challenge", _Codechallenge)
                    .AddQueryParam("grant_type", "authorization_code")
                    //.AddQueryParam("redirect_uri", "http://localhost:8989/api/v3/importlist/action/ligma")
                    .Build();
                request.Method = HttpMethod.Post;

                return new { };
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

        private string GeneratePKCEforMal()
        {
            // For the sake of MAL, the code challenge is the same as the code verifie
            var validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-~._";

            var code = new char[128];

            for (var i = 0; i < code.Length; i++)
            {
                var selectedchar = validChars[RandomNumberGenerator.GetInt32(validChars.Length)];
                code[i] = selectedchar;
            }

            var codeChallenge = new string(code);
            _logger.Info($"MAL Code Challenge: {codeChallenge}");

            return codeChallenge;
        }

        private class IDS
        {
            [JsonProperty("mal_id")]
            public int MalId { get; set; }

            [JsonProperty("thetvdb_id")]
            public int TvdbId { get; set; }
        }

        private void generateDictionary(string jsonfile)
        {
            var ids = Json.Deserialize<List<IDS>>(jsonfile);

            foreach (var id in ids)
            {
                Maltotvdb.Add(id.MalId, id.TvdbId);
            }
        }

        private async void LoadMalIDConversionJson()
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("https://github.com/Fribb/anime-lists/blob/master/anime-list-mini.json");
                var result = await response.Content.ReadAsStringAsync();
                generateDictionary(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex.Message);
                MalIdConversions = "";
            }
        }
    }
}
