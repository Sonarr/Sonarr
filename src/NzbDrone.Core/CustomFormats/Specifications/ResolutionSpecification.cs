using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class ResolutionSpecificationValidator : AbstractValidator<ResolutionSpecification>
    {
        public ResolutionSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class ResolutionSpecification : CustomFormatSpecificationBase
    {
        private static readonly ResolutionSpecificationValidator Validator = new ResolutionSpecificationValidator();

        public override int Order => 6;
        public override string ImplementationName => "Resolution";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationResolution", Type = FieldType.Select, SelectOptions = typeof(Resolution))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return (input.EpisodeInfo?.Quality?.Quality?.Resolution ?? (int)Resolution.Unknown) == Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
