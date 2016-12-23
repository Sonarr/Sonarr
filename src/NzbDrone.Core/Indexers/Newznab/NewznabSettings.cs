using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabSettingsValidator : AbstractValidator<NewznabSettings>
    {
        private static readonly string[] ApiKeyWhiteList =
        {
            "nzbs.org",
            "nzb.su",
            "dognzb.cr",
            "nzbplanet.net",
            "nzbid.org",
            "nzbndx.com",
            "nzbindex.in"
        };

        private static bool ShouldHaveApiKey(NewznabSettings settings)
        {
            if (settings.Url == null)
            {
                return false;
            }

            return ApiKeyWhiteList.Any(c => settings.Url.ToLowerInvariant().Contains(c));
        }

        private static readonly Regex AdditionalParametersRegex = new Regex(@"(&.+?\=.+?)+", RegexOptions.Compiled);

        public NewznabSettingsValidator()
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

    public class NewznabSettings : IProviderConfig
    {
        private static readonly NewznabSettingsValidator Validator = new NewznabSettingsValidator();

        public NewznabSettings()
        {
            Categories = new[] { 5030, 5040 };
            AnimeCategories = Enumerable.Empty<int>();
        }

        [FieldDefinition(0, Label = "URL")]
        public string Url { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "Categories", HelpText = "Comma Separated list, leave blank to disable standard/daily shows", Advanced = true)]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(3, Label = "Anime Categories", HelpText = "Comma Separated list, leave blank to disable anime", Advanced = true)]
        public IEnumerable<int> AnimeCategories { get; set; }

        [FieldDefinition(4, Label = "Additional Parameters", HelpText = "Additional Newznab parameters", Advanced = true)]
        public string AdditionalParameters { get; set; }

        public virtual NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}