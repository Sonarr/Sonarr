using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexClientSettingsValidator : AbstractValidator<PlexClientSettings>
    {
        public PlexClientSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty();
            RuleFor(c => c.Port).GreaterThan(0);
        }
    }

    public class PlexClientSettings : IProviderConfig
    {
        private static readonly PlexClientSettingsValidator Validator = new PlexClientSettingsValidator();

        public PlexClientSettings()
        {
            Port = 3000;
        }

        [FieldDefinition(0, Label = "Host")]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Username")]
        public String Username { get; set; }

        [FieldDefinition(3, Label = "Password")]
        public String Password { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Host);
            }
        }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
