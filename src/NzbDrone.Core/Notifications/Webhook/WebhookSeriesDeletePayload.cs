using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookSeriesDeletePayload : WebhookPayload
    {
        public WebhookSeries Series { get; set; }
        public bool DeletedFiles { get; set; }
    }
}
