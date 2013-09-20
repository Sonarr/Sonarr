using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Messaging.Events;
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
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;


        public DownloadService(IProvideDownloadClient downloadClientProvider,
            IEventAggregator eventAggregator, Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void DownloadReport(RemoteEpisode remoteEpisode)
        {
            Ensure.That(() => remoteEpisode.Series).IsNotNull();
            Ensure.That(() => remoteEpisode.Episodes).HasItems();

            var downloadTitle = remoteEpisode.Release.Title;
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            if (!downloadClient.IsConfigured)
            {
                _logger.Warn("Download client {0} isn't configured yet.", downloadClient.GetType().Name);
                return;
            }

            downloadClient.DownloadNzb(remoteEpisode);

            _logger.ProgressInfo("Report sent to download client. {0}", downloadTitle);
            _eventAggregator.PublishEvent(new EpisodeGrabbedEvent(remoteEpisode));
        }
    }
}