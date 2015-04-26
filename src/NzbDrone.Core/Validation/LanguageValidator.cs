using FluentValidation.Validators;

namespace NzbDrone.Core.Validation
{
    public class LanguageValidator : PropertyValidator
    {
        public LanguageValidator()
            : base("Unknown Language")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return false;

            if ((int) context.PropertyValue == 0) return false;

            return true;
        }
    }
}
