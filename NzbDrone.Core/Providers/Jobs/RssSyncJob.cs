using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Indexer;

namespace NzbDrone.Core.Providers.Jobs
{
    public class RssSyncJob : IJob
    {
        private readonly IEnumerable<IndexerBase> _indexers;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RssSyncJob(IEnumerable<IndexerBase> indexers)
        {
            _indexers = indexers;
        }

        public string Name
        {
            get { return "RSS Sync"; }
        }

        public int DefaultInterval
        {
            get { return 15; }
        }

        public void Start(ProgressNotification notification, int targetId)
        {
            foreach (var indexer in _indexers.Where(i => i.Settings.Enable))
            {
                indexer.Fetch();
            }
        }
    }
}