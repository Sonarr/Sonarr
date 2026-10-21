using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Tmdb.List;

public class TmdbListImport(ISonarrCloudRequestBuilder requestBuilder,
                            IHttpClient httpClient,
                            IImportListStatusService importListStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            ILocalizationService localizationService,
                            Logger logger)
    : TmdbImportBase<TmdbListSettings>(requestBuilder, httpClient, importListStatusService, configService, parsingService, localizationService, logger)
{
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
        return new TmdbListParser();
    }

    public override IImportListRequestGenerator GetRequestGenerator()
    {
        // Paging already stops at the last page, so 50 pages of 20 matches MaxNumResultsPerQuery (1000).
        return new TmdbListRequestGenerator(Settings, 50);
    }

    private List<FieldSelectOption<int>> GetAccountListOptions()
    {
        const int maxPages = 3;
        var options = new List<FieldSelectOption<int>>
        {
            new() { Name = "None", Value = 0 }
        };

        if (Settings.AuthToken.IsNullOrWhiteSpace() || Settings.AccountId.IsNullOrWhiteSpace())
        {
            return options;
        }

        var builder = new HttpRequestBuilder(Settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .Resource($"4/account/{Uri.EscapeDataString(Settings.AccountId)}/lists")
            .SetHeader("Authorization", $"Bearer {Settings.AuthToken}");

        try
        {
            for (var i = 1; i <= maxPages; i++)
            {
                builder.AddQueryParam("page", i, true);
                var request = new ImportListRequest(builder.Build());

                var response = FetchImportListResponse(request);
                var resource = STJson.Deserialize<TmdbPagedResource<TmdbAccountListResource>>(response.Content);

                var results = resource.Results ?? [];

                options.AddRange(results
                    .Select(r => new FieldSelectOption<int>
                    {
                        Name = r.Name,
                        Value = r.Id,
                        Hint = $"(Id: {r.Id}, Items: {r.NumberOfItems})"
                    }));

                if (results.Count < PageSize)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            // An expired token or a TMDb outage shouldn't turn the options request into a 500.
            _logger.Warn(ex, "Unable to fetch TMDb account lists");
        }

        return options;
    }
}
