using FluentValidation;
using Workarr.Annotations;
using Workarr.Validation;

namespace Workarr.Notifications.Join
{
    public class JoinSettingsValidator : AbstractValidator<JoinSettings>
    {
        public JoinSettingsValidator()
        {
            RuleFor(s => s.ApiKey).NotEmpty();
            RuleFor(s => s.DeviceIds).Empty().WithMessage("Use Device Names instead");
        }
    }

    public class JoinSettings : NotificationSettingsBase<JoinSettings>
    {
        private static readonly JoinSettingsValidator Validator = new ();

        public JoinSettings()
        {
            Priority = (int)JoinPriority.Normal;
        }

        [FieldDefinition(0, Label = "ApiKey", HelpText = "NotificationsJoinSettingsApiKeyHelpText", HelpLink = "https://joinjoaomgcd.appspot.com/")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "NotificationsJoinSettingsDeviceIds", HelpText = "NotificationsJoinSettingsDeviceIdsHelpText", Hidden = HiddenType.HiddenIfNotSet)]
        public string DeviceIds { get; set; }

        [FieldDefinition(2, Label = "NotificationsJoinSettingsDeviceNames", HelpText = "NotificationsJoinSettingsDeviceNamesHelpText", HelpLink = "https://joaoapps.com/join/api/")]
        public string DeviceNames { get; set; }

        [FieldDefinition(3, Label = "NotificationsJoinSettingsNotificationPriority", Type = FieldType.Select, SelectOptions = typeof(JoinPriority))]
        public int Priority { get; set; }

        public override WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
