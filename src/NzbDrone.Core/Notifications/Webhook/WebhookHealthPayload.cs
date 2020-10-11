using NzbDrone.Core.HealthCheck;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookHealthPayload : WebhookPayload
    {
        public HealthCheckResult Level { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string WikiUrl { get; set; }
    }
}
