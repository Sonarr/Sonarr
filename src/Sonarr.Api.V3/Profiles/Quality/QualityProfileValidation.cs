using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;

namespace Sonarr.Api.V3.Profiles.Quality
{
    public static class QualityProfileValidation
    {
        public static IRuleBuilderOptions<T, IList<QualityProfileQualityItemResource>> MustHaveAllowedQuality<T>(this IRuleBuilder<T, IList<QualityProfileQualityItemResource>> ruleBuilder)
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
            var list = context.PropertyValue as IList<QualityProfileQualityItemResource>;

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
