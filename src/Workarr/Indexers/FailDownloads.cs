using Workarr.Annotations;

namespace Workarr.Indexers;

public enum FailDownloads
{
    [FieldOption(Label = "Executables")]
    Executables = 0,

    [FieldOption(Label = "Potentially Dangerous")]
    PotentiallyDangerous = 1
}
