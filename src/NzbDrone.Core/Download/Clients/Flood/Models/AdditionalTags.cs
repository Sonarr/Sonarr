using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Download.Clients.Flood.Models
{
    public enum AdditionalTags
    {
        [FieldOption(Hint = "big-buck-bunny")]
        TitleSlug = 0,

        [FieldOption(Hint = "Bluray-2160p")]
        Quality = 1,

        [FieldOption(Hint = "English")]
        Languages = 2,

        [FieldOption(Hint = "Example-Raws")]
        ReleaseGroup = 3,

        [FieldOption(Hint = "2020")]
        Year = 4,

        [FieldOption(Hint = "Torznab")]
        Indexer = 5,

        [FieldOption(Hint = "C-SPAN")]
        Network = 6
    }
}
