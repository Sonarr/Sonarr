using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Organizer
{
    public static class FileNameValidation
    {
        private static readonly Regex SeasonFolderRegex = new Regex(@"(\{season(\:\d+)?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static readonly Regex OriginalTokenRegex = new Regex(@"(\{original[- ._](?:title|filename)\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IRuleBuilderOptions<T, string> ValidEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new ValidStandardEpisodeFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidDailyEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new ValidDailyEpisodeFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidAnimeEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new ValidAnimeEpisodeFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidSeriesFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.SeriesTitleRegex)).WithMessage("Must contain series title");
        }

        public static IRuleBuilderOptions<T, string> ValidSeasonFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(SeasonFolderRegex)).WithMessage("Must contain season number");
        }

        public static IRuleBuilderOptions<T, string> ValidSpecialsFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));

            return ruleBuilder.SetValidator(new IllegalCharactersValidator());
        }
    }

    public class ValidStandardEpisodeFormatValidator : PropertyValidator
    {
        public ValidStandardEpisodeFormatValidator()
            : base("Must contain season and episode numbers OR Original Title")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;

            if (!FileNameBuilder.SeasonEpisodePatternRegex.IsMatch(value) &&
                !FileNameValidation.OriginalTokenRegex.IsMatch(value))
            {
                return false;
            }

            return true;
        }
    }

    public class ValidDailyEpisodeFormatValidator : PropertyValidator
    {
        public ValidDailyEpisodeFormatValidator()
            : base("Must contain Air Date OR Season and Episode OR Original Title")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;

            if (!FileNameBuilder.SeasonEpisodePatternRegex.IsMatch(value) &&
                !FileNameBuilder.AirDateRegex.IsMatch(value) &&
                !FileNameValidation.OriginalTokenRegex.IsMatch(value))
            {
                return false;
            }

            return true;
        }
    }

    public class ValidAnimeEpisodeFormatValidator : PropertyValidator
    {
        public ValidAnimeEpisodeFormatValidator()
            : base("Must contain Absolute Episode number OR Season and Episode OR Original Title")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;

            if (!FileNameBuilder.SeasonEpisodePatternRegex.IsMatch(value) &&
                !FileNameBuilder.AbsoluteEpisodePatternRegex.IsMatch(value) &&
                !FileNameValidation.OriginalTokenRegex.IsMatch(value))
            {
                return false;
            }

            return true;
        }
    }

    public class IllegalCharactersValidator : PropertyValidator
    {
        private readonly char[] _invalidPathChars = Path.GetInvalidPathChars();

        public IllegalCharactersValidator()
            : base("Contains illegal characters: {InvalidCharacters}")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;
            var invalidCharacters = new List<char>();

            if (value.IsNullOrWhiteSpace())
            {
                return true;
            }

            foreach (var i in _invalidPathChars)
            {
                if (value.IndexOf(i) >= 0)
                {
                    invalidCharacters.Add(i);
                }
            }

            if (invalidCharacters.Any())
            {
                context.MessageFormatter.AppendArgument("InvalidCharacters", string.Join("", invalidCharacters));
                return false;
            }

            return true;
        }
    }
}
