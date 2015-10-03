using System;
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

        [FieldDefinition(3, Label = "Sound", Type = FieldType.Textbox, HelpText = "Notification sound, leave blank to use the default", HelpLink = "https://pushover.net/api#sounds")]
        public string Sound { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(UserKey) && Priority >= -1 && Priority <= 2;
            }
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
