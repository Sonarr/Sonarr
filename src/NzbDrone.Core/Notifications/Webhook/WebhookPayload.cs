using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookPayload
    {
        public string EventType { get; set; }
        public WebhookSeries Series { get; set; }
        public List<WebhookEpisode> Episodes { get; set; }
    }
}