using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NzbDrone.Core.MetadataSource.Tmdb.Resource
{
    public class TmdbTvSearchResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbTvSearchResult> Results { get; set; } = new List<TmdbTvSearchResult>();
    }

    public class TmdbFindResponse
    {
        [JsonPropertyName("tv_results")]
        public List<TmdbFindTvResult> TvResults { get; set; } = new List<TmdbFindTvResult>();
    }

    public class TmdbFindTvResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class TmdbTvSearchResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("overview")]
        public string Overview { get; set; }

        [JsonPropertyName("first_air_date")]
        public string FirstAirDate { get; set; }

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; }

        [JsonPropertyName("backdrop_path")]
        public string BackdropPath { get; set; }

        [JsonPropertyName("original_language")]
        public string OriginalLanguage { get; set; }

        [JsonPropertyName("origin_country")]
        public List<string> OriginCountry { get; set; } = new List<string>();
    }

    public class TmdbTvDetailsResource
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("overview")]
        public string Overview { get; set; }

        [JsonPropertyName("first_air_date")]
        public string FirstAirDate { get; set; }

        [JsonPropertyName("last_air_date")]
        public string LastAirDate { get; set; }

        [JsonPropertyName("episode_run_time")]
        public List<int> EpisodeRunTime { get; set; } = new List<int>();

        [JsonPropertyName("genres")]
        public List<TmdbGenreResource> Genres { get; set; } = new List<TmdbGenreResource>();

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("networks")]
        public List<TmdbNetworkResource> Networks { get; set; } = new List<TmdbNetworkResource>();

        [JsonPropertyName("origin_country")]
        public List<string> OriginCountry { get; set; } = new List<string>();

        [JsonPropertyName("original_language")]
        public string OriginalLanguage { get; set; }

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; }

        [JsonPropertyName("backdrop_path")]
        public string BackdropPath { get; set; }

        [JsonPropertyName("vote_average")]
        public decimal VoteAverage { get; set; }

        [JsonPropertyName("vote_count")]
        public int VoteCount { get; set; }

        [JsonPropertyName("external_ids")]
        public TmdbExternalIdsResource ExternalIds { get; set; }

        [JsonPropertyName("aggregate_credits")]
        public TmdbAggregateCreditsResource AggregateCredits { get; set; }

        [JsonPropertyName("content_ratings")]
        public TmdbContentRatingsResource ContentRatings { get; set; }

        [JsonPropertyName("images")]
        public TmdbImagesResource Images { get; set; }

        [JsonPropertyName("seasons")]
        public List<TmdbSeasonSummaryResource> Seasons { get; set; } = new List<TmdbSeasonSummaryResource>();
    }

    public class TmdbSeasonDetailsResource
    {
        [JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; }

        [JsonPropertyName("episodes")]
        public List<TmdbEpisodeResource> Episodes { get; set; } = new List<TmdbEpisodeResource>();
    }

    public class TmdbEpisodeResource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("overview")]
        public string Overview { get; set; }

        [JsonPropertyName("air_date")]
        public string AirDate { get; set; }

        [JsonPropertyName("episode_number")]
        public int EpisodeNumber { get; set; }

        [JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

        [JsonPropertyName("runtime")]
        public int? Runtime { get; set; }

        [JsonPropertyName("vote_average")]
        public decimal VoteAverage { get; set; }

        [JsonPropertyName("vote_count")]
        public int VoteCount { get; set; }

        [JsonPropertyName("still_path")]
        public string StillPath { get; set; }

        [JsonPropertyName("episode_type")]
        public string EpisodeType { get; set; }
    }

    public class TmdbGenreResource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class TmdbNetworkResource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class TmdbExternalIdsResource
    {
        [JsonPropertyName("imdb_id")]
        public string ImdbId { get; set; }

        [JsonPropertyName("tvdb_id")]
        public int? TvdbId { get; set; }
    }

    public class TmdbAggregateCreditsResource
    {
        [JsonPropertyName("cast")]
        public List<TmdbCastResource> Cast { get; set; } = new List<TmdbCastResource>();
    }

    public class TmdbCastResource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("profile_path")]
        public string ProfilePath { get; set; }

        [JsonPropertyName("roles")]
        public List<TmdbRoleResource> Roles { get; set; } = new List<TmdbRoleResource>();
    }

    public class TmdbRoleResource
    {
        [JsonPropertyName("character")]
        public string Character { get; set; }
    }

    public class TmdbContentRatingsResource
    {
        [JsonPropertyName("results")]
        public List<TmdbContentRatingResource> Results { get; set; } = new List<TmdbContentRatingResource>();
    }

    public class TmdbContentRatingResource
    {
        [JsonPropertyName("iso_3166_1")]
        public string CountryCode { get; set; }

        [JsonPropertyName("rating")]
        public string Rating { get; set; }
    }

    public class TmdbImagesResource
    {
        [JsonPropertyName("posters")]
        public List<TmdbImageResource> Posters { get; set; } = new List<TmdbImageResource>();

        [JsonPropertyName("backdrops")]
        public List<TmdbImageResource> Backdrops { get; set; } = new List<TmdbImageResource>();

        [JsonPropertyName("logos")]
        public List<TmdbImageResource> Logos { get; set; } = new List<TmdbImageResource>();
    }

    public class TmdbImageResource
    {
        [JsonPropertyName("file_path")]
        public string FilePath { get; set; }
    }

    public class TmdbSeasonSummaryResource
    {
        [JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; }
    }
}
