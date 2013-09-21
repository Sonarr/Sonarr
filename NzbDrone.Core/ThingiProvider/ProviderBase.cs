
using FluentValidation.Results;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Notifications;

namespace NzbDrone.Core.ThingiProvider
{
    public class NotificationProviderRepository : BasicRepository<NotificationDefinition>
    {
        public NotificationProviderRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }


    public class IndexerProviderRepository : BasicRepository<IndexerDefinition>
    {
        public IndexerProviderRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }

    public abstract class ProviderBase
    {
        public ProviderDefinition Definition { get; set; }
    }

    public abstract class ProviderDefinition : ModelBase
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
        ValidationResult Validate();
    }

    public class NullSetting : IProviderConfig
    {
        public static readonly NullSetting Instance = new NullSetting();

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }
}