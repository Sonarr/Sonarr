
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider
{


    public class DownloadProviderRepository : BasicRepository<DownloadProviderModel>
    {
        public DownloadProviderRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }

    public class DownloadProviderModel : Provider<DownloadProviderConfig>
    {

    }

    public class DownloadProviderConfig : ProviderSetting
    {

    }


    public abstract class Provider<TSettings> : ModelBase
        where TSettings : ProviderSetting
    {
        public string Implementation { get; set; }
        public TSettings Config { get; set; }
    }

    public abstract class ProviderSetting : IEmbeddedDocument
    {

    }

    public abstract class ProviderBase<TSettings> where TSettings : ProviderSetting
    {
        public TSettings Settings { get; private set; }

        public void LoadSettings(TSettings setting)
        {
            Settings = setting;
        }
    }
}