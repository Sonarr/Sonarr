namespace NzbDrone.Core.Notifications.Telegram
{
    public class TelegramLink
    {
        public MetadataLinkType? Type { get; set; }
        public string Label { get; set; }
        public string Link { get; set; }

        public TelegramLink(MetadataLinkType? type, string label, string link)
        {
            Type = type;
            Label = label;
            Link = link;
        }
    }
}
