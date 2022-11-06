namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookPayload
    {
        public WebhookEventType EventType { get; set; }
        public string InstanceName { get; set; }
        public string ApplicationUrl { get; set; }
    }
}
