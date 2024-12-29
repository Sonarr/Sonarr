using Workarr.Download.TrackedDownloads;

namespace Workarr.Notifications.Webhook
{
    public class WebhookDownloadStatusMessage
    {
        public string Title { get; set; }
        public List<string> Messages { get; set; }

        public WebhookDownloadStatusMessage(TrackedDownloadStatusMessage statusMessage)
        {
            Title = statusMessage.Title;
            Messages = statusMessage.Messages.ToList();
        }
    }
}
