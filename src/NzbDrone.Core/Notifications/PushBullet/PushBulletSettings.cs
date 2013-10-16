using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class PushBulletSettingsValidator : AbstractValidator<PushBulletSettings>
    {
        public PushBulletSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.DeviceId).GreaterThan(0);
        }
    }

    public class PushBulletSettings : IProviderConfig
    {
        private static readonly PushBulletSettingsValidator Validator = new PushBulletSettingsValidator();

        [FieldDefinition(0, Label = "API Key", HelpLink = "https://www.pushbullet.com/")]
        public String ApiKey { get; set; }

        [FieldDefinition(1, Label = "Device ID")]
        public Int32 DeviceId { get; set; }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(ApiKey) && DeviceId > 0;
            }
        }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
