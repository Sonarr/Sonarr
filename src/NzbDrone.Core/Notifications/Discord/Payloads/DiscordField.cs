namespace NzbDrone.Core.Notifications.Discord.Payloads
{
    public class DiscordField
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Inline { get; set; }
    }
}
