using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class PushBulletDevice
    {
        [JsonProperty(PropertyName = "Iden")]
        public string Id { get; set; }

        public string Nickname { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
    }
}
