using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists.Tmdb;

public abstract class TmdbImportBase<TSettings>(ISonarrCloudRequestBuilder requestBuilder,
                                                IHttpClient httpClient,
                                                IImportListStatusService importListStatusService,
                                                IConfigService configService,
                                                IParsingService parsingService,
                                                ILocalizationService localizationService,
                                                Logger logger)
    : HttpImportListBase<TSettings>(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
    where TSettings : TmdbSettingsBase<TSettings>, new()
{
    private const string SonarrAuthAccess = "auth/tmdb/access";
    private const string SonarrAuthRequest = "auth/tmdb/request";
    private const string TmdbAuthUserApproval = "https://www.themoviedb.org/auth/access";

    private readonly IHttpRequestBuilderFactory _requestBuilder = requestBuilder.Services;

    public override ImportListType ListType => ImportListType.Tmdb;
    public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);
    public override IEnumerable<ProviderDefinition> DefaultDefinitions => GetPresetDefinitions();

    public override object RequestAction(string action, IDictionary<string, string> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return action switch
        {
            "startOAuth" => StartOAuth(query),
            "getOAuthToken" => GetOAuthToken(query),
            _ => new { }
        };
    }

    protected override bool IsValidItem(ImportListItemInfo listItem)
    {
        return listItem != null && base.IsValidItem(listItem);
    }

    protected virtual IEnumerable<KeyValuePair<string, TSettings>> GetPresetDefinitionPairs() => [];

    private static string GetRequiredQuery(IDictionary<string, string> query, string key)
    {
        if (!query.TryGetValue(key, out var value) || value.IsNullOrWhiteSpace())
        {
            throw new NzbDroneClientException(HttpStatusCode.BadRequest, "Missing required query parameter: {0}", key);
        }

        return value;
    }

    private object StartOAuth(IDictionary<string, string> query)
    {
        var request = _requestBuilder.Create()
            .Accept(HttpAccept.Json)
            .Resource(SonarrAuthRequest)
            .SetHeader("Content-Type", "application/json")
            .AddQueryParam("redirectUrl", GetRequiredQuery(query, "callbackUrl"))
            .Build();

        var response = _httpClient.Execute(request);
        var resource = STJson.Deserialize<RequestTokenResource>(response.Content);

        if (!resource.IsSuccess || resource.RequestToken.IsNullOrWhiteSpace())
        {
            throw new NzbDroneClientException(HttpStatusCode.BadGateway, "TMDb request token failed: {0}", resource.StatusMessage);
        }

        request = new HttpRequestBuilder(TmdbAuthUserApproval)
            .AddQueryParam("request_token", resource.RequestToken)
            .Build();

        return new
        {
            OauthUrl = request.Url.ToString(),
            resource.RequestToken
        };
    }

    private object GetOAuthToken(IDictionary<string, string> query)
    {
        var request = _requestBuilder.Create()
            .Accept(HttpAccept.Json)
            .Resource(SonarrAuthAccess)
            .SetHeader("Content-Type", "application/json")
            .AddQueryParam("requestToken", GetRequiredQuery(query, "requestToken"))
            .Build();

        var response = _httpClient.Execute(request);
        var resource = STJson.Deserialize<AccessTokenResource>(response.Content);

        if (!resource.IsSuccess)
        {
            throw new NzbDroneClientException(HttpStatusCode.BadGateway, "TMDb access token failed: {0}", resource.StatusMessage);
        }

        return new
        {
            accountId = resource.AccountId,
            authToken = resource.AccessToken
        };
    }

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
