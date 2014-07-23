using System;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

namespace NzbDrone.Core.Organizer
{
    public static class FileNameValidation
    {
        private static readonly Regex SeasonFolderRegex = new Regex(@"(\{season(\:\d+)?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        public static IRuleBuilderOptions<T, string> ValidAnimeEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new ValidAnimeEpisodeFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidSeriesFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.SeriesTitleRegex)).WithMessage("Must contain series title");
        }

        public static IRuleBuilderOptions<T, string> ValidSeasonFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new RegularExpressionValidator(SeasonFolderRegex)).WithMessage("Must contain season number");
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

    public class ValidAnimeEpisodeFormatValidator : PropertyValidator
    {
        public ValidAnimeEpisodeFormatValidator()
            : base("Must contain Absolute Episode number or Season and Episode")
        {

        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as String;

            if (!FileNameBuilder.SeasonEpisodePatternRegex.IsMatch(value) &&
                !FileNameBuilder.AbsoluteEpisodePatternRegex.IsMatch(value))
            {
                return false;
            }

            return true;
        }
    }
}
