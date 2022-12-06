using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class RootFolderSpecificationValidator : AbstractValidator<RootFolderSpecification>
    {
        public RootFolderSpecificationValidator()
        {
            RuleFor(c => c.Value).IsValidPath();
        }
    }

    public class RootFolderSpecification : AutoTaggingSpecificationBase
    {
        private static readonly RootFolderSpecificationValidator Validator = new RootFolderSpecificationValidator();

        public override int Order => 1;
        public override string ImplementationName => "Root Folder";

        [FieldDefinition(1, Label = "Root Folder", Type = FieldType.RootFolder)]
        public string Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return series.RootFolderPath.PathEquals(Value);
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
