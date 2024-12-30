namespace Workarr.Notifications.Webhook
{
    public class WebhookGrabPayload : WebhookPayload
    {
        public WebhookSeries Series { get; set; }
        public List<WebhookEpisode> Episodes { get; set; }
        public WebhookRelease Release { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadId { get; set; }
        public WebhookCustomFormatInfo CustomFormatInfo { get; set; }
    }
}
