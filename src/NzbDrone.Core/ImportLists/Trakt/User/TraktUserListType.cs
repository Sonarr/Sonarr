using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public enum TraktUserListType
    {
        [FieldOption(Label = "ImportListsTraktSettingsUserListTypeWatch")]
        UserWatchList = 0,
        [FieldOption(Label = "ImportListsTraktSettingsUserListTypeWatched")]
        UserWatchedList = 1,
        [FieldOption(Label = "ImportListsTraktSettingsUserListTypeCollection")]
        UserCollectionList = 2
    }
}
