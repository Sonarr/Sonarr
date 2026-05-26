using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.ImportLists.TMDb;

public class TMDbToken
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    [SetsRequiredMembers]
    private TMDbToken(JwtSecurityToken jwt)
    {
        Raw = jwt.RawData;

        if (jwt.Payload.TryGetValue("redirect_to", out var redirectToObj))
        {
            RedirectTo = (string)redirectToObj;
        }

        foreach (var claim in jwt.Claims.Where(c => c.Type == "scopes"))
        {
            CanRead |= claim.Value == "api_read";
            CanWrite |= claim.Value == "api_write";
            IsPendingApproval |= claim.Value == "pending_request_token";
        }
    }

    public string RedirectTo { get; init; }
    public bool IsPendingApproval { get; init; }

    public required string Raw { get; init; }
    public required bool CanRead { get; init; }
    public required bool CanWrite { get; init; }

    public static bool TryParse(string raw, out TMDbToken token)
    {
        token = null;
        JwtSecurityToken deserializedToken;

        if (raw.IsNullOrWhiteSpace())
        {
            return false;
        }

        try
        {
            deserializedToken = Handler.ReadJwtToken(raw);
        }
        catch (SecurityTokenMalformedException)
        {
            return false;
        }

        token = new TMDbToken(deserializedToken);
        return true;
    }

    public override string ToString() => Raw;
}

public class RequestTokenResponse
{
    [JsonPropertyName("success")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("status_code")]
    public int StatusCode { get; init; }

    [JsonPropertyName("status_message")]
    public string StatusMessage { get; init; }

    [JsonPropertyName("request_token")]
    public string RequestToken { get; init; }

    [JsonPropertyName("statusCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
    public int SonarrStatusCode
    {
        get => StatusCode;
        init => StatusCode = value;
    }

    [JsonPropertyName("statusMessage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
    public string SonarrStatusMessage
    {
        get => StatusMessage;
        init => StatusMessage = value;
    }

    [JsonPropertyName("requestToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
    public string SonarrRequestToken
    {
        get => RequestToken;
        init => RequestToken = value;
    }
}

public class AccessTokenResponse
{
    [JsonPropertyName("success")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("status_code")]
    public int StatusCode { get; init; }

    [JsonPropertyName("status_message")]
    public string StatusMessage { get; init; }

    [JsonPropertyName("account_id")]
    public string AccountId { get; init; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonIgnore]
    public string AuthToken => AccessToken;
}

[DebuggerDisplay("Page: {Page}/{TotalPages}, Page Results: {Results.Count,n0}")]
public class TMDbPagedResource<TPagedResult>
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

public class TMDbAccountListResource
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
public class TMDbMediaResource
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
public class TMDbCreditsResource
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("cast")]
    public IReadOnlyList<TMDbCastResource> Cast { get; init; }

    [JsonPropertyName("crew")]
    public IReadOnlyList<TMDbCrewResource> Crew { get; init; }
}

public class TMDbCastResource : TMDbMediaResource
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

public class TMDbCrewResource : TMDbMediaResource
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
