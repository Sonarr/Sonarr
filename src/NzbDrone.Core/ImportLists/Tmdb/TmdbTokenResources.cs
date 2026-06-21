using System.Text.Json.Serialization;

namespace NzbDrone.Core.ImportLists.Tmdb;

public class RequestTokenResource
{
    [JsonPropertyName("success")]
    public bool IsSuccess { get; init; }

    public int StatusCode { get; init; }

    public string StatusMessage { get; init; }

    public string RequestToken { get; init; }
}

public class AccessTokenResource
{
    [JsonPropertyName("success")]
    public bool IsSuccess { get; init; }

    public int StatusCode { get; init; }

    public string StatusMessage { get; init; }

    public string AccountId { get; init; }

    public string AccessToken { get; init; }

    [JsonIgnore]
    public string AuthToken => AccessToken;
}
