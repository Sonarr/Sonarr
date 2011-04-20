using System.Linq;
using NLog;

namespace NzbDrone.Core.Providers.Timers
{
    public class RssSyncTimer : ITimer
    {
        private readonly IndexerProvider _indexerProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RssSyncTimer(IndexerProvider indexerProvider)
        {
            _indexerProvider = indexerProvider;
        }

        public string Name
        {
            get { return "RSS Sync"; }
        }

        public int DefaultInterval
        {
            get { return 15; }
        }

        public void Start()
        {
            Logger.Info("Doing Things!!!!");

            var indexers = _indexerProvider.AllIndexers().Where(c => c.Enable);

            foreach (var indexerSetting in indexers)
            {
                
            }
        }
    }
}