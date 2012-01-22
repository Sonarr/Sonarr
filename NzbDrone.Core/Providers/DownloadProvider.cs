using System;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class DownloadProvider
    {
        private readonly SabProvider _sabProvider;
        private readonly HistoryProvider _historyProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public DownloadProvider(SabProvider sabProvider, HistoryProvider historyProvider,
            EpisodeProvider episodeProvider, ExternalNotificationProvider externalNotificationProvider)
        {
            _sabProvider = sabProvider;
            _historyProvider = historyProvider;
            _episodeProvider = episodeProvider;
            _externalNotificationProvider = externalNotificationProvider;
        }

        public DownloadProvider()
        {
        }

        public virtual bool DownloadReport(EpisodeParseResult parseResult)
        {
            if (_sabProvider.IsInQueue(parseResult))
            {
                Logger.Warn("Episode {0} is already in sab's queue. skipping.", parseResult);
                return false;
            }

            var sabTitle = _sabProvider.GetSabTitle(parseResult);
            var addSuccess = _sabProvider.AddByUrl(parseResult.NzbUrl, sabTitle);

            if (addSuccess)
            {
                Logger.Trace("Download added to Queue: {0}", sabTitle);

                foreach (var episode in _episodeProvider.GetEpisodesByParseResult(parseResult))
                {
                    var history = new History();
                    history.Date = DateTime.Now;
                    history.Indexer = parseResult.Indexer;
                    history.IsProper = parseResult.Quality.Proper;
                    history.Quality = parseResult.Quality.QualityType;
                    history.NzbTitle = parseResult.OriginalString;
                    history.EpisodeId = episode.EpisodeId;
                    history.SeriesId = episode.SeriesId;

                    _historyProvider.Add(history);
                    _episodeProvider.MarkEpisodeAsFetched(episode.EpisodeId);
                }
            }

            _externalNotificationProvider.OnGrab(sabTitle);

            return addSuccess;
        }
    }
}