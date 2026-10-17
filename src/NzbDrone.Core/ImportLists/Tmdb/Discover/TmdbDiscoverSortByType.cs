using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public enum TmdbDiscoverSortByType
{
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypeFirstAirDateAsc")]
    FirstAirDateAsc = 0,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypeFirstAirDateDesc")]
    FirstAirDateDesc = 1,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypeNameAsc")]
    NameAsc = 2,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypeNameDesc")]
    NameDesc = 3,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypeOriginalNameAsc")]
    OriginalNameAsc = 4,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypeOriginalNameDesc")]
    OriginalNameDesc = 5,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypePopularityAsc")]
    PopularityAsc = 6,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypePopularityDesc")]
    PopularityDesc = 7,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypeVoteAverageAsc")]
    VoteAverageAsc = 8,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypeVoteAverageDesc")]
    VoteAverageDesc = 9,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypeVoteCountAsc")]
    VoteCountAsc = 10,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortByTypeVoteCountDesc")]
    VoteCountDesc = 11
}
