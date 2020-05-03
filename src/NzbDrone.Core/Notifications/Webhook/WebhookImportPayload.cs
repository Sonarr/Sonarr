using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookImportPayload : WebhookPayload
    {
        public List<WebhookEpisode> Episodes { get; set; }
        public WebhookEpisodeFile EpisodeFile { get; set; }
        public bool IsUpgrade { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }
    }
}
