using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.TMDb.Person;

public enum TMDbCrewDepartment
{
    Art,
    Camera,

    [FieldOption(label: "Costume & Makeup", Hint = "")]
    CostumeMakeup,

    Crew,
    Directing,
    Editing,
    Lighting,
    Production,
    Sound,

    [FieldOption(label: "Visual Effects", Hint = "")]
    VisualEffects,

    Writing
}
