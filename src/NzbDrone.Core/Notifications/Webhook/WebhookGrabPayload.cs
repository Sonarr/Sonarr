using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookGrabPayload : WebhookPayload
    {
        public List<WebhookEpisode> Episodes { get; set; }
        public WebhookRelease Release { get; set; }
    }
}
