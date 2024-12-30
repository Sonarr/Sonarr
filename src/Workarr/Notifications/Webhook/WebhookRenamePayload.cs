namespace Workarr.Notifications.Webhook
{
    public class WebhookRenamePayload : WebhookPayload
    {
        public WebhookSeries Series { get; set; }
        public List<WebhookRenamedEpisodeFile> RenamedEpisodeFiles { get; set; }
    }
}
