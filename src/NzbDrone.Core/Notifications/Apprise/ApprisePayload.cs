namespace NzbDrone.Core.Notifications.Apprise
{
    public class ApprisePayload
    {
        public string Urls { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public AppriseNotificationType Type { get; set; }

        public string Tag { get; set; }
    }
}
