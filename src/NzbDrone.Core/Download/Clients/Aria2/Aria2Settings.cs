using FluentValidation;
using NzbDrone.Core.Annotations;
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

    public class Aria2Settings : DownloadClientSettingsBase<Aria2Settings>
    {
        private static readonly Aria2SettingsValidator Validator = new ();

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

        [FieldDefinition(2, Label = "XmlRpcPath", Type = FieldType.Textbox)]
        public string RpcPath { get; set; }

        [FieldDefinition(3, Label = "UseSsl", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        [FieldDefinition(4, Label = "SecretToken", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string SecretToken { get; set; }

        [FieldDefinition(5, Label = "Directory", Type = FieldType.Textbox, HelpText = "DownloadClientAriaSettingsDirectoryHelpText")]
        public string Directory { get; set; }

        public override NzbDroneValidationResult Validate()
        {
                return new NzbDroneValidationResult(Validator.Validate(this));
            }
    }
}
