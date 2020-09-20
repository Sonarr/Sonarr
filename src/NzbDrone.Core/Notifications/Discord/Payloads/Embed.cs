using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Discord.Payloads
{
    public class Embed
    {
        public string Description { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int Color { get; set; }
        public string Url { get; set; }
        public DiscordAuthor Author { get; set; }
        public DiscordImage Thumbnail { get; set; }
        public DiscordImage Image { get; set; }
        public string Timestamp { get; set; }
        public List<DiscordField> Fields { get; set; }
    }
}
