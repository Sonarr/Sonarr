using System;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Instrumentation.Extensions;
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
            Ensure.That(remoteEpisode.Series, () => remoteEpisode.Series).IsNotNull();
            Ensure.That(remoteEpisode.Episodes, () => remoteEpisode.Episodes).HasItems();

            var downloadTitle = remoteEpisode.Release.Title;
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            if (downloadClient == null)
            {
                _logger.Warn("Download client isn't configured yet.");
                return;
            }

            var downloadClientId = downloadClient.DownloadNzb(remoteEpisode);
            var episodeGrabbedEvent = new EpisodeGrabbedEvent(remoteEpisode);
            episodeGrabbedEvent.DownloadClient = downloadClient.GetType().Name;

            if (!String.IsNullOrWhiteSpace(downloadClientId))
            {
                episodeGrabbedEvent.DownloadClientId = downloadClientId;
            }

            _logger.ProgressInfo("Report sent to download client. {0}", downloadTitle);
            _eventAggregator.PublishEvent(episodeGrabbedEvent);
        }
    }
}