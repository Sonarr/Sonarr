using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        bool DownloadReport(RemoteEpisode episode);
    }

    public class DownloadService : IDownloadService
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;


        public DownloadService(IProvideDownloadClient downloadClientProvider, IConfigService configService,
            IEventAggregator eventAggregator, Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _configService = configService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public bool DownloadReport(RemoteEpisode episode)
        {
            var downloadTitle = episode.Report.Title;
            if (!_configService.DownloadClientUseSceneName)
            {
                downloadTitle = episode.GetDownloadTitle();
            }

            var provider = _downloadClientProvider.GetDownloadClient();
            var recentEpisode = ContainsRecentEpisode(episode);

            bool success = provider.DownloadNzb(episode.Report.NzbUrl, downloadTitle, recentEpisode);

            if (success)
            {
                _logger.Info("Report sent to download client. {0}", downloadTitle);
                _eventAggregator.Publish(new EpisodeGrabbedEvent(episode));
            }

            return success;
        }

        private static bool ContainsRecentEpisode(RemoteEpisode episode)
        {
            return episode.Episodes.Any(e => e.AirDate >= DateTime.Today.AddDays(-7));
        }
    }
}