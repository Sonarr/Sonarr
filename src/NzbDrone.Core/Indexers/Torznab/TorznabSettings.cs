using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class TorznabSettingsValidator : AbstractValidator<TorznabSettings>
    {
        private static readonly string[] ApiKeyWhiteList =
        {
            "hdaccess.net",
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
            RuleFor(c => c.Url).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty().When(ShouldHaveApiKey);
            RuleFor(c => c.Categories).NotEmpty().When(c => !c.AnimeCategories.Any());
            RuleFor(c => c.AnimeCategories).NotEmpty().When(c => !c.Categories.Any());
            RuleFor(c => c.AdditionalParameters)
                .Matches(AdditionalParametersRegex)
                .When(c => !c.AdditionalParameters.IsNullOrWhiteSpace());
        }
    }

    public class TorznabSettings : IProviderConfig
    {
        private static readonly TorznabSettingsValidator Validator = new TorznabSettingsValidator();

        public TorznabSettings()
        {
            Categories = new[] { 5030, 5040 };
            AnimeCategories = Enumerable.Empty<int>();
            EnableRageIDLookup = true;
        }

        [FieldDefinition(0, Label = "URL")]
        public string Url { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "Categories", HelpText = "Comma Separated list, leave blank to disable standard/daily shows", Advanced = true)]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(3, Label = "Anime Categories", HelpText = "Comma Separated list, leave blank to disable anime", Advanced = true)]
        public IEnumerable<int> AnimeCategories { get; set; }

        [FieldDefinition(4, Label = "Additional Parameters", HelpText = "Additional Torznab parameters", Advanced = true)]
        public string AdditionalParameters { get; set; }

        // TODO: To be removed in the next version.
        [FieldDefinition(5, Type = FieldType.Checkbox, Label = "Enable RageID Lookup", HelpText = "Disable this if your tracker doesn't have tvrage ids, Sonarr will then use (more expensive) title queries.", Advanced = true)]
        public bool EnableRageIDLookup { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}