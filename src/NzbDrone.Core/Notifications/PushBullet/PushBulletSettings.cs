using System;
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
            DeviceIds = Array.Empty<string>();
            ChannelTags = Array.Empty<string>();
        }

        [FieldDefinition(0, Label = "NotificationsPushBulletSettingsAccessToken", Privacy = PrivacyLevel.ApiKey, HelpLink = "https://www.pushbullet.com/#settings/account")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "NotificationsPushBulletSettingsDeviceIds", HelpText = "NotificationsPushBulletSettingsDeviceIdsHelpText", Type = FieldType.Device)]
        public IEnumerable<string> DeviceIds { get; set; }

        [FieldDefinition(2, Label = "NotificationsPushBulletSettingsChannelTags", HelpText = "NotificationsPushBulletSettingsChannelTagsHelpText", Type = FieldType.Tag)]
        public IEnumerable<string> ChannelTags { get; set; }

        [FieldDefinition(3, Label = "NotificationsPushBulletSettingSenderId", HelpText = "NotificationsPushBulletSettingSenderIdHelpText")]
        public string SenderId { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
