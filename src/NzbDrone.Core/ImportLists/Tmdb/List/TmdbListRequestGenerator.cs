using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.List;

public class TmdbListRequestGenerator : TmdbRequestGeneratorBase<TmdbListSettings>
{
    public TmdbListRequestGenerator(TmdbListSettings settings, int maxPages)
        : base(settings, maxPages)
    {
    }

    protected override void SetupSeriesRequestsBuilder(HttpRequestBuilder builder)
    {
        builder.ResourceUrl = $"4/list/{(Settings.ListId.IsNotNullOrWhiteSpace()
            ? Settings.ListId : Settings.AccountListId)}";
    }
}
