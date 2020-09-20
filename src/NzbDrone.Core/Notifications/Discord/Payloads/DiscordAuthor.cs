using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Discord.Payloads
{
    public class DiscordAuthor
    {
        public string Name { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
    }
}
