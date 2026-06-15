using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.List;

public class TmdbListRequestGenerator : TmdbRequestGeneratorBase<TmdbListSettings>
{
    public TmdbListRequestGenerator(TmdbListSettings settings, int maxPages)
        : base(settings, maxPages)
    {
    }

    protected override HttpRequestBuilder CreateSeriesRequestsBuilder()
    {
        return new HttpRequestBuilder(Settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {Settings.AuthToken}")
            .Resource($"4/list/{Settings.ListId ?? Settings.AccountListId}");
    }
}
