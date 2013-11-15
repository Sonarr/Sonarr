using System;
using FluentValidation;
using FluentValidation.Validators;

namespace NzbDrone.Core.Organizer
{
    public static class FileNameValidation
    {
        public static IRuleBuilderOptions<T, string> ValidEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.SeasonEpisodePatternRegex)).WithMessage("Must contain season and episode numbers");
        }

        public static IRuleBuilderOptions<T, string> ValidDailyEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new ValidDailyEpisodeFormatValidator());
        }
    }

    public class ValidDailyEpisodeFormatValidator : PropertyValidator
    {
        public ValidDailyEpisodeFormatValidator()
            : base("Must contain Air Date or Season and Episode")
        {

        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as String;

            if (!FileNameBuilder.SeasonEpisodePatternRegex.IsMatch(value) &&
                !FileNameBuilder.AirDateRegex.IsMatch(value))
            {
                return false;
            }

            return true;
        }
    }
}
