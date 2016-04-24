using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Common.Http
{
    public interface IHttpProxySettingsProvider
    {
        HttpRequestProxySettings GetProxySettings(HttpRequest request);
    }
}
