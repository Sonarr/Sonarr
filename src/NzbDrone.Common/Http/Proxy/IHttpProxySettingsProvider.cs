using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Common.Http.Proxy
{
    public interface IHttpProxySettingsProvider
    {
        HttpProxySettings GetProxySettings(HttpRequest request);
    }
}
