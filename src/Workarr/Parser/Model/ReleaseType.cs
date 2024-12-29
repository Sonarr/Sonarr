using Workarr.Annotations;

namespace Workarr.Parser.Model
{
    public enum ReleaseType
    {
        Unknown = 0,

        [FieldOption(label: "Single Episode")]
        SingleEpisode = 1,

        [FieldOption(label: "Multi-Episode")]
        MultiEpisode = 2,

        [FieldOption(label: "Season Pack")]
        SeasonPack = 3
    }
}
