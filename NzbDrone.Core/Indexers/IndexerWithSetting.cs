using NzbDrone.Common;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Indexers
{
    public abstract class IndexerWithSetting<TSetting> : IndexerBase where TSetting : class, IIndexerSetting, new()
    {
        public TSetting Settings { get; private set; }

        public TSetting ImportSettingsFromJson(string json)
        {
            Settings = new JsonSerializer().Deserialize<TSetting>(json) ?? new TSetting();

            return Settings;
        }
    }
}