using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.ClientSchema;
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

            var indexers = _indexerService.Templates().Where(c => c.Implementation =="Newznab");


            var result = new List<IndexerResource>(indexers.Count());

            foreach (var indexer in indexers)
            {
                var indexerResource = new IndexerResource();
                indexerResource.InjectFrom(indexer);
                indexerResource.Fields = SchemaBuilder.ToSchema(indexer.Settings);

                result.Add(indexerResource);
            }

            return result;
        }
    }
}