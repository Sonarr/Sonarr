using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Account;

public enum TmdbAccountListType
{
    [FieldOption(Label = "ImportListsTmdbSettingsAccountListTypeFavorite")]
    Favorite = 0,
    [FieldOption(Label = "ImportListsTmdbSettingsAccountListTypeRated")]
    Rated = 1,
    [FieldOption(Label = "ImportListsTmdbSettingsAccountListTypeRecommended")]
    Recommended = 2,
    [FieldOption(Label = "ImportListsTmdbSettingsAccountListTypeWatchlist")]
    Watchlist = 3
}
