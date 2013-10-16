using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

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
        public String ApiKey { get; set; }

        [FieldDefinition(1, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(NotifyMyAndroidPriority))]
        public Int32 Priority { get; set; }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(ApiKey) && Priority != null & Priority >= -1 && Priority <= 2;
            }
        }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
