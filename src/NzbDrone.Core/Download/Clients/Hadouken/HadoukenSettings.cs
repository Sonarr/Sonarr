using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Hadouken
{
    public sealed class HadoukenSettings : IProviderConfig
    {
        private static readonly HadoukenSettingsValidator Validator = new HadoukenSettingsValidator();

        public HadoukenSettings()
        {
            Host = "localhost";
            Port = 7070;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Auth. type", Type = FieldType.Select, SelectOptions = typeof(AuthenticationType), HelpText = "How to authenticate against Hadouken.")]
        public int AuthenticationType { get; set; }

        [FieldDefinition(3, Label = "Username", Type = FieldType.Textbox, HelpText = "Only used for basic auth.")]
        public string Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password, HelpText = "Only used for basic auth.")]
        public string Password { get; set; }

        [FieldDefinition(5, Label = "Token", Type = FieldType.Password, HelpText = "Only used for token auth.")]
        public string Token { get; set; }

        [FieldDefinition(6, Label = "Use SSL", Type = FieldType.Checkbox, Advanced = true)]
        public bool UseSsl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
