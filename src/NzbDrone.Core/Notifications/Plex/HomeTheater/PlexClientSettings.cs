using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using System;

namespace NzbDrone.Core.Notifications.Plex.HomeTheater
{
    public class PlexClientSettingsValidator : AbstractValidator<PlexClientSettings>
    {
        public PlexClientSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.UrlBase).ActuallyValidUrlBase();
        }
    }

    public class PlexClientSettings : IProviderConfig
    {
        private static readonly PlexClientSettingsValidator Validator = new PlexClientSettingsValidator();

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; } = 3000;

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Connect to Plex over HTTPS instead of HTTP")]
        public bool UseSsl { get; set; } = false;

        [FieldDefinition(3, Label = "Url Base", Type = FieldType.Textbox, Advanced = true, HelpText = "Adds a prefix to the Plex url, see http://[host]:[port]/[urlBase]")]
        public string UrlBase { get; set; } = "";

        [FieldDefinition(4, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(5, Label = "Password", Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [JsonIgnore]
        public string Address => $"{(UseSsl ? "https" : "http")}://{Host}:{Port}{(String.IsNullOrEmpty(UrlBase) ? "" : $"/{UrlBase}")}";

        public bool IsValid => !string.IsNullOrWhiteSpace(Host);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
