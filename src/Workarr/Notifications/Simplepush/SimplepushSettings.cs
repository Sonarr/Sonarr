using FluentValidation;
using Workarr.Annotations;
using Workarr.Validation;

namespace Workarr.Notifications.Simplepush
{
    public class SimplepushSettingsValidator : AbstractValidator<SimplepushSettings>
    {
        public SimplepushSettingsValidator()
        {
            RuleFor(c => c.Key).NotEmpty();
        }
    }

    public class SimplepushSettings : NotificationSettingsBase<SimplepushSettings>
    {
        private static readonly SimplepushSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "NotificationsSimplepushSettingsKey", Privacy = PrivacyLevel.ApiKey, HelpLink = "https://simplepush.io/features")]
        public string Key { get; set; }

        [FieldDefinition(1, Label = "NotificationsSimplepushSettingsEvent", HelpText = "NotificationsSimplepushSettingsEventHelpText", HelpLink = "https://simplepush.io/features")]
        public string Event { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Key);

        public override WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
