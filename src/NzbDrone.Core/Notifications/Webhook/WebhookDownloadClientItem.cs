using NzbDrone.Core.Download;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookDownloadClientItem
    {
        public WebhookDownloadClientItem()
        {
        }

        public WebhookDownloadClientItem(QualityModel quality, DownloadClientItem downloadClientItem)
        {
            Quality = quality.Quality.Name;
            QualityVersion = quality.Revision.Version;
            Title = downloadClientItem.Title;
            Size = downloadClientItem.TotalSize;
        }

        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string Title { get; set; }
        public string Indexer { get; set; }
        public long Size { get; set; }
    }
}
