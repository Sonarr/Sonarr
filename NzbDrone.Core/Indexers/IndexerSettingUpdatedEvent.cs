using NzbDrone.Common.Messaging;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public class IndexerSettingUpdatedEvent : IEvent
    {
        public string IndexerName { get; private set; }
        public IProviderConfig IndexerSetting { get; private set; }

        public IndexerSettingUpdatedEvent(string indexerName, IProviderConfig indexerSetting)
        {
            IndexerName = indexerName;
            IndexerSetting = indexerSetting;
        }
    }
}