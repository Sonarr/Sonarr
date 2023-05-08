namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookSeriesAddPayload : WebhookPayload
    {
        public WebhookSeries Series { get; set; }
    }
}
