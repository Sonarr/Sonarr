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
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.AuthToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.AccountId);

        var builder = new HttpRequestBuilder(settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {settings.AuthToken}");

        var accountId = Uri.EscapeDataString(settings.AccountId);

        switch (settings.AccountListType)
        {
            case (int)TmdbAccountListType.Rated:
                builder.Resource($"4/account/{accountId}/tv/rated")
                    .AddQueryParam("sort_by", "created_at.desc");
                break;
            case (int)TmdbAccountListType.Recommended:
                builder.Resource($"4/account/{accountId}/tv/recommendations");
                break;
            case (int)TmdbAccountListType.Watchlist:
                builder.Resource($"4/account/{accountId}/tv/watchlist")
                    .AddQueryParam("sort_by", "created_at.desc");
                break;
            case (int)TmdbAccountListType.Favorite:
                builder.Resource($"4/account/{accountId}/tv/favorites")
                    .AddQueryParam("sort_by", "created_at.desc");
                break;
        }

        return builder;
    }
}
