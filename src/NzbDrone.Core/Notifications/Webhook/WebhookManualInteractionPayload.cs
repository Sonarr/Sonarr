using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookManualInteractionPayload : WebhookPayload
    {
        public WebhookSeries Series { get; set; }
        public List<WebhookEpisode> Episodes { get; set; }
        public WebhookDownloadClientItem DownloadInfo { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadId { get; set; }
        public WebhookCustomFormatInfo CustomFormatInfo { get; set; }
        public WebhookGrabbedRelease Release { get; set; }
    }
}
