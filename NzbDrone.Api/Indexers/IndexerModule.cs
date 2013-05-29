using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.REST;
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

            if (indexer == null)
            {
                throw new BadRequestException("Invalid Notification Implementation");
            }

            indexer.Name = indexerResource.Name;
            indexer.Enable = indexerResource.Enable;
            indexer.Settings = SchemaDeserializer.DeserializeSchema(indexer.Settings, indexerResource.Fields);

            indexer = _indexerService.Create(indexer);

            var responseResource = new IndexerResource();
            responseResource.InjectFrom(indexer);
            responseResource.Fields = SchemaBuilder.GenerateSchema(indexer.Settings);

            return responseResource;
        }
    }
}