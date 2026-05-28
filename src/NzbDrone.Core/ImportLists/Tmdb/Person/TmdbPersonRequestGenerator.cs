using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Person;

public class TmdbPersonRequestGenerator : TmdbRequestGeneratorBase<TmdbPersonSettings>
{
    public TmdbPersonRequestGenerator(TmdbPersonSettings settings)
        : base(settings)
    {
    }

    protected override void SetupSeriesRequestsBuilder(HttpRequestBuilder builder)
    {
        builder.Resource($"3/person/{Settings.PersonId}/tv_credits");
    }
}
