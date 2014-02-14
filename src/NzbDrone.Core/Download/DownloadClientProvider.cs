using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Download.Clients.Sabnzbd;

namespace NzbDrone.Core.Download
{
    public interface IProvideDownloadClient
    {
        IDownloadClient GetDownloadClient();
    }

    public class DownloadClientProvider : IProvideDownloadClient
    {
        private readonly IDownloadClientFactory _downloadClientFactory;

        public DownloadClientProvider(IDownloadClientFactory downloadClientFactory)
        {
            _downloadClientFactory = downloadClientFactory;
        }

        public IDownloadClient GetDownloadClient()
        {
            return _downloadClientFactory.Enabled().FirstOrDefault();
        }
    }
}