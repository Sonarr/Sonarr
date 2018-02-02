using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Slack.Payloads
{
    public class SlackPayload
    {
        public string Text { get; set; }

        public string Username { get; set; }

        [JsonProperty("icon_emoji")]
        public string IconEmoji { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        public string Channel { get; set; }

        public List<Attachment> Attachments { get; set; }
    }
}
