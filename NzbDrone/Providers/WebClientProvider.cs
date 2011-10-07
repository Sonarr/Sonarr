using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NzbDrone.Providers
{
    internal class WebClientProvider
    {

        public virtual string DownloadString(string url)
        {
            return new WebClient().DownloadString(url);
        }
    }
}
