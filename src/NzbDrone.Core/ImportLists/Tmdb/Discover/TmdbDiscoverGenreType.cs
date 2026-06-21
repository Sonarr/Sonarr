using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public enum TmdbDiscoverGenreType
{
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeActionAdventure")]
    ActionAdventure = 10759,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeAnimation")]
    Animation = 16,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeComedy")]
    Comedy = 35,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeCrime")]
    Crime = 80,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeDocumentary")]
    Documentary = 99,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeDrama")]
    Drama = 18,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeFamily")]
    Family = 10751,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeKids")]
    Kids = 10762,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeMystery")]
    Mystery = 9648,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeNews")]
    News = 10763,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeReality")]
    Reality = 10764,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeSciFiFantasy")]
    SciFiFantasy = 10765,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeSoap")]
    Soap = 10766,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeTalk")]
    Talk = 10767,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeWarPolitics")]
    WarPolitics = 10768,
    [FieldOption(Label = "ImportListsTmdbSettingsDiscoverGenreTypeWestern")]
    Western = 37
}
