using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookRenamePayload : WebhookPayload
    {
        public WebhookSeries Series { get; set; }
        public List<WebhookRenamedEpisodeFile> RenamedEpisodeFiles { get; set; }
    }
}
