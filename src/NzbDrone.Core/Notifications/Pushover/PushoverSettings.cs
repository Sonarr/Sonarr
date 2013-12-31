using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

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

        [FieldDefinition(0, Label = "API Key", HelpLink = "https://pushover.net/apps/clone/nzbdrone")]
        public String ApiKey { get; set; }

        [FieldDefinition(1, Label = "User Key", HelpLink = "https://pushover.net/")]
        public String UserKey { get; set; }

        [FieldDefinition(2, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(PushoverPriority) )]
        public Int32 Priority { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(UserKey) && Priority != null & Priority >= -1 && Priority <= 2;
            }
        }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
