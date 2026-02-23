using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class OriginalCountrySpecificationValidator : AbstractValidator<OriginalCountrySpecification>
    {
        public OriginalCountrySpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();

            RuleFor(c => c.Value).Custom((countries, context) =>
            {
                if (countries.Any(c => c.Length != 3))
                {
                    context.AddFailure("Country code must be 3 characters long");
                }
            });
        }
    }

    public class OriginalCountrySpecification : AutoTaggingSpecificationBase
    {
        private static readonly OriginalCountrySpecificationValidator Validator = new();

        public override int Order => 1;
        public override string ImplementationName => "Original Country";

        [FieldDefinition(1, Label = "AutoTaggingSpecificationOriginalCountry", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return Value.Any(network => series.OriginalCountry.EqualsIgnoreCase(network));
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
