using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.REST;
using NzbDrone.Core.Indexers;
using Omu.ValueInjecter;
using FluentValidation;

namespace NzbDrone.Api.Indexers
{
    public class IndexerModule : NzbDroneRestModule<IndexerResource>
    {
        private readonly IIndexerService _indexerService;

        public IndexerModule(IIndexerService indexerService)
        {
            _indexerService = indexerService;
            GetResourceAll = GetAll;
            CreateResource = CreateIndexer;
            UpdateResource = UpdateIndexer;
            DeleteResource = DeleteIndexer;


            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Implementation).NotEmpty();

            PostValidator.RuleFor(c => c.Fields).NotEmpty();
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

        private IndexerResource CreateIndexer(IndexerResource indexerResource)
        {
            var indexer = GetIndexer(indexerResource);
            indexer = _indexerService.Create(indexer);

            var response = indexer.InjectTo<IndexerResource>();
            response.Fields = SchemaBuilder.GenerateSchema(indexer.Settings);

            return response;
        }

        private IndexerResource UpdateIndexer(IndexerResource indexerResource)
        {
            var indexer = _indexerService.Get(indexerResource.Id);
            indexer.InjectFrom(indexerResource);
            indexer.Settings = SchemaDeserializer.DeserializeSchema(indexer.Settings, indexerResource.Fields);

            ValidateSetting(indexer.Settings);

            indexer = _indexerService.Update(indexer);

            var response = indexer.InjectTo<IndexerResource>();
            response.Fields = SchemaBuilder.GenerateSchema(indexer.Settings);

            return response;
        }


        private static void ValidateSetting(IIndexerSetting setting)
        {
            var validationResult = setting.Validate();

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        private Indexer GetIndexer(IndexerResource indexerResource)
        {
            var indexer = _indexerService.Schema()
                               .SingleOrDefault(i =>
                                        i.Implementation.Equals(indexerResource.Implementation,
                                        StringComparison.InvariantCultureIgnoreCase));

            if (indexer == null)
            {
                throw new BadRequestException("Invalid Indexer Implementation");
            }

            indexer.InjectFrom(indexerResource);
            indexer.Settings = SchemaDeserializer.DeserializeSchema(indexer.Settings, indexerResource.Fields);

            ValidateSetting(indexer.Settings);

            return indexer;
        }

        private void DeleteIndexer(int id)
        {
            _indexerService.Delete(id);
        }
    }
}