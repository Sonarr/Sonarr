using System.Net;

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