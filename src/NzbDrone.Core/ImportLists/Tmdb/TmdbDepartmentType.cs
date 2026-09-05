using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb;

public enum TmdbDepartmentType
{
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeArt")]
    Art,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeCamera")]
    Camera,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeCostumeMakeup")]
    CostumeMakeup,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeCrew")]
    Crew,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeDirecting")]
    Directing,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeEditing")]
    Editing,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeLighting")]
    Lighting,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeProduction")]
    Production,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeSound")]
    Sound,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeVisualEffects")]
    VisualEffects,
    [FieldOption(Label = "ImportListsTmdbSettingsDepartmentTypeWriting")]
    Writing
}
