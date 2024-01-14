using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public enum SimklUserListType
    {
        [FieldOption(Label = "ImportListsSimklSettingsUserListTypeWatching")]
        Watching = 0,
        [FieldOption(Label = "ImportListsSimklSettingsUserListTypePlanToWatch")]
        PlanToWatch = 1,
        [FieldOption(Label = "ImportListsSimklSettingsUserListTypeHold")]
        Hold = 2,
        [FieldOption(Label = "ImportListsSimklSettingsUserListTypeCompleted")]
        Completed = 3,
        [FieldOption(Label = "ImportListsSimklSettingsUserListTypeDropped")]
        Dropped = 4
    }
}
