using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Growl
{
    public class GrowlSettingsValidator : AbstractValidator<GrowlSettings>
    {
        public GrowlSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
        }
    }

    public class GrowlSettings : IProviderConfig
    {
        private static readonly GrowlSettingsValidator Validator = new GrowlSettingsValidator();

        public GrowlSettings()
        {
            Port = 23053;
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Password")]
        public string Password { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Password) && Port > 0;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
