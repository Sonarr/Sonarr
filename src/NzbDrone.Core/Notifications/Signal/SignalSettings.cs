using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
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

    public class SignalSettings : IProviderConfig
    {
        private static readonly SignalSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox, HelpText = "localhost")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox, HelpText = "8080")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Use a secure connection.")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Sender Number", Privacy = PrivacyLevel.ApiKey, HelpText = "Phone number of the sender register in signal-api")]
        public string SenderNumber { get; set; }

        [FieldDefinition(4, Label = "Group ID / PhoneNumber", HelpText = "GroupID / PhoneNumber of the receiver")]
        public string ReceiverId { get; set; }

        [FieldDefinition(5, Label = "Login", Privacy = PrivacyLevel.UserName, HelpText = "Username used to authenticate requests toward signal-api")]
        public string AuthUsername { get; set; }

        [FieldDefinition(6, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password, HelpText = "Password used to authenticate requests toward signal-api")]
        public string AuthPassword { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
