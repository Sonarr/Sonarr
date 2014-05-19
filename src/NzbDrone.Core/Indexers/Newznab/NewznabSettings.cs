using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Common;
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
            RuleFor(c => c.Url).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty().When(ShouldHaveApiKey);
            RuleFor(c => c.Categories).NotEmpty();
            RuleFor(c => c.AdditionalParameters)
                .Matches(AdditionalParametersRegex)
                .When(c => !c.AdditionalParameters.IsNullOrWhiteSpace());
        }
    }

    public class NewznabSettings : IProviderConfig
    {
        private static readonly NewznabSettingsValidator Validator = new NewznabSettingsValidator();

        public NewznabSettings()
        {
            Categories = new[] { 5030, 5040 };
        }

        [FieldDefinition(0, Label = "URL")]
        public String Url { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public String ApiKey { get; set; }

        [FieldDefinition(2, Label = "Categories", HelpText = "Comma Separated list of categories", Advanced = true)]
        public IEnumerable<Int32> Categories { get; set; }

        [FieldDefinition(3, Label = "Additional Parameters", HelpText = "Additional newznab parameters", Advanced = true)]
        public String AdditionalParameters { get; set; }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}