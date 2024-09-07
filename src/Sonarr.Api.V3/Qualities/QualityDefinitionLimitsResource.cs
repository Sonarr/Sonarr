using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.V3.Qualities
{
    public class QualityDefinitionLimitsResource
    {
        public int Min { get; set; } = QualityDefinitionLimits.Min;
        public int Max { get; set; } = QualityDefinitionLimits.Max;
    }
}
