using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class SourceSpecificationValidator : AbstractValidator<SourceSpecification>
    {
        public SourceSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class SourceSpecification : CustomFormatSpecificationBase
    {
        private static readonly SourceSpecificationValidator Validator = new SourceSpecificationValidator();

        public override int Order => 5;
        public override string ImplementationName => "Source";

        [FieldDefinition(1, Label = "Source", Type = FieldType.Select, SelectOptions = typeof(QualitySource))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(ParsedEpisodeInfo episodeInfo)
        {
            return (episodeInfo?.Quality?.Quality?.Source ?? (int)QualitySource.Unknown) == (QualitySource)Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
