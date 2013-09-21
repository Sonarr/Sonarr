
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Notifications;

namespace NzbDrone.Core.ThingiProvider
{


    public class DownloadProviderRepository : BasicRepository<DownloadProviderModel>
    {
        public DownloadProviderRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }


    public class NotificationProviderRepository : BasicRepository<NotificationProviderModel>
    {
        public NotificationProviderRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }

    public class DownloadProviderModel : Provider
    {

    }


    public abstract class Provider : ModelBase
    {
        public string Name { get; set; }
        public string Implementation { get; set; }

        public string ConfigContract
        {
            get
            {
                if (Settings == null) return null;
                return Settings.GetType().Name;
            }
            set
            {
                
            }
        }

        public IProviderConfig Settings { get; set; }
    }

    public interface IProviderConfig
    {
        bool IsValid { get; }
    }
}