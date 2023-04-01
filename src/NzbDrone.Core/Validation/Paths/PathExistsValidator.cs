using FluentValidation.Validators;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Validation.Paths
{
    public class PathExistsValidator : PropertyValidator
    {
        private readonly IDiskProvider _diskProvider;

        public PathExistsValidator(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        protected override string GetDefaultMessageTemplate() => "Path does not exist";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            return _diskProvider.FolderExists(context.PropertyValue.ToString());
        }
    }
}
