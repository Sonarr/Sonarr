using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        bool DownloadReport(RemoteEpisode episode);
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

        public bool DownloadReport(RemoteEpisode episode)
        {
            var downloadTitle = episode.Report.Title;

            var provider = _downloadClientProvider.GetDownloadClient();
            var recentEpisode = ContainsRecentEpisode(episode);

            bool success = provider.DownloadNzb(episode.Report.NzbUrl, downloadTitle, recentEpisode);

            if (success)
            {
                _logger.Info("Report sent to download client. {0}", downloadTitle);
                _messageAggregator.PublishEvent(new EpisodeGrabbedEvent(episode));
            }

            return success;
        }

        private static bool ContainsRecentEpisode(RemoteEpisode episode)
        {
            return episode.Episodes.Any(e => e.AirDate >= DateTime.Today.AddDays(-7));
        }
    }
}