using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        bool DownloadReport(RemoteEpisode remoteEpisode);
    }

    public class DownloadService : IDownloadService
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Logger _logger;


        public DownloadService(IProvideDownloadClient downloadClientProvider,
            IMessageAggregator messageAggregator, Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _messageAggregator = messageAggregator;
            _logger = logger;
        }

        public bool DownloadReport(RemoteEpisode remoteEpisode)
        {
            var downloadTitle = remoteEpisode.Report.Title;
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            if (!downloadClient.IsConfigured)
            {
                _logger.Warn("Download client {0} isn't configured yet.", downloadClient.GetType().Name);
                return false;
            }

            bool success = downloadClient.DownloadNzb(remoteEpisode);

            if (success)
            {
                _logger.Info("Report sent to download client. {0}", downloadTitle);
                _messageAggregator.PublishEvent(new EpisodeGrabbedEvent(remoteEpisode));
            }

            return success;
        }
    }
}