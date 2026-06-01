using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace NzbDrone.Core.ImportLists.Tmdb;

[DebuggerDisplay("Page: {Page}/{TotalPages}, Page Results: {Results.Count,n0}")]
public class TmdbPagedResource<TPagedResult>
    where TPagedResult : new()
{
    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("results")]
    public IReadOnlyList<TPagedResult> Results { get; init; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("total_results")]
    public int TotalResults { get; init; }
}

public class TmdbAccountListResource
{
    [JsonPropertyName("account_object_id")]
    public string AccountObjectId { get; init; }

    [JsonPropertyName("adult")]
    public int AdultCount { get; init; }

    [JsonPropertyName("average_rating")]
    public float AverageRating { get; init; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("featured")]
    public int FeaturedCount { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("iso_3166_1")]
    public string CountryCode { get; init; }

    [JsonPropertyName("iso_639_1")]
    public string LanguageCode { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("number_of_items")]
    public int TotalItemsCount { get; init; }

    [JsonPropertyName("public")]
    public int Public { get; init; }

    [JsonPropertyName("revenue")]
    public long Revenue { get; init; }

    [JsonPropertyName("runtime")]
    public string Runtime { get; init; }

    [JsonPropertyName("sort_by")]
    public int SortBy { get; init; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; init; }
}

[DebuggerDisplay("Name: {Name}, Id: {Id}, Language: {OriginalLanguage}")]
public class TmdbMediaResource
{
    [JsonPropertyName("adult")]
    public bool Adult { get; init; }

    [JsonPropertyName("backdrop_path")]
    public string BackdropPath { get; init; }

    [JsonPropertyName("genre_ids")]
    public IReadOnlyList<int> GenreIds { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("origin_country")]
    public IReadOnlyList<string> OriginCountry { get; init; }

    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; init; }

    [JsonPropertyName("original_name")]
    public string OriginalName { get; init; }

    [JsonPropertyName("overview")]
    public string Overview { get; init; }

    [JsonPropertyName("popularity")]
    public float Popularity { get; init; }

    [JsonPropertyName("poster_path")]
    public string PosterPath { get; init; }

    [JsonPropertyName("media_type")]
    public string MediaType { get; init; }

    [JsonPropertyName("first_air_date")]
    public string FirstAirDate { get; init; }

    [JsonPropertyName("softcore")]
    public bool Softcore { get; init; }

    [JsonPropertyName("vote_average")]
    public float VoteAverage { get; init; }

    [JsonPropertyName("vote_count")]
    public int VoteCount { get; init; }
}

[DebuggerDisplay("Id: {Id}, Cast: {Cast.Count,n0}, Crew: {Crew.Count,n0}")]
public class TmdbCreditsResource
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("cast")]
    public IReadOnlyList<TmdbCastResource> Cast { get; init; }

    [JsonPropertyName("crew")]
    public IReadOnlyList<TmdbCrewResource> Crew { get; init; }
}

public class TmdbCastResource : TmdbMediaResource
{
    [JsonPropertyName("character")]
    public string Character { get; init; }

    [JsonPropertyName("credit_id")]
    public string CreditId { get; init; }

    [JsonPropertyName("episode_count")]
    public int EpisodeCount { get; init; }

    [JsonPropertyName("first_credit_air_date")]
    public string FirstCreditAirDate { get; init; }
}

public class TmdbCrewResource : TmdbMediaResource
{
    [JsonPropertyName("job")]
    public string Job { get; init; }

    [JsonPropertyName("department")]
    public string Department { get; init; }

    [JsonPropertyName("credit_id")]
    public string CreditId { get; init; }

    [JsonPropertyName("episode_count")]
    public int EpisodeCount { get; init; }

    [JsonPropertyName("first_credit_air_date")]
    public string FirstCreditAirDate { get; init; }
}
