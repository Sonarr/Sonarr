using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb;

public abstract class TmdbRequestGeneratorBase<TSettings> : IImportListRequestGenerator
    where TSettings : TmdbSettingsBase<TSettings>
{
    protected TmdbRequestGeneratorBase(TSettings settings)
    {
        Settings = settings;
    }

    protected TSettings Settings { get; }

    public ImportListPageableRequestChain GetListItems()
    {
        var pageableRequests = new ImportListPageableRequestChain();
        pageableRequests.Add(GetSeriesRequests());
        return pageableRequests;
    }

    protected abstract void SetupSeriesRequestsBuilder(HttpRequestBuilder builder);

    private IEnumerable<ImportListRequest> GetSeriesRequests()
    {
        var builder = new HttpRequestBuilder(Settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {Settings.AuthToken}")
            .AddQueryParam("language", "en-US");

        SetupSeriesRequestsBuilder(builder);
        if (Settings.MaxPages > 0)
        {
            for (var i = 1; i <= Settings.MaxPages; i++)
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
