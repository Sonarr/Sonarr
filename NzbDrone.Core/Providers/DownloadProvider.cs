using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.DownloadClients;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class DownloadProvider
    {
        private readonly SabProvider _sabProvider;
        private readonly HistoryProvider _historyProvider;
        private readonly EpisodeService _episodeService;
        private readonly ExternalNotificationProvider _externalNotificationProvider;
        private readonly ConfigProvider _configProvider;
        private readonly BlackholeProvider _blackholeProvider;
        private readonly SignalRProvider _signalRProvider;
        private readonly PneumaticProvider _pneumaticProvider;
        private readonly NzbgetProvider _nzbgetProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public DownloadProvider(SabProvider sabProvider, HistoryProvider historyProvider,
            EpisodeService episodeService, ExternalNotificationProvider externalNotificationProvider,
            ConfigProvider configProvider, BlackholeProvider blackholeProvider,
            SignalRProvider signalRProvider, PneumaticProvider pneumaticProvider,
            NzbgetProvider nzbgetProvider)
        {
            _sabProvider = sabProvider;
            _historyProvider = historyProvider;
            _episodeService = episodeService;
            _externalNotificationProvider = externalNotificationProvider;
            _configProvider = configProvider;
            _blackholeProvider = blackholeProvider;
            _signalRProvider = signalRProvider;
            _pneumaticProvider = pneumaticProvider;
            _nzbgetProvider = nzbgetProvider;
        }

        public DownloadProvider()
        {
        }

        public virtual bool DownloadReport(EpisodeParseResult parseResult)
        {
            var downloadTitle = GetDownloadTitle(parseResult);
            var provider = GetActiveDownloadClient();
            var recentEpisode = ContainsRecentEpisode(parseResult);

            bool success = provider.DownloadNzb(parseResult.NzbUrl, downloadTitle, recentEpisode);

            if (success)
            {
                logger.Trace("Download added to Queue: {0}", downloadTitle);

                foreach (var episode in parseResult.Episodes)
                {
                    var history = new History
                                      {
                                            Date = DateTime.Now,
                                            Indexer = parseResult.Indexer,
                                            IsProper = parseResult.Quality.Proper,
                                            Quality = parseResult.Quality.Quality,
                                            NzbTitle = parseResult.OriginalString,
                                            EpisodeId = episode.OID,
                                            SeriesId = episode.SeriesId,
                                            NzbInfoUrl = parseResult.NzbInfoUrl,
                                            ReleaseGroup = parseResult.ReleaseGroup,
                                      };

                    _historyProvider.Add(history);
                    _episodeService.MarkEpisodeAsFetched(episode.OID);

                    _signalRProvider.UpdateEpisodeStatus(episode.OID, EpisodeStatusType.Downloading, null);
                }

                _externalNotificationProvider.OnGrab(downloadTitle);
            }

            return success;
        }

        public virtual IDownloadClient GetActiveDownloadClient()
        {
            switch (_configProvider.DownloadClient)
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

        public virtual String GetDownloadTitle(EpisodeParseResult parseResult)
        {
            if(_configProvider.DownloadClientUseSceneName)
            {
                logger.Trace("Using scene name: {0}", parseResult.OriginalString);
                return parseResult.OriginalString;
            }

            var seriesTitle = MediaFileProvider.CleanFilename(parseResult.Series.Title);

            //Handle Full Naming
            if (parseResult.FullSeason)
            {
                var seasonResult = String.Format("{0} - Season {1} [{2}]", seriesTitle,
                                     parseResult.SeasonNumber, parseResult.Quality.Quality);

                if (parseResult.Quality.Proper)
                    seasonResult += " [Proper]";

                return seasonResult;
            }

            if (parseResult.Series.SeriesType == SeriesType.Daily)
            {
                var dailyResult = String.Format("{0} - {1:yyyy-MM-dd} - {2} [{3}]", seriesTitle,
                                     parseResult.AirDate, parseResult.Episodes.First().Title, parseResult.Quality.Quality);

                if (parseResult.Quality.Proper)
                    dailyResult += " [Proper]";

                return dailyResult;
            }

            //Show Name - 1x01-1x02 - Episode Name
            //Show Name - 1x01 - Episode Name
            var episodeString = new List<String>();
            var episodeNames = new List<String>();

            foreach (var episode in parseResult.Episodes)
            {
                episodeString.Add(String.Format("{0}x{1:00}", episode.SeasonNumber, episode.EpisodeNumber));
                episodeNames.Add(Parser.CleanupEpisodeTitle(episode.Title));
            }

            var epNumberString = String.Join("-", episodeString);
            string episodeName;


            if (episodeNames.Distinct().Count() == 1)
                episodeName = episodeNames.First();

            else
                episodeName = String.Join(" + ", episodeNames.Distinct());

            var result = String.Format("{0} - {1} - {2} [{3}]", seriesTitle, epNumberString, episodeName, parseResult.Quality.Quality);

            if (parseResult.Quality.Proper)
            {
                result += " [Proper]";
            }

            return result;
        }

        public virtual bool ContainsRecentEpisode(EpisodeParseResult parseResult)
        {
            return parseResult.Episodes.Any(e => e.AirDate >= DateTime.Today.AddDays(-7));
        }
    }
}