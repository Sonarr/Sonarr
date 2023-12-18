using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public enum TraktUserListType
    {
        [FieldOption(Label = "User Watch List")]
        UserWatchList = 0,
        [FieldOption(Label = "User Watched List")]
        UserWatchedList = 1,
        [FieldOption(Label = "User Collection List")]
        UserCollectionList = 2
    }
}
