using FluentValidation.Validators;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Validation.Paths
{
    public class StartupFolderValidator : PropertyValidator
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public StartupFolderValidator(IAppFolderInfo appFolderInfo)
        {
            _appFolderInfo = appFolderInfo;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' cannot be {relationship} the start up folder";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var startupFolder = _appFolderInfo.StartUpFolder;
            var folder = context.PropertyValue.ToString();
            context.MessageFormatter.AppendArgument("path", folder);

            if (startupFolder.PathEquals(folder))
            {
                context.MessageFormatter.AppendArgument("relationship", "set to");

                return false;
            }

            if (startupFolder.IsParentPath(folder))
            {
                context.MessageFormatter.AppendArgument("relationship", "child of");

                return false;
            }

            return true;
        }
    }
}
