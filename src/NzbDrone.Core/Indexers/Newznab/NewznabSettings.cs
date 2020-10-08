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
            if (settings.BaseUrl == null)
            {
                return false;
            }

            return ApiKeyWhiteList.Any(c => settings.BaseUrl.ToLowerInvariant().Contains(c));
        }

        private static readonly Regex AdditionalParametersRegex = new Regex(@"(&.+?\=.+?)+", RegexOptions.Compiled);

        public NewznabSettingsValidator()
        {
            RuleFor(c => c).Custom((c, context) =>
            {
                if (c.Categories.Empty() && c.AnimeCategories.Empty())
                {
                    context.AddFailure("Either 'Categories' or 'Anime Categories' must be provided");
                }
            });

            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiPath).ValidUrlBase("/api");
            RuleFor(c => c.ApiKey).NotEmpty().When(ShouldHaveApiKey);
            RuleFor(c => c.AdditionalParameters).Matches(AdditionalParametersRegex)
                                                .When(c => !c.AdditionalParameters.IsNullOrWhiteSpace());
        }
    }

    public class NewznabSettings : IIndexerSettings
    {
        private static readonly NewznabSettingsValidator Validator = new NewznabSettingsValidator();

        public NewznabSettings()
        {
            ApiPath = "/api";
            Categories = new[] { 5030, 5040 };
            AnimeCategories = Enumerable.Empty<int>();
        }

        [FieldDefinition(0, Label = "URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Path", HelpText = "Path to the api, usually /api", Advanced = true)]
        public string ApiPath { get; set; }

        [FieldDefinition(2, Label = "API Key", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(3, Label = "Categories", Type = FieldType.Select, SelectOptionsProviderAction = "newznabCategories", HelpText = "Comma Separated list, leave blank to disable standard/daily shows")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(4, Label = "Anime Categories", Type = FieldType.Select, SelectOptionsProviderAction = "newznabCategories", HelpText = "Comma Separated list, leave blank to disable anime")]
        public IEnumerable<int> AnimeCategories { get; set; }

        [FieldDefinition(5, Label = "Additional Parameters", HelpText = "Additional Newznab parameters", Advanced = true)]
        public string AdditionalParameters { get; set; }

        // Field 6 is used by TorznabSettings MinimumSeeders
        // If you need to add another field here, update TorznabSettings as well and this comment

        public virtual NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
