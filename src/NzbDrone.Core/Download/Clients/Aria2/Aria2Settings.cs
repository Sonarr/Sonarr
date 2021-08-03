using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Aria2
{
    public class Aria2SettingsValidator : AbstractValidator<Aria2Settings>
    {
        public Aria2SettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
        }
    }

    public class Aria2Settings : IProviderConfig
    {
        private static readonly Aria2SettingsValidator Validator = new Aria2SettingsValidator();

        public Aria2Settings()
        {
            Host = "localhost";
            Port = 6800;
            RpcPath = "/rpc";
            UseSsl = false;
            SecretToken = "MySecretToken";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Number)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "XML RPC Path", Type = FieldType.Textbox)]
        public string RpcPath { get; set; }

        [FieldDefinition(3, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        [FieldDefinition(4, Label = "Secret token", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string SecretToken { get; set; }

        public NzbDroneValidationResult Validate()
        {
                return new NzbDroneValidationResult(Validator.Validate(this));
            }
    }
}
