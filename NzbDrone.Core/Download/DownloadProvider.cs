using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadProvider
    {
        bool DownloadReport(EpisodeParseResult parseResult);
        IDownloadClient GetActiveDownloadClient();
        bool ContainsRecentEpisode(EpisodeParseResult parseResult);
    }

    public class DownloadProvider : IDownloadProvider
    {
        private readonly SabProvider _sabProvider;
        private readonly IConfigService _configService;
        private readonly BlackholeProvider _blackholeProvider;
        private readonly PneumaticProvider _pneumaticProvider;
        private readonly NzbgetProvider _nzbgetProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;


        public DownloadProvider(SabProvider sabProvider, IConfigService configService,
            BlackholeProvider blackholeProvider,
            PneumaticProvider pneumaticProvider,
            NzbgetProvider nzbgetProvider,
            IEventAggregator eventAggregator, Logger logger)
        {
            _sabProvider = sabProvider;
            _configService = configService;
            _blackholeProvider = blackholeProvider;
            _pneumaticProvider = pneumaticProvider;
            _nzbgetProvider = nzbgetProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }


        public virtual bool DownloadReport(EpisodeParseResult parseResult)
        {
            var downloadTitle = parseResult.OriginalString;
            if (!_configService.DownloadClientUseSceneName)
            {
                downloadTitle = parseResult.GetDownloadTitle();
            }

            var provider = GetActiveDownloadClient();
            var recentEpisode = ContainsRecentEpisode(parseResult);

            bool success = provider.DownloadNzb(parseResult.NzbUrl, downloadTitle, recentEpisode);

            if (success)
            {
                _logger.Trace("Download added to Queue: {0}", downloadTitle);
                _eventAggregator.Publish(new EpisodeGrabbedEvent(parseResult));
            }

            return success;
        }

        public virtual IDownloadClient GetActiveDownloadClient()
        {
            switch (_configService.DownloadClient)
            {
                case DownloadClientType.Blackhole:
                    return _blackholeProvider;

                case DownloadClientType.Pneumatic:
                    return _pneumaticProvider;

                case DownloadClientType.Nzbget:
                    return _nzbgetProvider;

                default:
                    return _sabProvider;
            }
        }

        public virtual bool ContainsRecentEpisode(EpisodeParseResult parseResult)
        {
            return parseResult.Episodes.Any(e => e.AirDate >= DateTime.Today.AddDays(-7));
        }
    }
}