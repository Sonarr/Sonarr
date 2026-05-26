using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists.TMDb;

public abstract class TMDbImportBase<TSettings> : HttpImportListBase<TSettings>
    where TSettings : TMDbSettingsBase<TSettings>, new()
{
    private const string SonarrAuthAccess = "auth/tmdb/access";
    private const string SonarrAuthRequest = "auth/tmdb/request";
    private const string TMDbAuthAccess = "4/auth/access_token";
    private const string TMDbAuthRequest = "4/auth/request_token";
    private const string TMDbAuthUserApproval = "https://www.themoviedb.org/auth/access";

    private readonly IHttpRequestBuilderFactory _requestBuilder;

    protected TMDbImportBase(ISonarrCloudRequestBuilder requestBuilder,
                             IHttpClient httpClient,
                             IImportListStatusService importListStatusService,
                             IConfigService configService,
                             IParsingService parsingService,
                             ILocalizationService localizationService,
                             Logger logger)
        : base(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
    {
        _requestBuilder = requestBuilder.Services;
    }

    public override ImportListType ListType => ImportListType.TMDb;
    public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);
    public override IEnumerable<ProviderDefinition> DefaultDefinitions => GetPresetDefinitions();

    public override object RequestAction(string action, IDictionary<string, string> query)
    {
        if (action == "startOAuth")
        {
            if (TMDbToken.TryParse(Settings.ApiKey, out var apiAccessToken) && !apiAccessToken.CanRead)
            {
                _logger.Warn("Access token does not contain valid read permissions and will be ignored.");
            }

            HttpRequest request;
            if (apiAccessToken.Raw.IsNotNullOrWhiteSpace())
            {
                request = CreateOAuthRequestBuilder(TMDbAuthRequest, apiAccessToken.Raw).Build();
                request.SetContent(JsonSerializer.Serialize(new { redirect_to = query["callbackUrl"] }));
            }
            else
            {
                request = CreateOAuthRequestBuilder(SonarrAuthRequest)
                    .AddQueryParam("redirectUrl", query["callbackUrl"])
                    .Build();
            }

            var response = _httpClient.Execute(request);
            var resource = JsonSerializer.Deserialize<RequestTokenResponse>(response.Content);

            _ = TMDbToken.TryParse(resource.RequestToken, out var requestToken);
            if (requestToken.RedirectTo.IsNullOrWhiteSpace())
            {
                _logger.Warn("Request token does not contain an embedded 'redirect_to' payload object.");
            }

            request = new HttpRequestBuilder(TMDbAuthUserApproval)
                .AddQueryParam("request_token", requestToken.Raw)
                .Build();

            return new
            {
                OauthUrl = request.Url.ToString(),
                request_token = requestToken.Raw,
                api_access_token = apiAccessToken.Raw ?? string.Empty
            };
        }
        else if (action == "getOAuthToken")
        {
            HttpRequest request;
            var apiAccessToken = query["api_access_token"];
            if (apiAccessToken.IsNotNullOrWhiteSpace())
            {
                request = CreateOAuthRequestBuilder(TMDbAuthAccess, apiAccessToken).Build();
                request.SetContent(JsonSerializer.Serialize(new { request_token = query["request_token"] }));
            }
            else
            {
                request = CreateOAuthRequestBuilder(SonarrAuthAccess)
                    .AddQueryParam("requestToken", query["request_token"])
                    .Build();
            }

            var response = _httpClient.Execute(request);
            return JsonSerializer.Deserialize<AccessTokenResponse>(response.Content);
        }

        return new { };
    }

    protected override bool IsValidItem(ImportListItemInfo listItem)
    {
        return listItem != null && base.IsValidItem(listItem);
    }

    protected virtual IEnumerable<KeyValuePair<string, TSettings>> GetPresetDefinitionPairs() => [];

    private IEnumerable<ProviderDefinition> GetPresetDefinitions()
    {
        return GetPresetDefinitionPairs().Select(definition => new ImportListDefinition
        {
            Name = definition.Key,
            Settings = definition.Value,
            Implementation = GetType().Name
        });
    }

    private HttpRequestBuilder CreateOAuthRequestBuilder(string resourceUrl, string apiAccessToken = null)
    {
        HttpRequestBuilder builder;
        if (apiAccessToken.IsNotNullOrWhiteSpace())
        {
            builder = new HttpRequestBuilder(Settings.BaseUrl) { Method = HttpMethod.Post }
                .SetHeader("Authorization", $"Bearer {apiAccessToken}");
        }
        else
        {
            builder = _requestBuilder.Create();
        }

        builder.Accept(HttpAccept.Json)
            .Resource(resourceUrl)
            .SetHeader("Content-Type", "application/json");

        return builder;
    }
}
