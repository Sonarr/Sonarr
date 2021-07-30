using FluentValidation.Validators;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Validation.Paths
{
    public class FileExistsValidator : PropertyValidator
    {
        private readonly IDiskProvider _diskProvider;

        public FileExistsValidator(IDiskProvider diskProvider)
            : base("File does not exist")
        {
            _diskProvider = diskProvider;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            return _diskProvider.FileExists(context.PropertyValue.ToString());
        }
    }
}
