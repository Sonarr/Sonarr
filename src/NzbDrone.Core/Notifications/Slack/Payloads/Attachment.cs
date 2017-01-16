namespace NzbDrone.Core.Notifications.Slack.Payloads
{
    public class Attachment
    {
        public string Fallback { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public string Color { get; set; }
    }
}
