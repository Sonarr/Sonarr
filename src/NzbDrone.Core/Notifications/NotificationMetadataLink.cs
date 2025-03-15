namespace NzbDrone.Core.Notifications
{
    public class NotificationMetadataLink
    {
        public MetadataLinkType? Type { get; set; }
        public string Label { get; set; }
        public string Link { get; set; }

        public NotificationMetadataLink(MetadataLinkType? type, string label, string link)
        {
            Type = type;
            Label = label;
            Link = link;
        }
    }
}
