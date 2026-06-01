using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb;

public abstract class TmdbRequestGeneratorBase<TSettings> : IImportListRequestGenerator
    where TSettings : TmdbSettingsBase<TSettings>
{
    protected TmdbRequestGeneratorBase(TSettings settings, int maxPages)
    {
        Settings = settings;
        MaxPages = maxPages;
    }

    protected int MaxPages { get; }

    protected TSettings Settings { get; }

    public ImportListPageableRequestChain GetListItems()
    {
        var pageableRequests = new ImportListPageableRequestChain();
        pageableRequests.Add(GetSeriesRequests());
        return pageableRequests;
    }

    protected abstract HttpRequestBuilder CreateSeriesRequestsBuilder();

    private IEnumerable<ImportListRequest> GetSeriesRequests()
    {
        var builder = CreateSeriesRequestsBuilder();
        if (MaxPages > 0)
        {
            for (var i = 1; i <= MaxPages; i++)
            {
                builder.AddQueryParam("page", i, true);
                yield return new ImportListRequest(builder.Build());
            }
        }
        else
        {
            yield return new ImportListRequest(builder.Build());
        }
    }
}
