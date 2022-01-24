using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class ResolutionSpecification : CustomFormatSpecificationBase
    {
        public override int Order => 6;
        public override string ImplementationName => "Resolution";

        [FieldDefinition(1, Label = "Resolution", Type = FieldType.Select, SelectOptions = typeof(Resolution))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(ParsedEpisodeInfo episodeInfo)
        {
            return (episodeInfo?.Quality?.Quality?.Resolution ?? (int)Resolution.Unknown) == Value;
        }
    }
}
