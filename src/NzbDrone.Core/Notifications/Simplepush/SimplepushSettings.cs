using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Simplepush
{
    public class SimplepushSettingsValidator : AbstractValidator<SimplepushSettings>
    {
        public SimplepushSettingsValidator()
        {
            RuleFor(c => c.Key).NotEmpty();
        }
    }

    public class SimplepushSettings : IProviderConfig
    {
        private static readonly SimplepushSettingsValidator Validator = new SimplepushSettingsValidator();

        [FieldDefinition(0, Label = "Key", Privacy = PrivacyLevel.ApiKey, HelpLink = "https://simplepush.io/features")]
        public string Key { get; set; }

        [FieldDefinition(1, Label = "Event", HelpText = "Customize the behavior of push notifications", HelpLink = "https://simplepush.io/features")]
        public string Event { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Key);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
