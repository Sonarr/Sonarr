using System;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Indexers
{
    public abstract class IndexerWithSetting<TSetting> :
        Indexer,
        IHandle<IndexerSettingUpdatedEvent> where TSetting : IIndexerSetting, new()
    {
        protected IndexerWithSetting(IProviderIndexerSetting settingProvider)
        {
            Settings = settingProvider.Get<TSetting>(this);
        }

        public override bool IsConfigured
        {
            get { return Settings.IsValid; }
        }

        protected TSetting Settings { get; private set; }

        public void Handle(IndexerSettingUpdatedEvent message)
        {
            if (message.IndexerName.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
            {
                Settings = (TSetting)message.IndexerSetting;
            }
        }
    }
}