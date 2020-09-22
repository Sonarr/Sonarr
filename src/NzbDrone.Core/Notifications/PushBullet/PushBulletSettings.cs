using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class PushBulletSettingsValidator : AbstractValidator<PushBulletSettings>
    {
        public PushBulletSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class PushBulletSettings : IProviderConfig
    {
        private static readonly PushBulletSettingsValidator Validator = new PushBulletSettingsValidator();

        public PushBulletSettings()
        {
            DeviceIds = new string[] { };
            ChannelTags = new string[] { };
        }

        [FieldDefinition(0, Label = "Access Token", Privacy = PrivacyLevel.ApiKey, HelpLink = "https://www.pushbullet.com/#settings/account")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "Device IDs", HelpText = "List of device IDs (leave blank to send to all devices)", Type = FieldType.Device)]
        public IEnumerable<string> DeviceIds { get; set; }

        [FieldDefinition(2, Label = "Channel Tags", HelpText = "List of Channel Tags to send notifications to", Type = FieldType.Tag)]
        public IEnumerable<string> ChannelTags { get; set; }

        [FieldDefinition(3, Label = "Sender ID", HelpText = "The device ID to send notifications from, use device_iden in the device's URL on pushbullet.com (leave blank to send from yourself)")]
        public string SenderId { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
