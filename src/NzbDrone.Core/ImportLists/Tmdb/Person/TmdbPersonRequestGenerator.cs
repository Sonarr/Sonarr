using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Person;

public class TmdbPersonRequestGenerator(TmdbPersonSettings settings) : IImportListRequestGenerator
{
    public ImportListPageableRequestChain GetListItems()
    {
        var pageableRequests = new ImportListPageableRequestChain();
        pageableRequests.Add(GetSeriesRequests());
        return pageableRequests;
    }

    private IEnumerable<ImportListRequest> GetSeriesRequests()
    {
        var builder = new HttpRequestBuilder(settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {settings.AuthToken}")
            .Resource($"3/person/{settings.PersonId}/tv_credits");

        yield return new ImportListRequest(builder.Build());
    }
}
