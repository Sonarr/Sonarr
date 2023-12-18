using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public enum TraktUserWatchedListType
    {
        [FieldOption(Label = "All")]
        All = 0,
        [FieldOption(Label = "In Progress")]
        InProgress = 1,
        [FieldOption(Label = "100% Watched")]
        CompletelyWatched = 2
    }
}
