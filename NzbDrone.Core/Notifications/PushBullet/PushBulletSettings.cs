using System;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class PushBulletSettings : IProviderConfig
    {
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
    }
}
