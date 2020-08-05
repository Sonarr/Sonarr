using System.Runtime.Serialization;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public enum TraktUserListType
    {
        [EnumMember(Value = "User Watch List")]
        UserWatchList = 0,
        [EnumMember(Value = "User Watched List")]
        UserWatchedList = 1,
        [EnumMember(Value = "User Collection List")]
        UserCollectionList = 2
    }
}
