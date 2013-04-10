using NzbDrone.Common.Eventing;

namespace NzbDrone.Core.Indexers
{
    public class IndexerSettingUpdatedEvent : IEvent
    {
        public string IndexerName { get; private set; }
        public IIndexerSetting IndexerSetting { get; private set; }

        public IndexerSettingUpdatedEvent(string indexerName, IIndexerSetting indexerSetting)
        {
            IndexerName = indexerName;
            IndexerSetting = indexerSetting;
        }
    }
}