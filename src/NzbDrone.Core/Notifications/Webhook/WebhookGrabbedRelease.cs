using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookGrabbedRelease
    {
        public WebhookGrabbedRelease()
        {
        }

        public WebhookGrabbedRelease(GrabbedReleaseInfo release)
        {
            if (release == null)
            {
                return;
            }

            ReleaseTitle = release.Title;
            Indexer = release.Indexer;
            Size = release.Size;
        }

        public string ReleaseTitle { get; set; }
        public string Indexer { get; set; }
        public long Size { get; set; }
    }
}
