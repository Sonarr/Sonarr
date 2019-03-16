namespace NzbDrone.Core.Notifications.Discord.Payloads
{
    public class Embed
    {
        public string Description { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int Color { get; set; }
    }
}
