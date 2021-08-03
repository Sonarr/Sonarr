using FluentValidation.Validators;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Validation
{
    public class FolderChmodValidator : PropertyValidator
    {
        private readonly IDiskProvider _diskProvider;

        public FolderChmodValidator(IDiskProvider diskProvider)
            : base("Must contain a valid Unix permissions octal")
        {
            _diskProvider = diskProvider;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            return _diskProvider.IsValidFolderPermissionMask(context.PropertyValue.ToString());
        }
    }
}
