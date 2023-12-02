using System;
using System.Collections.Generic;
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
            Devices = Array.Empty<string>();
        }

        // TODO: Get Pushover to change our app name (or create a new app) when we have a new logo
        [FieldDefinition(0, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey, HelpLink = "https://pushover.net/apps/clone/sonarr")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "NotificationsPushoverSettingsUserKey", Privacy = PrivacyLevel.UserName, HelpLink = "https://pushover.net/")]
        public string UserKey { get; set; }

        [FieldDefinition(2, Label = "NotificationsPushoverSettingsDevices", HelpText = "NotificationsPushoverSettingsDevicesHelpText", Type = FieldType.Tag)]
        public IEnumerable<string> Devices { get; set; }

        [FieldDefinition(3, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(PushoverPriority))]
        public int Priority { get; set; }

        [FieldDefinition(4, Label = "NotificationsPushoverSettingsRetry", Type = FieldType.Textbox, HelpText = "NotificationsPushoverSettingsRetryHelpText")]
        public int Retry { get; set; }

        [FieldDefinition(5, Label = "NotificationsPushoverSettingsExpire", Type = FieldType.Textbox, HelpText = "NotificationsPushoverSettingsExpireHelpText")]
        public int Expire { get; set; }

        [FieldDefinition(6, Label = "NotificationsPushoverSettingsSound", Type = FieldType.Textbox, HelpText = "NotificationsPushoverSettingsSoundHelpText", HelpLink = "https://pushover.net/api#sounds")]
        public string Sound { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(UserKey) && Priority >= -1 && Priority <= 2;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
