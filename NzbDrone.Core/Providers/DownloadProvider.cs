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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DownloadProvider(SabProvider sabProvider)
        {
            _sabProvider = sabProvider;
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

            return _sabProvider.AddByUrl(parseResult.NzbUrl, sabTitle);
        }
    }
}