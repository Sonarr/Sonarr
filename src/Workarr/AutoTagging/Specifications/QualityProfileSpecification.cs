using FluentValidation;
using Workarr.Annotations;
using Workarr.Tv;
using Workarr.Validation;

namespace Workarr.AutoTagging.Specifications
{
    public class QualityProfileSpecificationValidator : AbstractValidator<QualityProfileSpecification>
    {
        public QualityProfileSpecificationValidator()
        {
            RuleFor(c => c.Value).GreaterThan(0);
        }
    }

    public class QualityProfileSpecification : AutoTaggingSpecificationBase
    {
        private static readonly QualityProfileSpecificationValidator Validator = new ();

        public override int Order => 1;
        public override string ImplementationName => "Quality Profile";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationQualityProfile", Type = FieldType.QualityProfile)]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return Value == series.QualityProfileId;
        }

        public override WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
