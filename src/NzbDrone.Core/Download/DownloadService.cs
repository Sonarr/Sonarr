using System;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
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
        private readonly IRateLimitService _rateLimitService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public DownloadService(IProvideDownloadClient downloadClientProvider,
            IRateLimitService rateLimitService,
            IEventAggregator eventAggregator,
            Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _rateLimitService = rateLimitService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void DownloadReport(RemoteEpisode remoteEpisode)
        {
            Ensure.That(remoteEpisode.Series, () => remoteEpisode.Series).IsNotNull();
            Ensure.That(remoteEpisode.Episodes, () => remoteEpisode.Episodes).HasItems();

            var downloadTitle = remoteEpisode.Release.Title;
            var downloadClient = _downloadClientProvider.GetDownloadClient(remoteEpisode.Release.DownloadProtocol);

            if (downloadClient == null)
            {
                _logger.Warn("{0} Download client isn't configured yet.", remoteEpisode.Release.DownloadProtocol);
                return;
            }

            // Limit grabs to 2 per second.
            if (remoteEpisode.Release.DownloadUrl.IsNotNullOrWhiteSpace() && !remoteEpisode.Release.DownloadUrl.StartsWith("magnet:"))
            {
                var uri = new Uri(remoteEpisode.Release.DownloadUrl);
                _rateLimitService.WaitAndPulse(uri.Host, TimeSpan.FromSeconds(2));
            }

            var downloadClientId = downloadClient.Download(remoteEpisode);
            var episodeGrabbedEvent = new EpisodeGrabbedEvent(remoteEpisode);
            episodeGrabbedEvent.DownloadClient = downloadClient.GetType().Name;

            if (!String.IsNullOrWhiteSpace(downloadClientId))
            {
                episodeGrabbedEvent.DownloadId = downloadClientId;
            }

            _logger.ProgressInfo("Report sent to download client. {0}", downloadTitle);
            _eventAggregator.PublishEvent(episodeGrabbedEvent);
        }
    }
}