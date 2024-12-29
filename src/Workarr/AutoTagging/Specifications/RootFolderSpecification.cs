using FluentValidation;
using Workarr.Annotations;
using Workarr.Extensions;
using Workarr.Tv;
using Workarr.Validation;
using Workarr.Validation.Paths;

namespace Workarr.AutoTagging.Specifications
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

        [FieldDefinition(1, Label = "AutoTaggingSpecificationRootFolder", Type = FieldType.RootFolder)]
        public string Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Series series)
        {
            return PathExtensions.PathEquals(series.RootFolderPath, Value);
        }

        public override WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
