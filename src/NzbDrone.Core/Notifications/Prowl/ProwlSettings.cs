using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class ProwlSettingsValidator : AbstractValidator<ProwlSettings>
    {
        public ProwlSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class ProwlSettings : IProviderConfig
    {
        private static readonly ProwlSettingsValidator Validator = new ProwlSettingsValidator();

        [FieldDefinition(0, Label = "API Key", HelpLink = "https://www.prowlapp.com/api_settings.php")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "Priority", Type = FieldType.Select, SelectOptions= typeof(ProwlPriority) )]
        public int Priority { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(ApiKey) && Priority >= -2 && Priority <= 2;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
