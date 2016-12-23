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
            RuleFor(s => s.DeviceIds).Matches(@"\A\S+\z").When(s => !string.IsNullOrEmpty(s.DeviceIds));
        }
    }

    public class JoinSettings : IProviderConfig
    {
        private static readonly JoinSettingsValidator Validator = new JoinSettingsValidator();

        [FieldDefinition(0, Label = "API Key", HelpText = "The API Key from your Join account settings (click Join API button).", HelpLink = "https://joinjoaomgcd.appspot.com/")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "Device IDs", HelpText = "Comma separated list of Device IDs you'd like to send notifications to. If unset, all devices will receive notifications.", HelpLink = "https://joinjoaomgcd.appspot.com/")]
        public string DeviceIds { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
