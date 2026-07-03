using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class SourceSpecificationValidator : AbstractValidator<SourceSpecification>
    {
        public SourceSpecificationValidator()
        {
            RuleFor(c => c.Value).Custom((value, context) =>
            {
                if (!Enum.IsDefined(typeof(QualitySource), value))
                {
                    context.AddFailure($"Invalid source condition value: {value}");
                }
            });
        }
    }

    public class SourceSpecification : CustomFormatSpecificationBase
    {
        private static readonly SourceSpecificationValidator Validator = new();

        public override int Order => 5;
        public override string ImplementationName => "Source";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationSource", Type = FieldType.Select, SelectOptions = typeof(QualitySource))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return (input.EpisodeInfo?.Quality?.Quality?.Source ?? (int)QualitySource.Unknown) == (QualitySource)Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
