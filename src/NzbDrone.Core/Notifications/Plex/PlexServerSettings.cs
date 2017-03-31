using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexServerSettingsValidator : AbstractValidator<PlexServerSettings>
    {
        public PlexServerSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.AuthToken).NotEmpty();
        }
    }

    public class PlexServerSettings : IProviderConfig
    {
        private static readonly PlexServerSettingsValidator Validator = new PlexServerSettingsValidator();

        public PlexServerSettings()
        {
            Port = 32400;
            UpdateLibrary = true;
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Username", Persisted = false)]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password, Persisted = false)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "AuthToken", Advanced = true)]
        public string AuthToken { get; set; }

        [FieldDefinition(5, Label = "Get Auth Token", Type = FieldType.Action, Persisted = false)]
        public string Authenticate { get; set; }

        [FieldDefinition(6, Label = "Update Library", Type = FieldType.Checkbox)]
        public bool UpdateLibrary { get; set; }

        [FieldDefinition(7, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Connect to Plex over HTTPS instead of HTTP")]
        public bool UseSsl { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Host);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
