using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Signal
{
    public class SignalSettingsValidator : AbstractValidator<SignalSettings>
    {
        public SignalSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty();
            RuleFor(c => c.Port).NotEmpty();
            RuleFor(c => c.SenderNumber).NotEmpty();
            RuleFor(c => c.ReceiverId).NotEmpty();
        }
    }

    public class SignalSettings : NotificationSettingsBase<SignalSettings>
    {
        private static readonly SignalSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox, Placeholder = "localhost")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox, Placeholder = "8080")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "NotificationsSettingsUseSslHelpText")]
        [FieldToken(TokenField.HelpText, "UseSsl", "serviceName",  "Signal")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "NotificationsSignalSettingsSenderNumber", Privacy = PrivacyLevel.ApiKey, HelpText = "NotificationsSignalSettingsSenderNumberHelpText")]
        public string SenderNumber { get; set; }

        [FieldDefinition(4, Label = "NotificationsSignalSettingsGroupIdPhoneNumber", HelpText = "NotificationsSignalSettingsGroupIdPhoneNumberHelpText")]
        public string ReceiverId { get; set; }

        [FieldDefinition(5, Label = "Username", Privacy = PrivacyLevel.UserName, HelpText = "NotificationsSignalSettingsUsernameHelpText")]
        public string AuthUsername { get; set; }

        [FieldDefinition(6, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password, HelpText = "NotificationsSignalSettingsPasswordHelpText")]
        public string AuthPassword { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
