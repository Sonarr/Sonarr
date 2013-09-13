using NLog;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        void DownloadReport(RemoteEpisode remoteEpisode);
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

        public void DownloadReport(RemoteEpisode remoteEpisode)
        {
            var downloadTitle = remoteEpisode.Release.Title;
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            if (!downloadClient.IsConfigured)
            {
                _logger.Warn("Download client {0} isn't configured yet.", downloadClient.GetType().Name);
                return;
            }

            downloadClient.DownloadNzb(remoteEpisode);

            _logger.ProgressInfo("Report sent to download client. {0}", downloadTitle);
            _messageAggregator.PublishEvent(new EpisodeGrabbedEvent(remoteEpisode));
        }
    }
}