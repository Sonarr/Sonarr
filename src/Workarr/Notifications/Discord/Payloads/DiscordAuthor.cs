using Newtonsoft.Json;

namespace Workarr.Notifications.Discord.Payloads
{
    public class DiscordAuthor
    {
        public string Name { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
    }
}
