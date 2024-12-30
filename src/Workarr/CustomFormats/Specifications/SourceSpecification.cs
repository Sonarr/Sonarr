using FluentValidation;
using Workarr.Annotations;
using Workarr.Qualities;
using Workarr.Validation;

namespace Workarr.CustomFormats.Specifications
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

        [FieldDefinition(1, Label = "CustomFormatsSpecificationSource", Type = FieldType.Select, SelectOptions = typeof(QualitySource))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return (input.EpisodeInfo?.Quality?.Quality?.Source ?? (int)QualitySource.Unknown) == (QualitySource)Value;
        }

        public override WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
