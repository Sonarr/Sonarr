using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class StatusSpecificationValidator : AbstractValidator<StatusSpecification>
    {
    }

    public class StatusSpecification : AutoTaggingSpecificationBase
    {
        private static readonly StatusSpecificationValidator Validator = new ();

        public override int Order => 1;
        public override string ImplementationName => "Status";

        [FieldDefinition(1, Label = "Status", Type = FieldType.Select, SelectOptions = typeof(SeriesStatusType))]
        public int Status { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return series.Status == (SeriesStatusType)Status;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
