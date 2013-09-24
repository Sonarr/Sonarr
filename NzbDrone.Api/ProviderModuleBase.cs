using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Nancy;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Indexers;
using NzbDrone.Api.Mapping;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.ThingiProvider;
using Omu.ValueInjecter;

namespace NzbDrone.Api
{
    public abstract class ProviderModuleBase<TProviderResource, TProvider, TProviderDefinition> : NzbDroneRestModule<TProviderResource>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
        where TProviderResource : ProviderResource, new()
    {
        private readonly IProviderFactory<TProvider, TProviderDefinition> _providerFactory;

        protected ProviderModuleBase(IProviderFactory<TProvider, TProviderDefinition> providerFactory)
        {
            _providerFactory = providerFactory;
            Get["templates"] = x => GetTemplates();
            GetResourceAll = GetAll;
            GetResourceById = GetProviderById;
            CreateResource = CreateProvider;
            UpdateResource = UpdateProvider;
            DeleteResource = DeleteProvider;



            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Implementation).NotEmpty();
            SharedValidator.RuleFor(c => c.ConfigContract).NotEmpty();

            PostValidator.RuleFor(c => c.Fields).NotEmpty();
        }

        private TProviderResource GetProviderById(int id)
        {
            return _providerFactory.Get(id).InjectTo<TProviderResource>();
        }

        private List<TProviderResource> GetAll()
        {
            var indexerDefinitions = _providerFactory.All();

            var result = new List<TProviderResource>(indexerDefinitions.Count);

            foreach (var definition in indexerDefinitions)
            {
                var indexerResource = new TProviderResource();
                indexerResource.InjectFrom(definition);
                indexerResource.Fields = SchemaBuilder.ToSchema(definition.Settings);

                result.Add(indexerResource);
            }

            return result;
        }

        private int CreateProvider(TProviderResource indexerResource)
        {
            var indexer = GetDefinition(indexerResource);
            indexer = _providerFactory.Create(indexer);
            return indexer.Id;
        }

        private void UpdateProvider(TProviderResource indexerResource)
        {
            var indexer = GetDefinition(indexerResource);

            ValidateIndexer(indexer);

            _providerFactory.Update(indexer);
        }


        private static void ValidateIndexer(ProviderDefinition definition)
        {
            if (!definition.Enable) return;

            var validationResult = definition.Settings.Validate();

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        private TProviderDefinition GetDefinition(TProviderResource indexerResource)
        {

            var definition = new TProviderDefinition();

            definition.InjectFrom(indexerResource);

            var configContract = ReflectionExtensions.CoreAssembly.FindTypeByName(definition.ConfigContract);
            definition.Settings = (IProviderConfig)SchemaBuilder.ReadFormSchema(indexerResource.Fields, configContract);

            ValidateIndexer(definition);

            return definition;
        }

        private void DeleteProvider(int id)
        {
            _providerFactory.Delete(id);
        }

        private Response GetTemplates()
        {

            var indexers = _providerFactory.Templates();


            var result = new List<IndexerResource>(indexers.Count());

            foreach (var indexer in indexers)
            {
                var indexerResource = new IndexerResource();
                indexerResource.InjectFrom(indexer);
                indexerResource.Fields = SchemaBuilder.ToSchema(indexer.Settings);

                result.Add(indexerResource);
            }

            return result.AsResponse();
        }
    }
}