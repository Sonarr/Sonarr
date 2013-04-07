using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        bool DownloadReport(IndexerParseResult parseResult);
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

        public bool DownloadReport(IndexerParseResult parseResult)
        {
            var downloadTitle = parseResult.OriginalString;
            if (!_configService.DownloadClientUseSceneName)
            {
                downloadTitle = parseResult.GetDownloadTitle();
            }

            var provider = _downloadClientProvider.GetDownloadClient();
            var recentEpisode = ContainsRecentEpisode(parseResult);

            bool success = provider.DownloadNzb(parseResult.NzbUrl, downloadTitle, recentEpisode);

            if (success)
            {
                _logger.Info("Report sent to download client. {0}", downloadTitle);
                _eventAggregator.Publish(new EpisodeGrabbedEvent(parseResult));
            }

            return success;
        }

        private static bool ContainsRecentEpisode(IndexerParseResult parseResult)
        {
            return parseResult.Episodes.Any(e => e.AirDate >= DateTime.Today.AddDays(-7));
        }
    }
}