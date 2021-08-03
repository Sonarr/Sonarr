using FluentValidation.Validators;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Validation.Paths
{
    public class StartupFolderValidator : PropertyValidator
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public StartupFolderValidator(IAppFolderInfo appFolderInfo)
            : base("Path cannot be an ancestor of the start up folder")
        {
            _appFolderInfo = appFolderInfo;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            return !_appFolderInfo.StartUpFolder.IsParentPath(context.PropertyValue.ToString());
        }
    }
}
