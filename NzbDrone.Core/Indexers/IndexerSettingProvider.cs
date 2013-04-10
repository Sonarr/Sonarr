using Newtonsoft.Json;

namespace NzbDrone.Core.Indexers
{
    public interface IProviderIndexerSetting
    {
        TSetting Get<TSetting>(IIndexerBase indexer) where TSetting : IIndexerSetting, new();
    }

    public class IndexerSettingProvider : IProviderIndexerSetting
    {
        private readonly IIndexerRepository _indexerRepository;

        public IndexerSettingProvider(IIndexerRepository indexerRepository)
        {
            _indexerRepository = indexerRepository;
        }

        public TSetting Get<TSetting>(IIndexerBase indexer) where TSetting : IIndexerSetting, new()
        {
            var indexerDef = _indexerRepository.Find(indexer.Name);

            if (indexerDef == null || string.IsNullOrWhiteSpace(indexerDef.Settings))
            {
                return new TSetting();
            }

            return JsonConvert.DeserializeObject<TSetting>(indexerDef.Settings);
        }
    }
}