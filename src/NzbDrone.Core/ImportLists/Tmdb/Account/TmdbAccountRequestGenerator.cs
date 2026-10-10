using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Account;

public class TmdbAccountRequestGenerator : IImportListRequestGenerator
{
    private readonly TmdbAccountSettings _settings;
    private readonly int _maxPages;

    public TmdbAccountRequestGenerator(TmdbAccountSettings settings, int maxPages)
    {
        _settings = settings;
        _maxPages = maxPages;
    }

    public ImportListPageableRequestChain GetListItems()
    {
        var pageableRequests = new ImportListPageableRequestChain();
        pageableRequests.Add(GetSeriesRequests());
        return pageableRequests;
    }

    private IEnumerable<ImportListRequest> GetSeriesRequests()
    {
        var builder = CreateSeriesRequestsBuilder();

        for (var i = 1; i <= _maxPages; i++)
        {
            builder.AddQueryParam("page", i, true);
            yield return new ImportListRequest(builder.Build());
        }
    }

    private HttpRequestBuilder CreateSeriesRequestsBuilder()
    {
        var builder = new HttpRequestBuilder(_settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {_settings.AuthToken}");

        builder.ResourceUrl = (TmdbAccountListType)_settings.AccountListType switch
        {
            TmdbAccountListType.Rated => $"4/account/{_settings.AccountId}/tv/rated",
            TmdbAccountListType.Recommended => $"4/account/{_settings.AccountId}/tv/recommendations",
            TmdbAccountListType.Watchlist => $"4/account/{_settings.AccountId}/tv/watchlist",
            _ => $"4/account/{_settings.AccountId}/tv/favorites"
        };

        return builder;
    }
}
