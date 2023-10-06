using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
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

        [FieldDefinition(1, Label = "Quality Profile", Type = FieldType.QualityProfile)]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return Value == series.QualityProfileId;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
