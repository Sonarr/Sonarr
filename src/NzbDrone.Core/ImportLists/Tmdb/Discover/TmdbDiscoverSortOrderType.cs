using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public enum TmdbDiscoverSortOrderType
{
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortOrderTypeAscending")]
    Ascending = 0,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverSortOrderTypeDescending")]
    Descending = 1
}
