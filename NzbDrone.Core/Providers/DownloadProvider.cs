using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class DownloadProvider
    {
        private readonly SabProvider _sabProvider;
        private readonly HistoryProvider _historyProvider;
        private readonly EpisodeProvider _episodeProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DownloadProvider(SabProvider sabProvider, HistoryProvider historyProvider, EpisodeProvider episodeProvider)
        {
            _sabProvider = sabProvider;
            _historyProvider = historyProvider;
            _episodeProvider = episodeProvider;
        }

        public DownloadProvider()
        {
        }

        public virtual bool DownloadReport(EpisodeParseResult parseResult)
        {
            var sabTitle = _sabProvider.GetSabTitle(parseResult);

            if (_sabProvider.IsInQueue(sabTitle))
            {
                Logger.Warn("Episode {0} is already in sab's queue. skipping.", parseResult);
                return false;
            }

            var addSuccess = _sabProvider.AddByUrl(parseResult.NzbUrl, sabTitle);

            if (addSuccess)
            {
                foreach (var episode in parseResult.Episodes)
                {
                    var history = new History();
                    history.Date = DateTime.Now;
                    history.Indexer = parseResult.Indexer;
                    history.IsProper = parseResult.Proper;
                    history.Quality = parseResult.Quality;
                    history.NzbTitle = parseResult.NzbTitle;
                    history.EpisodeId = episode.EpisodeId;

                    _historyProvider.Add(history);
                }
            }

            return addSuccess;
        }
    }
}