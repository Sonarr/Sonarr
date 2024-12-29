using Workarr.MediaCover;

namespace Workarr.Notifications.Webhook
{
    public class WebhookImage
    {
        public MediaCoverTypes CoverType { get; set; }
        public string Url { get; set; }
        public string RemoteUrl { get; set; }

        public WebhookImage(MediaCover.MediaCover image)
        {
            CoverType = image.CoverType;
            RemoteUrl = image.RemoteUrl;
            Url = image.Url;
        }
    }
}
