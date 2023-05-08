using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Synology
{
    public class SynologyIndexerSettingsValidator : AbstractValidator<SynologyIndexerSettings>
    {
    }

    public class SynologyIndexerSettings : IProviderConfig
    {
        private static readonly SynologyIndexerSettingsValidator Validator = new SynologyIndexerSettingsValidator();

        public SynologyIndexerSettings()
        {
            UpdateLibrary = true;
        }

        [FieldDefinition(0, Label = "Update Library", Type = FieldType.Checkbox, HelpText = "Call synoindex on localhost to update a library file")]
        public bool UpdateLibrary { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
