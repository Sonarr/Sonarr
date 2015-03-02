using FluentValidation.Validators;

namespace NzbDrone.Core.Validation
{
    public class ISO639Validator : PropertyValidator
    {
        public ISO639Validator()
            : base("Unknown Language")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return false;

            string val = context.PropertyValue.ToString();
            try
            {
                System.Globalization.CultureInfo obj = System.Globalization.CultureInfo.GetCultureInfo(val);
            } catch (System.Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
