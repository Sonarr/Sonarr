using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;

namespace Sonarr.Api.V3.Profiles.Language
{
    public static class LanguageValidation
    {
        public static IRuleBuilderOptions<T, IList<ProfileLanguageItemResource>> MustHaveAllowedLanguage<T>(this IRuleBuilder<T, IList<ProfileLanguageItemResource>> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));

            return ruleBuilder.SetValidator(new LanguageValidator<T>());
        }
    }


    public class LanguageValidator<T> : PropertyValidator
    {
        public LanguageValidator()
            : base("Must have at least one allowed language")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var list = context.PropertyValue as IList<ProfileLanguageItemResource>;

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
