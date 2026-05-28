using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Person;

public enum TmdbCrewDepartment
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
