using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Notifications.CustomScript
{
    public class CustomScriptSettingsValidator : AbstractValidator<CustomScriptSettings>
    {
        public CustomScriptSettingsValidator()
        {
            RuleFor(c => c.Path).IsValidPath();
        }
    }

    public class CustomScriptSettings : IProviderConfig
    {
        private static readonly CustomScriptSettingsValidator Validator = new CustomScriptSettingsValidator();

        [FieldDefinition(0, Label = "Path", Type = FieldType.FilePath)]
        public string Path { get; set; }

        [FieldDefinition(1, Label = "Arguments", HelpText = "Arguments to pass to the script")]
        public string Arguments { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
