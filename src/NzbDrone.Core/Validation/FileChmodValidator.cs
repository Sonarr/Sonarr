using FluentValidation.Validators;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Validation
{
    public class FileChmodValidator : PropertyValidator
    {
        private readonly IDiskProvider _diskProvider;

        public FileChmodValidator(IDiskProvider diskProvider)
            : base("Must contain a valid Unix permissions octal")
        {
            _diskProvider = diskProvider;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return false;

            return _diskProvider.IsValidFilePermissionMask(context.PropertyValue.ToString());
        }
    }
}