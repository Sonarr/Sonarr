using FluentValidation.Validators;
using Workarr.Disk;

namespace Workarr.Validation.Paths
{
    public class FileExistsValidator : PropertyValidator
    {
        private readonly IDiskProvider _diskProvider;

        public FileExistsValidator(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        protected override string GetDefaultMessageTemplate() => "File '{file}' does not exist";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            context.MessageFormatter.AppendArgument("file", context.PropertyValue.ToString());

            return _diskProvider.FileExists(context.PropertyValue.ToString());
        }
    }
}
