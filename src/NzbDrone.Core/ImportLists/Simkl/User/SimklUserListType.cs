using System.Runtime.Serialization;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public enum SimklUserListType
    {
        [EnumMember(Value = "Watching")]
        Watching = 0,
        [EnumMember(Value = "Plan To Watch")]
        PlanToWatch = 1,
        [EnumMember(Value = "Hold")]
        Hold = 2,
        [EnumMember(Value = "Completed")]
        Completed = 3,
        [EnumMember(Value = "Dropped")]
        Dropped = 4
    }
}
