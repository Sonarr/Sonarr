using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
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
            return settings.BaseUrl != null && ApiKeyWhiteList.Any(c => settings.BaseUrl.ToLowerInvariant().Contains(c));
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
            MultiLanguages = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "IndexerSettingsApiPath", HelpText = "IndexerSettingsApiPathHelpText", Advanced = true)]
        [FieldToken(TokenField.HelpText, "IndexerSettingsApiPath", "url", "/api")]
        public string ApiPath { get; set; }

        [FieldDefinition(2, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(3, Label = "IndexerSettingsCategories", Type = FieldType.Select, SelectOptionsProviderAction = "newznabCategories", HelpText = "IndexerSettingsCategoriesHelpText")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(4, Label = "IndexerSettingsAnimeCategories", Type = FieldType.Select, SelectOptionsProviderAction = "newznabCategories", HelpText = "IndexerSettingsAnimeCategoriesHelpText")]
        public IEnumerable<int> AnimeCategories { get; set; }

        [FieldDefinition(5, Label = "IndexerSettingsAnimeStandardFormatSearch", Type = FieldType.Checkbox, HelpText = "IndexerSettingsAnimeStandardFormatSearchHelpText")]
        public bool AnimeStandardFormatSearch { get; set; }

        [FieldDefinition(6, Label = "IndexerSettingsAdditionalParameters", HelpText = "IndexerSettingsAdditionalNewznabParametersHelpText", Advanced = true)]
        public string AdditionalParameters { get; set; }

        [FieldDefinition(7, Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Label = "IndexerSettingsMultiLanguageRelease", HelpText = "IndexerSettingsMultiLanguageReleaseHelpText", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        // Field 8 is used by TorznabSettings MinimumSeeders
        // If you need to add another field here, update TorznabSettings as well and this comment

        public virtual NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
