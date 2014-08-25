using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;

namespace NzbDrone.Api.Profiles
{
    public static class ProfileValidation
    {
        public static IRuleBuilderOptions<T, IList<ProfileQualityItemResource>> MustHaveAllowedQuality<T>(this IRuleBuilder<T, IList<ProfileQualityItemResource>> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));

            return ruleBuilder.SetValidator(new AllowedValidator<T>());
        }
    }

    public class AllowedValidator<T> : PropertyValidator
    {
        public AllowedValidator()
            : base("Must contain at least one allowed quality")
        {

        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var list = context.PropertyValue as IList<ProfileQualityItemResource>;

            if (list == null)
            {
                return false;
            }

            if (!list.Any(c => c.Allowed))
            {
                return false;
            }

            return true;
        }
    }
}
