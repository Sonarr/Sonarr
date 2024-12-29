using Workarr.Annotations;

namespace Workarr.ImportLists.Trakt.User
{
    public enum TraktUserWatchedListType
    {
        [FieldOption(Label = "ImportListsTraktSettingsWatchedListTypeAll")]
        All = 0,
        [FieldOption(Label = "ImportListsTraktSettingsWatchedListTypeInProgress")]
        InProgress = 1,
        [FieldOption(Label = "ImportListsTraktSettingsWatchedListTypeCompleted")]
        CompletelyWatched = 2
    }
}
