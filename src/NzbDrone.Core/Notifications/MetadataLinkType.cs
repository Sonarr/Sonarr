using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications
{
    public enum MetadataLinkType
    {
        [FieldOption(Label = "IMDb")]
        Imdb = 0,

        [FieldOption(Label = "TVDb")]
        Tvdb = 1,

        [FieldOption(Label = "TVMaze")]
        Tvmaze = 2,

        [FieldOption(Label = "Trakt")]
        Trakt = 3
    }
}
