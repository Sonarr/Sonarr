using System;
using FluentValidation.Validators;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Validation.Paths
{
    public class FolderWritableValidator : PropertyValidator
    {
        private readonly IDiskProvider _diskProvider;

        public FolderWritableValidator(IDiskProvider diskProvider)
            : base($"Folder is not writable by user {Environment.UserName}")
        {
            _diskProvider = diskProvider;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            return _diskProvider.FolderWritable(context.PropertyValue.ToString());
        }
    }
}
