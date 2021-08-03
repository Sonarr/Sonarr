using NzbDrone.Common.Http;

namespace NzbDrone.Core.Notifications.Webhook
{
    public enum WebhookMethod
    {
        POST = HttpMethod.POST,
        PUT = HttpMethod.PUT
    }
}
