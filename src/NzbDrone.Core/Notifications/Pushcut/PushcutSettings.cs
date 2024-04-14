using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Pushcut
{
    public class PushcutSettingsValidator : AbstractValidator<PushcutSettings>
    {
        public PushcutSettingsValidator()
        {
            RuleFor(settings => settings.ApiKey).NotEmpty();
            RuleFor(settings => settings.NotificationName).NotEmpty();
        }
    }

    public class PushcutSettings : NotificationSettingsBase<PushcutSettings>
    {
        private static readonly PushcutSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "NotificationsPushcutSettingsNotificationName", Type = FieldType.Textbox, HelpText = "NotificationsPushcutSettingsNotificationNameHelpText")]
        public string NotificationName { get; set; }

        [FieldDefinition(1, Label = "ApiKey", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey, HelpText = "NotificationsPushcutSettingsApiKeyHelpText")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "NotificationsPushcutSettingsTimeSensitive", Type = FieldType.Checkbox, HelpText = "NotificationsPushcutSettingsTimeSensitiveHelpText")]
        public bool TimeSensitive { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
