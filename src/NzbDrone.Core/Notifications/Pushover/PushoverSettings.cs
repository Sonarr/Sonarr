using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Pushover
{
    public class PushoverSettingsValidator : AbstractValidator<PushoverSettings>
    {
        public PushoverSettingsValidator()
        {
            RuleFor(c => c.UserKey).NotEmpty();
            RuleFor(c => c.Retry).GreaterThanOrEqualTo(30).LessThanOrEqualTo(86400).When(c => (PushoverPriority)c.Priority == PushoverPriority.Emergency);
            RuleFor(c => c.Retry).GreaterThanOrEqualTo(0).LessThanOrEqualTo(86400).When(c => (PushoverPriority)c.Priority == PushoverPriority.Emergency);
        }
    }

    public class PushoverSettings : IProviderConfig
    {
        private static readonly PushoverSettingsValidator Validator = new PushoverSettingsValidator();

        public PushoverSettings()
        {
            Priority = 0;
        }

        //TODO: Get Pushover to change our app name (or create a new app) when we have a new logo
        [FieldDefinition(0, Label = "API Key", HelpLink = "https://pushover.net/apps/clone/nzbdrone")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "User Key", HelpLink = "https://pushover.net/")]
        public string UserKey { get; set; }

        [FieldDefinition(2, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(PushoverPriority) )]
        public int Priority { get; set; }

        [FieldDefinition(3, Label = "Retry", Type = FieldType.Textbox, HelpText = "Interval to retry Emergency alerts, minimum 30 seconds")]
        public int Retry { get; set; }

        [FieldDefinition(4, Label = "Expire", Type = FieldType.Textbox, HelpText = "Maximum time to retry Emergency alerts, maximum 86400 seconds")]
        public int Expire { get; set; }

        [FieldDefinition(5, Label = "Sound", Type = FieldType.Textbox, HelpText = "Notification sound, leave blank to use the default", HelpLink = "https://pushover.net/api#sounds")]
        public string Sound { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(UserKey) && Priority >= -1 && Priority <= 2;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
