using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class YearSpecificationValidator : AbstractValidator<YearSpecification>
    {
        public YearSpecificationValidator()
        {
            RuleFor(c => c.Min).NotEmpty();
            RuleFor(c => c.Min).GreaterThan(0);
            RuleFor(c => c.Max).NotEmpty();
            RuleFor(c => c.Max).GreaterThan(c => c.Min);
        }
    }

    public class YearSpecification : AutoTaggingSpecificationBase
    {
        private static readonly YearSpecificationValidator Validator = new ();

        public override int Order => 1;
        public override string ImplementationName => "Year";

        [FieldDefinition(1, Label = "Minimum Year", Type = FieldType.Number)]
        public int Min { get; set; }

        [FieldDefinition(2, Label = "Maximum Year", Type = FieldType.Number)]
        public int Max { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return series.Year >= Min && series.Year <= Max;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
