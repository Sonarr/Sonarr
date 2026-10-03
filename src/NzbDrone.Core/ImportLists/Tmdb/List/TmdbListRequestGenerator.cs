using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.List;

public class TmdbListRequestGenerator : IImportListRequestGenerator
{
    private readonly TmdbListSettings _settings;
    private readonly int _maxPages;

    public TmdbListRequestGenerator(TmdbListSettings settings, int maxPages)
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
        return new HttpRequestBuilder(_settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {_settings.AuthToken}")
            .Resource($"4/list/{_settings.ListId ?? _settings.AccountListId}");
    }
}
