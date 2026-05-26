using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.Person;

public sealed class TMDbPersonRequestGenerator : TMDbRequestGeneratorBase<TMDbPersonSettings>
{
    public TMDbPersonRequestGenerator(TMDbPersonSettings settings)
        : base(settings)
    {
    }

    protected override void SetupSeriesRequestsBuilder(HttpRequestBuilder builder)
    {
        builder.Resource($"3/person/{Settings.PersonId}/tv_credits");
    }
}
