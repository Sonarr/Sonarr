using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.AirDCPP
{
    public class AirDCPPClientSettingsValidator : AbstractValidator<AirDCPPClientSettings>
    {
        public AirDCPPClientSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.UrlBase).ValidUrlBase().When(c => c.UrlBase.IsNotNullOrWhiteSpace());
        }
    }

    public class AirDCPPClientSettings : IProviderConfig
    {
        private static readonly AirDCPPClientSettingsValidator Validator = new AirDCPPClientSettingsValidator();

        public AirDCPPClientSettings()
        {
            Host = "localhost";
            Port = 5600;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Use a secure connection.")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Url Base", Type = FieldType.Textbox, Advanced = true, HelpText = "Adds a prefix to the AirDC++ url, e.g. http://[host]:[port]/[urlBase]/api/v1")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "Username")]
        public string Username { get; set; }

        [FieldDefinition(5, Type = FieldType.Password, Label = "Password")]
        public string Password { get; set; }

        [FieldDefinition(6, Label = "Default download directory")]
        public string DownloadDirectory { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
