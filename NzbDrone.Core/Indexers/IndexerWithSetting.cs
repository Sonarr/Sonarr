using NzbDrone.Common;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Indexers
{
    public abstract class IndexerWithSetting<TSetting> : IndexerBase where TSetting : class, IIndexerSetting, new()
    {
        public TSetting Settings { get; set; }

        public override bool EnableByDefault
        {
            get { return false; }
        }

        public TSetting ImportSettingsFromJson(string json)
        {
            Settings = Json.Deserialize<TSetting>(json) ?? new TSetting();

            return Settings;
        }
    }
}