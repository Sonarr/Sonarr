using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.List;

public class TMDbListRequestGenerator : TMDbRequestGeneratorBase<TMDbListSettings>
{
    public TMDbListRequestGenerator(TMDbListSettings settings)
        : base(settings)
    {
    }

    protected override void SetupSeriesRequestsBuilder(HttpRequestBuilder builder)
    {
        builder.ResourceUrl = $"4/list/{(Settings.ListId.IsNotNullOrWhiteSpace()
            ? Settings.ListId : Settings.AccountListId)}";
    }
}
