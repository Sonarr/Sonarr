using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb;

public enum TmdbCrewDepartmentType
{
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeArt")]
    Art,
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeCamera")]
    Camera,
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeCostumeMakeup")]
    CostumeMakeup,
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeCrew")]
    Crew,
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeDirecting")]
    Directing,
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeEditing")]
    Editing,
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeLighting")]
    Lighting,
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeProduction")]
    Production,
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeSound")]
    Sound,
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeVisualEffects")]
    VisualEffects,
    [FieldOption(Label = "ImportListsTmdbSettingsCrewDepartmentTypeWriting")]
    Writing
}
