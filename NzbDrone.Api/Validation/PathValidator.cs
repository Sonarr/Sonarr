using FluentValidation.Validators;
using NzbDrone.Common;

namespace NzbDrone.Api.Validation
{
    public class PathValidator : PropertyValidator
    {
        public PathValidator()
            : base("Invalid Path")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            return context.PropertyValue.ToString().IsPathValid();
        }
    }
}