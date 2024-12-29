using FluentValidation;
using Workarr.Annotations;
using Workarr.Parser.Model;
using Workarr.Validation;

namespace Workarr.CustomFormats.Specifications
{
    public class SeasonPackSpecificationValidator : AbstractValidator<ReleaseTypeSpecification>
    {
        public SeasonPackSpecificationValidator()
        {
            RuleFor(c => c.Value).Custom((releaseType, context) =>
            {
                if (!Enum.IsDefined(typeof(ReleaseType), releaseType))
                {
                    context.AddFailure($"Invalid release type condition value: {releaseType}");
                }
            });
        }
    }

    public class ReleaseTypeSpecification : CustomFormatSpecificationBase
    {
        private static readonly SeasonPackSpecificationValidator Validator = new ();

        public override int Order => 10;
        public override string ImplementationName => "Release Type";

        [FieldDefinition(1, Label = "ReleaseType", Type = FieldType.Select, SelectOptions = typeof(ReleaseType))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return input.ReleaseType == (ReleaseType)Value;
        }

        public override WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
