using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsSettingsValidator : AbstractValidator<OmgwtfnzbsSettings>
    {
        public OmgwtfnzbsSettingsValidator()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.Delay).GreaterThanOrEqualTo(0);
        }
    }

    public class OmgwtfnzbsSettings : IIndexerSettings
    {
        private static readonly OmgwtfnzbsSettingsValidator Validator = new OmgwtfnzbsSettingsValidator();

        public OmgwtfnzbsSettings()
        {
            Delay = 30;
        }

        // Unused since Omg has a hardcoded url.
        public string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(1, Label = "API Key", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "Delay", HelpText = "Time in minutes to delay new nzbs before they appear on the RSS feed", Advanced = true)]
        public int Delay { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
