using FluentValidation;
using Workarr.Annotations;
using Workarr.Extensions;
using Workarr.Validation;

namespace Workarr.CustomFormats.Specifications
{
    public class SizeSpecificationValidator : AbstractValidator<SizeSpecification>
    {
        public SizeSpecificationValidator()
        {
            RuleFor(c => c.Min).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Max).GreaterThan(c => c.Min);
        }
    }

    public class SizeSpecification : CustomFormatSpecificationBase
    {
        private static readonly SizeSpecificationValidator Validator = new SizeSpecificationValidator();

        public override int Order => 8;
        public override string ImplementationName => "Size";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationMinimumSize", HelpText = "CustomFormatsSpecificationMinimumSizeHelpText", Unit = "GB", Type = FieldType.Number)]
        public double Min { get; set; }

        [FieldDefinition(1, Label = "CustomFormatsSpecificationMaximumSize", HelpText = "CustomFormatsSpecificationMaximumSizeHelpText", Unit = "GB", Type = FieldType.Number)]
        public double Max { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            var size = input.Size;

            return size > Min.Gigabytes() && size <= Max.Gigabytes();
        }

        public override WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
