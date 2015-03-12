using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
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
            AnimeCategories = Enumerable.Empty<Int32>();
        }

        [FieldDefinition(0, Label = "URL")]
        public String Url { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public String ApiKey { get; set; }

        [FieldDefinition(2, Label = "Categories", HelpText = "Comma Separated list, leave blank to disable standard/daily shows", Advanced = true)]
        public IEnumerable<Int32> Categories { get; set; }

        [FieldDefinition(3, Label = "Anime Categories", HelpText = "Comma Separated list, leave blank to disable anime", Advanced = true)]
        public IEnumerable<Int32> AnimeCategories { get; set; }

        [FieldDefinition(4, Label = "Additional Parameters", HelpText = "Additional Torznab parameters", Advanced = true)]
        public String AdditionalParameters { get; set; }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}