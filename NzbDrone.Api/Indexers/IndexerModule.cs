using System;
using System.Collections.Generic;
using System.Linq;
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
            CreateResource = Create;
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

        private IndexerResource Create(IndexerResource indexerResource)
        {
            var indexer = _indexerService.Schema()
                               .SingleOrDefault(i =>
                                        i.Implementation.Equals(indexerResource.Implementation,                                         
                                        StringComparison.InvariantCultureIgnoreCase));

            //TODO: How should be handle this error?
            if (indexer == null)
            {
                throw new InvalidOperationException();
            }

            indexer.Name = indexerResource.Name;
            indexer.Enable = indexerResource.Enable;
            indexer.Settings = (IIndexerSetting)SchemaDeserializer.DeserializeSchema(indexer.Settings, indexerResource.Fields);

            indexer = _indexerService.Create(indexer);
            indexerResource.Id = indexer.Id;

            return indexerResource;
        }
    }
}