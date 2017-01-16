using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class TorznabSettingsValidator : AbstractValidator<TorznabSettings>
    {
        private static readonly string[] ApiKeyWhiteList =
        {
            "hd4free.xyz",
        };

        private static bool ShouldHaveApiKey(TorznabSettings settings)
        {
            if (settings.Url == null)
            {
                return false;
            }

            return ApiKeyWhiteList.Any(c => settings.Url.ToLowerInvariant().Contains(c));
        }

        private static readonly Regex AdditionalParametersRegex = new Regex(@"(&.+?\=.+?)+", RegexOptions.Compiled);

        public TorznabSettingsValidator()
        {
            Custom(newznab =>
            {
                if (newznab.Categories.Empty() && newznab.AnimeCategories.Empty())
                {
                    return new ValidationFailure("", "Either 'Categories' or 'Anime Categories' must be provided");
                }

                return null;
            });

            RuleFor(c => c.Url).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty().When(ShouldHaveApiKey);
            RuleFor(c => c.AdditionalParameters).Matches(AdditionalParametersRegex)
                                                .When(c => !c.AdditionalParameters.IsNullOrWhiteSpace());
        }
    }

    public class TorznabSettings : NewznabSettings
    {
        private static readonly TorznabSettingsValidator Validator = new TorznabSettingsValidator();

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}