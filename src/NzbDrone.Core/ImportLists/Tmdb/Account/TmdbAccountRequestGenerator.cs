using System;
using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Account;

public class TmdbAccountRequestGenerator(TmdbAccountSettings settings, int maxPages) : IImportListRequestGenerator
{
    public ImportListPageableRequestChain GetListItems()
    {
        var pageableRequests = new ImportListPageableRequestChain();
        pageableRequests.Add(GetSeriesRequests());
        return pageableRequests;
    }

    private IEnumerable<ImportListRequest> GetSeriesRequests()
    {
        var builder = CreateSeriesRequestsBuilder();

        for (var i = 1; i <= maxPages; i++)
        {
            builder.AddQueryParam("page", i, true);
            yield return new ImportListRequest(builder.Build());
        }
    }

    private HttpRequestBuilder CreateSeriesRequestsBuilder()
    {
        var builder = new HttpRequestBuilder(settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {settings.AuthToken}");

        var accountId = Uri.EscapeDataString(settings.AccountId ?? string.Empty);

        builder.ResourceUrl = (TmdbAccountListType)settings.AccountListType switch
        {
            TmdbAccountListType.Rated => $"4/account/{accountId}/tv/rated",
            TmdbAccountListType.Recommended => $"4/account/{accountId}/tv/recommendations",
            TmdbAccountListType.Watchlist => $"4/account/{accountId}/tv/watchlist",
            _ => $"4/account/{accountId}/tv/favorites"
        };

        return builder;
    }
}
