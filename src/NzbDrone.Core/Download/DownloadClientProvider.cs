using System.Linq;

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