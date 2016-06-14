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

        public List<Attachment> Attachments { get; set; }
    }
}
