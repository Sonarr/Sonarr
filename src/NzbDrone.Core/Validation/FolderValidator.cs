using FluentValidation.Validators;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Validation
{
    public class FolderValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Invalid Path";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            return context.PropertyValue.ToString().IsPathValid(PathValidationType.CurrentOs);
        }
    }
}
