using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb.List;

public sealed class TMDbListImport : TMDbImportBase<TMDbListSettings>
{
    public TMDbListImport(ISonarrCloudRequestBuilder requestBuilder,
                          IHttpClient httpClient,
                          IImportListStatusService importListStatusService,
                          IConfigService configService,
                          IParsingService parsingService,
                          ILocalizationService localizationService,
                          Logger logger)
        : base(requestBuilder, httpClient, importListStatusService, configService, parsingService, localizationService, logger)
    {
    }

    public override int PageSize => 20;
    public override string Name => "TMDb List";

    public override object RequestAction(string action, IDictionary<string, string> query)
    {
        if (action == "getAccountLists")
        {
            return new { options = GetAccountListOptions() };
        }

        return base.RequestAction(action, query);
    }

    public override IParseImportListResponse GetParser()
    {
        return new TMDbListParser();
    }

    public override IImportListRequestGenerator GetRequestGenerator()
    {
        return new TMDbListRequestGenerator(Settings);
    }

    private List<FieldSelectStringOption> GetAccountListOptions()
    {
        const int maxPages = 3;
        var options = new List<FieldSelectStringOption>((PageSize * maxPages) + 1)
        {
            new() { Name = "None" }
        };

        if (Settings.AuthToken.IsNullOrWhiteSpace() || Settings.AccountId.IsNullOrWhiteSpace())
        {
            return options;
        }

        var builder = new HttpRequestBuilder(Settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .Resource($"4/account/{Settings.AccountId}/lists")
            .SetHeader("Authorization", $"Bearer {Settings.AuthToken}")
            .AddQueryParam("language", "en-US");

        for (var i = 1; i <= maxPages; i++)
        {
            builder.AddQueryParam("page", i, true);
            var request = new ImportListRequest(builder.Build());

            var response = FetchImportListResponse(request);
            var resource = JsonSerializer.Deserialize<TMDbPagedResource<TMDbAccountListResource>>(response.Content);

            options.AddRange(resource.Results
                .Select(r => new FieldSelectStringOption
                {
                    Name = r.Name,
                    Value = r.Id.ToString(CultureInfo.InvariantCulture),
                    Hint = $"(Id: {r.Id}, Items: {r.TotalItemsCount})"
                }));

            if (resource.Results.Count < PageSize || resource.Results.Count >= MaxNumResultsPerQuery)
            {
                break;
            }
        }

        return options;
    }
}
