using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Nancy;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.Extensions;
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

        protected ProviderModuleBase(IProviderFactory<TProvider, TProviderDefinition> providerFactory, string resource)
            : base(resource)
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
            var providerDefinitions = _providerFactory.All();

            var result = new List<TProviderResource>(providerDefinitions.Count);

            foreach (var definition in providerDefinitions)
            {
                var providerResource = new TProviderResource();
                providerResource.InjectFrom(definition);
                providerResource.Fields = SchemaBuilder.ToSchema(definition.Settings);

                result.Add(providerResource);
            }

            return result;
        }

        private int CreateProvider(TProviderResource providerResource)
        {
            var provider = GetDefinition(providerResource);
            provider = _providerFactory.Create(provider);
            return provider.Id;
        }

        private void UpdateProvider(TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource);

            Validate(providerDefinition);

            _providerFactory.Update(providerDefinition);
        }

        private TProviderDefinition GetDefinition(TProviderResource providerResource)
        {
            var definition = new TProviderDefinition();

            definition.InjectFrom(providerResource);

            var configContract = ReflectionExtensions.CoreAssembly.FindTypeByName(definition.ConfigContract);
            definition.Settings = (IProviderConfig)SchemaBuilder.ReadFormSchema(providerResource.Fields, configContract);

            Validate(definition);

            return definition;
        }

        private void DeleteProvider(int id)
        {
            _providerFactory.Delete(id);
        }

        private Response GetTemplates()
        {
            var templates = _providerFactory.Templates();

            var result = new List<TProviderResource>(templates.Count());

            foreach (var providerDefinition in templates)
            {
                var providerResource = new TProviderResource();
                providerResource.InjectFrom(providerDefinition);
                providerResource.Fields = SchemaBuilder.ToSchema(providerDefinition.Settings);

                result.Add(providerResource);
            }

            return result.AsResponse();
        }

        protected virtual void Validate(TProviderDefinition definition)
        {
            var validationResult = definition.Settings.Validate();

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}