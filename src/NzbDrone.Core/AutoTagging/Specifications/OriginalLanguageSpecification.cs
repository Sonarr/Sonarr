using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class OriginalLanguageSpecificationValidator : AbstractValidator<OriginalLanguageSpecification>
    {
        public OriginalLanguageSpecificationValidator()
        {
            RuleFor(c => c.Value).GreaterThanOrEqualTo(0);
        }
    }

    public class OriginalLanguageSpecification : AutoTaggingSpecificationBase
    {
        private static readonly OriginalLanguageSpecificationValidator Validator = new ();

        public override int Order => 1;
        public override string ImplementationName => "Original Language";

        [FieldDefinition(1, Label = "Language", Type = FieldType.Select, SelectOptions = typeof(OriginalLanguageFieldConverter))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return Value == series.OriginalLanguage.Id;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
