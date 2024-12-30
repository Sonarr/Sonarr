namespace Workarr.Notifications.Webhook
{
    public class WebhookSeriesDeletePayload : WebhookPayload
    {
        public WebhookSeries Series { get; set; }
        public bool DeletedFiles { get; set; }
    }
}
