using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.Pneumatic
{
    public class PneumaticSettingsValidator : AbstractValidator<PneumaticSettings>
    {
        public PneumaticSettingsValidator()
        {
            RuleFor(c => c.NzbFolder).IsValidPath();
            RuleFor(c => c.StrmFolder).IsValidPath();
        }
    }

    public class PneumaticSettings : IProviderConfig
    {
        private static readonly PneumaticSettingsValidator Validator = new PneumaticSettingsValidator();

        [FieldDefinition(0, Label = "DownloadClientPneumaticSettingsNzbFolder", Type = FieldType.Path, HelpText = "DownloadClientPneumaticSettingsNzbFolderHelpText")]
        public string NzbFolder { get; set; }

        [FieldDefinition(1, Label = "DownloadClientPneumaticSettingsStrmFolder", Type = FieldType.Path, HelpText = "DownloadClientPneumaticSettingsStrmFolderHelpText")]
        public string StrmFolder { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
