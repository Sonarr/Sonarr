using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public enum TmdbDiscoverSortType
{
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortTypeFirstAirDate")]
    FirstAirDate = 0,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortTypeName")]
    Name = 1,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortTypeOriginalName")]
    OriginalName = 2,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortTypePopularity")]
    Popularity = 3,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortTypeVoteAverage")]
    VoteAverage = 4,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortTypeVoteCount")]
    VoteCount = 5,
}
