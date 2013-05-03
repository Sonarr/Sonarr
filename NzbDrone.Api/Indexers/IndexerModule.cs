using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Core.Indexers;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Indexers
{
    public class IndexerModule : NzbDroneRestModule<IndexerResource>
    {
        private readonly IIndexerService _indexerService;

        public IndexerModule(IIndexerService indexerService)
        {
            _indexerService = indexerService;
            GetResourceAll = GetAll;
        }

        private List<IndexerResource> GetAll()
        {
            var indexers = _indexerService.All();

            var result = new List<IndexerResource>(indexers.Count);

            foreach (var indexer in indexers)
            {
                var indexerResource = new IndexerResource();
                indexerResource.InjectFrom(indexer);
                indexerResource.Fields = SchemaBuilder.GenerateSchema(indexer.Settings);

                result.Add(indexerResource);
            }

            return result;
        }
    }
}