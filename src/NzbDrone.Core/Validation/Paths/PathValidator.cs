using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Validation.Paths
{
    public static class PathValidation
    {
        public static IRuleBuilderOptions<T, string> IsValidPath<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new PathValidator());
        }
    }

    public class PathValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Invalid Path";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            return context.PropertyValue.ToString().IsPathValid(PathValidationType.CurrentOs);
        }
    }
}
