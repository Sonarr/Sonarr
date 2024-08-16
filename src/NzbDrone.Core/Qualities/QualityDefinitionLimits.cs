using System.Diagnostics.CodeAnalysis;

namespace NzbDrone.Core.Qualities;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification =
    "Serializable properties of a DTO")]
public record QualityDefinitionLimits
{
    public int MinLimit => 0;
    public int MaxLimit => 1000;
}
