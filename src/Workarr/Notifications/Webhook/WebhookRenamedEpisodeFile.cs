using Workarr.MediaFiles;

namespace Workarr.Notifications.Webhook
{
    public class WebhookRenamedEpisodeFile : WebhookEpisodeFile
    {
        public WebhookRenamedEpisodeFile(RenamedEpisodeFile renamedEpisode)
            : base(renamedEpisode.EpisodeFile)
        {
            PreviousRelativePath = renamedEpisode.PreviousRelativePath;
            PreviousPath = renamedEpisode.PreviousPath;
        }

        public string PreviousRelativePath { get; set; }
        public string PreviousPath { get; set; }
    }
}
