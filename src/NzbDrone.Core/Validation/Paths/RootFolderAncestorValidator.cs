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
            : base("Path is an ancestor of an existing root folder")
        {
            _rootFolderService = rootFolderService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            return !_rootFolderService.All().Any(s => context.PropertyValue.ToString().IsParentPath(s.Path));
        }
    }
}
