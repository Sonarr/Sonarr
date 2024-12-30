using Workarr.Parser.Model;

namespace Workarr.Notifications.Webhook
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
            ReleaseType = release.ReleaseType;
        }

        public WebhookGrabbedRelease(GrabbedReleaseInfo release, ReleaseType releaseType)
        {
            if (release == null)
            {
                ReleaseType = releaseType;

                return;
            }

            ReleaseTitle = release.Title;
            Indexer = release.Indexer;
            Size = release.Size;
            ReleaseType = release.ReleaseType;
        }

        public string ReleaseTitle { get; set; }
        public string Indexer { get; set; }
        public long? Size { get; set; }
        public ReleaseType ReleaseType { get; set; }
    }
}
