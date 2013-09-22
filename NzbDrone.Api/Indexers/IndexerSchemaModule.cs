using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.Indexers;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Indexers
{
    public class IndexerSchemaModule : NzbDroneRestModule<IndexerResource>
    {
        private readonly IIndexerService _indexerService;

        public IndexerSchemaModule(IIndexerService indexerService)
            : base("indexer/schema")
        {
            _indexerService = indexerService;
            GetResourceAll = GetSchema;
        }

        private List<IndexerResource> GetSchema()
        {

            var indexers = _indexerService.All().InjectTo<List<IndexerResource>>();

            /*         var indexers = _indexerService.Schema();

                     var result = new List<IndexerResource>(indexers.Count);

                     foreach (var indexer in indexers)
                     {
                         var indexerResource = new IndexerResource();
                         indexerResource.InjectFrom(indexer);
                         indexerResource.Fields = SchemaBuilder.GenerateSchema(indexer.Settings);

                         result.Add(indexerResource);
                     }

                     return result;*/

            return indexers;
        }
    }
}