using FluentValidation;
using Workarr.Annotations;
using Workarr.Tv;
using Workarr.Validation;

namespace Workarr.AutoTagging.Specifications
{
    public class YearSpecificationValidator : AbstractValidator<YearSpecification>
    {
        public YearSpecificationValidator()
        {
            RuleFor(c => c.Min).NotEmpty();
            RuleFor(c => c.Min).GreaterThan(0);
            RuleFor(c => c.Max).NotEmpty();
            RuleFor(c => c.Max).GreaterThanOrEqualTo(c => c.Min);
        }
    }

    public class YearSpecification : AutoTaggingSpecificationBase
    {
        private static readonly YearSpecificationValidator Validator = new ();

        public override int Order => 1;
        public override string ImplementationName => "Year";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationMinimumYear", Type = FieldType.Number)]
        public int Min { get; set; }

        [FieldDefinition(2, Label = "AutoTaggingSpecificationMaximumYear", Type = FieldType.Number)]
        public int Max { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return series.Year >= Min && series.Year <= Max;
        }

        public override WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
