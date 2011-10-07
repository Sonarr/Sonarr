using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NzbDrone.Providers
{
    public class WebClientProvider
    {

        public virtual string DownloadString(string url)
        {
            return new WebClient().DownloadString(url);
        }
    }
}
