using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers.Jobs
{
    public class RssSyncJob : IJob
    {
        private readonly IndexerProvider _indexerProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RssSyncJob(IndexerProvider indexerProvider)
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

        public void Start(ProgressNotification notification, int targetId)
        {
            Logger.Info("Doing Things!!!!");

            var indexers = _indexerProvider.AllIndexers().Where(c => c.Enable);

            foreach (var indexerSetting in indexers)
            {

            }
        }
    }
}