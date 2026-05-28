using System;
using System.Collections.Generic;
using System.Linq;
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

namespace NzbDrone.Core.ImportLists.Tmdb;

public abstract class TmdbImportBase<TSettings> : HttpImportListBase<TSettings>
    where TSettings : TmdbSettingsBase<TSettings>, new()
{
    private const string SonarrAuthAccess = "auth/tmdb/access";
    private const string SonarrAuthRequest = "auth/tmdb/request";
    private const string TmdbAuthUserApproval = "https://www.themoviedb.org/auth/access";

    private readonly IHttpRequestBuilderFactory _requestBuilder;

    protected TmdbImportBase(ISonarrCloudRequestBuilder requestBuilder,
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

    public override ImportListType ListType => ImportListType.Tmdb;
    public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);
    public override IEnumerable<ProviderDefinition> DefaultDefinitions => GetPresetDefinitions();

    public override object RequestAction(string action, IDictionary<string, string> query)
    {
        if (action == "startOAuth")
        {
            var request = _requestBuilder.Create()
                .Accept(HttpAccept.Json)
                .Resource(SonarrAuthRequest)
                .SetHeader("Content-Type", "application/json")
                .AddQueryParam("redirectUrl", query["callbackUrl"])
                .Build();

            var response = _httpClient.Execute(request);
            var resource = JsonSerializer.Deserialize<RequestTokenResource>(response.Content);

            if (!TmdbToken.TryParse(resource.RequestToken, out var requestToken) || requestToken.RedirectTo.IsNullOrWhiteSpace())
            {
                _logger.Warn("Request token does not contain an embedded 'redirect_to' payload object.");
            }

            request = new HttpRequestBuilder(TmdbAuthUserApproval)
                .AddQueryParam("request_token", requestToken.Raw)
                .Build();

            return new
            {
                OauthUrl = request.Url.ToString(),
                RequestToken = requestToken.Raw
            };
        }
        else if (action == "getOAuthToken")
        {
            var request = _requestBuilder.Create()
                .Accept(HttpAccept.Json)
                .Resource(SonarrAuthAccess)
                .SetHeader("Content-Type", "application/json")
                .AddQueryParam("requestToken", query["requestToken"])
                .Build();

            var response = _httpClient.Execute(request);
            var resource = JsonSerializer.Deserialize<AccessTokenResource>(response.Content);

            if (!TmdbToken.TryParse(resource.AccessToken, out var accessToken) || !accessToken.CanRead)
            {
                _logger.Warn("Access token does not provide the application with the required read permissions.");
            }

            return resource;
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
}
