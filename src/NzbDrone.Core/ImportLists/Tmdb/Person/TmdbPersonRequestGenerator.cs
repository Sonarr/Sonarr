using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Person;

public class TmdbPersonRequestGenerator : IImportListRequestGenerator
{
    private readonly TmdbPersonSettings _settings;

    public TmdbPersonRequestGenerator(TmdbPersonSettings settings)
    {
        _settings = settings;
    }

    public ImportListPageableRequestChain GetListItems()
    {
        var pageableRequests = new ImportListPageableRequestChain();
        pageableRequests.Add(GetSeriesRequests());
        return pageableRequests;
    }

    private IEnumerable<ImportListRequest> GetSeriesRequests()
    {
        var builder = new HttpRequestBuilder(_settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {_settings.AuthToken}")
            .Resource($"3/person/{_settings.PersonId}/tv_credits");

        yield return new ImportListRequest(builder.Build());
    }
}
