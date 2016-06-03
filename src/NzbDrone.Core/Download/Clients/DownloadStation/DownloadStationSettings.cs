using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationSettingsValidator : AbstractValidator<DownloadStationSettings>
    {
        public DownloadStationSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(0, 65535);
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Directory).IsValidPath().When(c => !string.IsNullOrEmpty(c.Directory));
        }
    }

    public class DownloadStationSettings : IProviderConfig
    {
        private static readonly DownloadStationSettingsValidator Validator = new DownloadStationSettingsValidator();

        public DownloadStationSettings()
        {
            Host = "localhost";
            Port = 5000;
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Username", Type = FieldType.Textbox)]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "Directory", Type = FieldType.Path, Advanced = true, HelpText = "Optional location to put downloads in, leave blank to use the default Download Station location")]
        public string Directory { get; set; }
    }
}
