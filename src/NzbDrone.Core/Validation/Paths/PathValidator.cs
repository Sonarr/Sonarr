using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Validation.Paths
{
    public static class PathValidation
    {
        public static IRuleBuilderOptions<T, string> IsValidPath<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new PathValidator());
        }

        public static IRuleBuilderOptions<T, string> ContainsNoPathTraversal<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new PathTraversalValidator());
        }
    }

    public class PathTraversalValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => OsInfo.IsWindows
            ? "Path '{path}' cannot contain a folder named '.' or '..', or a folder name ending with '.'"
            : "Path '{path}' cannot contain a folder named '.' or '..'";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            return !context.PropertyValue.ToString().ContainsPathTraversal();
        }
    }

    public class PathValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Invalid Path: '{path}'";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            return context.PropertyValue.ToString().IsPathValid(PathValidationType.CurrentOs);
        }
    }
}
