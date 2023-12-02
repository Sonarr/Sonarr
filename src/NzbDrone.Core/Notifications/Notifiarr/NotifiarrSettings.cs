using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Notifiarr
{
    public class NotifiarrSettingsValidator : AbstractValidator<NotifiarrSettings>
    {
        public NotifiarrSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class NotifiarrSettings : IProviderConfig
    {
        private static readonly NotifiarrSettingsValidator Validator = new NotifiarrSettingsValidator();

        [FieldDefinition(0, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey, HelpText = "NotificationsNotifiarrSettingsApiKeyHelpText", HelpLink = "https://notifiarr.com")]
        public string ApiKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
