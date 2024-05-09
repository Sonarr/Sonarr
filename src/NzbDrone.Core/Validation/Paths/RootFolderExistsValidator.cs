using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Validation.Paths
{
    public class RootFolderExistsValidator : PropertyValidator
    {
        private readonly IRootFolderService _rootFolderService;

        public RootFolderExistsValidator(IRootFolderService rootFolderService)
        {
            _rootFolderService = rootFolderService;
        }

        protected override string GetDefaultMessageTemplate() => "Root folder '{path}' does not exist";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            context.MessageFormatter.AppendArgument("path", context.PropertyValue?.ToString());

            return context.PropertyValue == null || _rootFolderService.All().Exists(r => r.Path.PathEquals(context.PropertyValue.ToString()));
        }
    }
}
