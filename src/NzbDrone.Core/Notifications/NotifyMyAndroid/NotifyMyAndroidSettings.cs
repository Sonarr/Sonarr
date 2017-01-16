using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.NotifyMyAndroid
{
    public class NotifyMyAndroidSettingsValidator : AbstractValidator<NotifyMyAndroidSettings>
    {
        public NotifyMyAndroidSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class NotifyMyAndroidSettings : IProviderConfig
    {
        private static readonly NotifyMyAndroidSettingsValidator Validator = new NotifyMyAndroidSettingsValidator();

        [FieldDefinition(0, Label = "API Key", HelpLink = "http://www.notifymyandroid.com/")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(NotifyMyAndroidPriority))]
        public int Priority { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(ApiKey) && Priority >= -1 && Priority <= 2;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
