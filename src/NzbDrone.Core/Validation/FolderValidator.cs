using FluentValidation.Validators;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Validation
{
    public class FolderValidator : PropertyValidator
    {
        public FolderValidator()
            : base("Invalid Path")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            return context.PropertyValue.ToString().IsPathValid();
        }
    }
}
