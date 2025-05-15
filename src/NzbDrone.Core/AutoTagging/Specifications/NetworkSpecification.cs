using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class NetworkSpecificationValidator : AbstractValidator<NetworkSpecification>
    {
        public NetworkSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class NetworkSpecification : AutoTaggingSpecificationBase
    {
        private static readonly NetworkSpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Network";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationNetwork", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return Value.Any(network => series.Network.EqualsIgnoreCase(network));
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
