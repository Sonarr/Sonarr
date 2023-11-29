using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class GenreSpecificationValidator : AbstractValidator<GenreSpecification>
    {
        public GenreSpecificationValidator()
        {
            RuleFor(c => c.Value).NotEmpty();
        }
    }

    public class GenreSpecification : AutoTaggingSpecificationBase
    {
        private static readonly GenreSpecificationValidator Validator = new ();

        public override int Order => 1;
        public override string ImplementationName => "Genre";

        [FieldDefinition(1, Label = "Genre(s)", Type = FieldType.Tag)]
        public IEnumerable<string> Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return series.Genres.Any(genre => Value.ContainsIgnoreCase(genre));
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
