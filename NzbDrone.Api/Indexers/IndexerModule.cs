using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ThingiProvider;
using Omu.ValueInjecter;
using FluentValidation;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Indexers
{
    public class IndexerModule : NzbDroneRestModule<IndexerResource>
    {
        private readonly IIndexerService _indexerService;

        public IndexerModule(IIndexerService indexerService)
        {
            _indexerService = indexerService;
            GetResourceAll = GetAll;
            GetResourceById = GetIndexer;
            CreateResource = CreateIndexer;
            UpdateResource = UpdateIndexer;
            DeleteResource = DeleteIndexer;


            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Implementation).NotEmpty();

            PostValidator.RuleFor(c => c.Fields).NotEmpty();
        }

        private IndexerResource GetIndexer(int id)
        {
            return _indexerService.Get(id).InjectTo<IndexerResource>();
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

        private int CreateIndexer(IndexerResource indexerResource)
        {
            var indexer = GetIndexer(indexerResource);
            indexer = _indexerService.Create(indexer);
            return indexer.Id;
        }

        private void UpdateIndexer(IndexerResource indexerResource)
        {
            var indexer = GetIndexer(indexerResource);

            ValidateIndexer(indexer.Settings);

            _indexerService.Update(indexer);
        }


        private static void ValidateIndexer(IProviderConfig config)
        {
            var validationResult = config.Validate();

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        private IndexerDefinition GetIndexer(IndexerResource indexerResource)
        {

            var definition = new IndexerDefinition();

            definition.InjectFrom(indexerResource);
            if (indexerResource.Enable)
            {
                ValidateIndexer(definition.Settings);
            }

            return definition;
        }

        private void DeleteIndexer(int id)
        {
            _indexerService.Delete(id);
        }
    }
}