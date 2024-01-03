using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Join
{
    public class JoinSettingsValidator : AbstractValidator<JoinSettings>
    {
        public JoinSettingsValidator()
        {
            RuleFor(s => s.ApiKey).NotEmpty();
            RuleFor(s => s.DeviceIds).Empty().WithMessage("Use Device Names instead");
        }
    }

    public class JoinSettings : IProviderConfig
    {
        public JoinSettings()
        {
            Priority = (int)JoinPriority.Normal;
        }

        private static readonly JoinSettingsValidator Validator = new JoinSettingsValidator();

        [FieldDefinition(0, Label = "ApiKey", HelpText = "NotificationsJoinSettingsApiKeyHelpText", HelpLink = "https://joinjoaomgcd.appspot.com/")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "NotificationsJoinSettingsDeviceIds", HelpText = "NotificationsJoinSettingsDeviceIdsHelpText", Hidden = HiddenType.HiddenIfNotSet)]
        public string DeviceIds { get; set; }

        [FieldDefinition(2, Label = "NotificationsJoinSettingsDeviceNames", HelpText = "NotificationsJoinSettingsDeviceNamesHelpText", HelpLink = "https://joaoapps.com/join/api/")]
        public string DeviceNames { get; set; }

        [FieldDefinition(3, Label = "NotificationsJoinSettingsNotificationPriority", Type = FieldType.Select, SelectOptions = typeof(JoinPriority))]
        public int Priority { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
