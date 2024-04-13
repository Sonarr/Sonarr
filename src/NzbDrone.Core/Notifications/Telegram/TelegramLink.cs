namespace NzbDrone.Core.Notifications.Telegram
{
    public class TelegramLink
    {
        public string Label { get; set; }
        public string Link { get; set; }

        public TelegramLink(string label, string link)
        {
            Label = label;
            Link = link;
        }
    }
}
