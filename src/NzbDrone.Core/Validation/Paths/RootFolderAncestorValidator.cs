using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Validation.Paths
{
    public class RootFolderAncestorValidator : PropertyValidator
    {
        private readonly IRootFolderService _rootFolderService;

        public RootFolderAncestorValidator(IRootFolderService rootFolderService)
        {
            _rootFolderService = rootFolderService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is an ancestor of an existing root folder";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            return !_rootFolderService.All().Any(s => context.PropertyValue.ToString().IsParentPath(s.Path));
        }
    }
}
