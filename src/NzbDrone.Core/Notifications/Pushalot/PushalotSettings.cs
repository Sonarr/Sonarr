using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Pushalot
{
    public class PushalotSettingsValidator : AbstractValidator<PushalotSettings>
    {
        public PushalotSettingsValidator()
        {
            RuleFor(c => c.AuthToken).NotEmpty();
        }
    }

    public class PushalotSettings : IProviderConfig
    {
        public PushalotSettings()
        {
            Image = true;
        }

        private static readonly PushalotSettingsValidator Validator = new PushalotSettingsValidator();

        [FieldDefinition(0, Label = "Authorization Token", HelpLink = "https://pushalot.com/manager/authorizations")]
        public string AuthToken { get; set; }

        [FieldDefinition(1, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(PushalotPriority))]
        public int Priority { get; set; }

        [FieldDefinition(2, Label = "Image", Type = FieldType.Checkbox, HelpText = "Include Sonarr logo with notifications")]
        public bool Image { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(AuthToken);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
