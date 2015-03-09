using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Fanzub
{
    public class FanzubSettingsValidator : AbstractValidator<FanzubSettings>
    {
        public FanzubSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class FanzubSettings : IProviderConfig
    {
        private static readonly FanzubSettingsValidator validator = new FanzubSettingsValidator();

        public FanzubSettings()
        {
            BaseUrl = "http://fanzub.com/rss/";
        }

        [FieldDefinition(0, Label = "Rss URL", HelpText = "Enter to URL to an Fanzub compatible RSS feed")]
        public String BaseUrl { get; set; }

        public ValidationResult Validate()
        {
            return validator.Validate(this);
        }
    }
}
