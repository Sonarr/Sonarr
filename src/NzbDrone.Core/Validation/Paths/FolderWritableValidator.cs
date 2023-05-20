using System;
using FluentValidation.Validators;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Validation.Paths
{
    public class FolderWritableValidator : PropertyValidator
    {
        private readonly IDiskProvider _diskProvider;

        public FolderWritableValidator(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        protected override string GetDefaultMessageTemplate() => "Folder '{path}' is not writable by user '{user}'";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());
            context.MessageFormatter.AppendArgument("user", Environment.UserName);

            return _diskProvider.FolderWritable(context.PropertyValue.ToString());
        }
    }
}
