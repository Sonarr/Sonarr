using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb;

// Numeric values are persisted in import list settings, never reorder or reuse them.
public enum TmdbDepartmentType
{
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeArt")]
    Art = 0,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeCamera")]
    Camera = 1,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeCostumeMakeup")]
    CostumeMakeup = 2,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeCrew")]
    Crew = 3,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeDirecting")]
    Directing = 4,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeEditing")]
    Editing = 5,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeLighting")]
    Lighting = 6,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeProduction")]
    Production = 7,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeSound")]
    Sound = 8,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeVisualEffects")]
    VisualEffects = 9,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeWriting")]
    Writing = 10
}
