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
            RuleFor(c => c.DeviceId).NotEmpty();
        }
    }

    public class PushBulletSettings : IProviderConfig
    {
        private static readonly PushBulletSettingsValidator Validator = new PushBulletSettingsValidator();

        [FieldDefinition(0, Label = "API Key", HelpLink = "https://www.pushbullet.com/")]
        public String ApiKey { get; set; }

        [FieldDefinition(1, Label = "Device ID", HelpText = "device_iden in the device's URL on pubshbullet.com")]
        public String DeviceId { get; set; }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(ApiKey) && !String.IsNullOrWhiteSpace(DeviceId);
            }
        }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
