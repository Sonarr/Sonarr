using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public enum MyAnimeListStatus
    {
        [FieldOption(label: "All")]
        All = 0,

        [FieldOption(label: "Watching")]
        Watching = 1,

        [FieldOption(label: "Completed")]
        Completed = 2,

        [FieldOption(label: "On Hold")]
        OnHold = 3,

        [FieldOption(label: "Dropped")]
        Dropped = 4,

        [FieldOption(label: "Plan to Watch")]
        PlanToWatch = 5
    }
}
