using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NzbDrone.Core.ImportLists.Tmdb;

public class TmdbPagedResource<TPagedResult>
    where TPagedResult : new()
{
    public IReadOnlyList<TPagedResult> Results { get; init; }
}

public class TmdbAccountListResource
{
    public int Id { get; init; }

    public string Name { get; init; }

    [JsonPropertyName("number_of_items")]
    public int TotalItemsCount { get; init; }
}

public class TmdbMediaResource
{
    public int Id { get; init; }

    public string Name { get; init; }

    [JsonPropertyName("media_type")]
    public string MediaType { get; init; }
}

public class TmdbCreditsResource
{
    public IReadOnlyList<TmdbCastResource> Cast { get; init; }

    public IReadOnlyList<TmdbCrewResource> Crew { get; init; }
}

public class TmdbCastResource : TmdbMediaResource
{
}

public class TmdbCrewResource : TmdbMediaResource
{
    public string Department { get; init; }
}
