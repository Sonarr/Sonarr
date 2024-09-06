using System.Diagnostics.CodeAnalysis;

namespace Sonarr.Api.V3.Qualities;

[SuppressMessage("StyleCop.CSharp.NamingRules",
    "SA1313:Parameter names should begin with lower-case letter",
    Justification =
        "False positive for record types since params should follow property naming rules")]
public record QualityDefinitionLimitsResource(int MinLimit, int MaxLimit);
