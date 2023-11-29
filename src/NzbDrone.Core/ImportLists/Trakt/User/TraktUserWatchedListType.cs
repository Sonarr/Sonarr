using System.Runtime.Serialization;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public enum TraktUserWatchedListType
    {
        [EnumMember(Value = "All")]
        All = 0,
        [EnumMember(Value = "In Progress")]
        InProgress = 1,
        [EnumMember(Value = "100% Watched")]
        CompletelyWatched = 2
    }
}
