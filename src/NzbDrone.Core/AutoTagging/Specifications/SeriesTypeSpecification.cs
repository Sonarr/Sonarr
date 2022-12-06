using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class SeriesTypeSpecificationValidator : AbstractValidator<SeriesTypeSpecification>
    {
        public SeriesTypeSpecificationValidator()
        {
            RuleFor(c => (SeriesTypes)c.Value).IsInEnum();
        }
    }

    public class SeriesTypeSpecification : AutoTaggingSpecificationBase
    {
        private static readonly SeriesTypeSpecificationValidator Validator = new SeriesTypeSpecificationValidator();

        public override int Order => 2;
        public override string ImplementationName => "Series Type";

        [FieldDefinition(1, Label = "Series Type", Type = FieldType.Select, SelectOptions = typeof(SeriesTypes))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return (int)series.SeriesType == Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
