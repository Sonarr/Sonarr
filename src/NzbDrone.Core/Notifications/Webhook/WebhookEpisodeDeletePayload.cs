using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookEpisodeDeletePayload : WebhookPayload
    {
        public WebhookSeries Series { get; set; }
        public List<WebhookEpisode> Episodes { get; set; }
        public EpisodeFile EpisodeFile { get; set; }
        public DeleteMediaFileReason DeleteReason { get; set; }
    }
}
