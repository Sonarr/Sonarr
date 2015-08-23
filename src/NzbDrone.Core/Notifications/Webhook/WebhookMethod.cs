using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Notifications.Webhook
{
    public enum WebhookMethod
    {
        POST = RestSharp.Method.POST,
        PUT = RestSharp.Method.PUT
    }
}