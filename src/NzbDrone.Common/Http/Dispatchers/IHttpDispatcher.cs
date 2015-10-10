using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.TPL;

namespace NzbDrone.Common.Http.Dispatchers
{
    public interface IHttpDispatcher
    {
        HttpResponse GetResponse(HttpRequest request, CookieContainer cookies);
    }
}
