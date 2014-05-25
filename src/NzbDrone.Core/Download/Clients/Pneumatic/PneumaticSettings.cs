using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.Pneumatic
{
    public class PneumaticSettingsValidator : AbstractValidator<PneumaticSettings>
    {
        public PneumaticSettingsValidator()
        {
            //Todo: Validate that the path actually exists
            RuleFor(c => c.NzbFolder).IsValidPath();
        }
    }

    public class PneumaticSettings : IProviderConfig
    {
        private static readonly PneumaticSettingsValidator Validator = new PneumaticSettingsValidator();

        [FieldDefinition(0, Label = "Nzb Folder", Type = FieldType.Path)]
        public String NzbFolder { get; set; }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
