using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
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

        [FieldDefinition(0, Label = "Nzb Folder", Type = FieldType.Path, HelpText = "This folder will need to be reachable from XBMC")]
        public String NzbFolder { get; set; }

        [FieldDefinition(1, Label = "Strm Folder", Type = FieldType.Path, HelpText = ".strm files in this folder will be import by drone")]
        public String StrmFolder { get; set; }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
