using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Person;

public class TmdbPersonRequestGenerator : TmdbRequestGeneratorBase<TmdbPersonSettings>
{
    public TmdbPersonRequestGenerator(TmdbPersonSettings settings)
        : base(settings, 0)
    {
    }

    protected override HttpRequestBuilder CreateSeriesRequestsBuilder()
    {
        return new HttpRequestBuilder(Settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {Settings.AuthToken}")
            .Resource($"3/person/{Settings.PersonId}/tv_credits");
    }
}
