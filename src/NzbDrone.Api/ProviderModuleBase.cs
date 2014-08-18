using System;
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

            Get["schema"] = x => GetTemplates();
            Post["test"] = x => Test(ReadResourceFromRequest());

            GetResourceAll = GetAll;
            GetResourceById = GetProviderById;
            CreateResource = CreateProvider;
            UpdateResource = UpdateProvider;
            DeleteResource = DeleteProvider;

            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Name).Must((v,c) => !_providerFactory.All().Any(p => p.Name == c && p.Id != v.Id)).WithMessage("Should be unique");
            SharedValidator.RuleFor(c => c.Implementation).NotEmpty();
            SharedValidator.RuleFor(c => c.ConfigContract).NotEmpty();

            PostValidator.RuleFor(c => c.Fields).NotNull();
        }

        private TProviderResource GetProviderById(int id)
        {
            var definition = _providerFactory.Get(id);
            var resource = definition.InjectTo<TProviderResource>();

            resource.InjectFrom(_providerFactory.GetProviderCharacteristics(_providerFactory.GetInstance(definition), definition));

            return resource;
        }

        private List<TProviderResource> GetAll()
        {
            var providerDefinitions = _providerFactory.All();

            var result = new List<TProviderResource>(providerDefinitions.Count);

            foreach (var definition in providerDefinitions)
            {
                var providerResource = new TProviderResource();
                providerResource.InjectFrom(definition);
                providerResource.InjectFrom(_providerFactory.GetProviderCharacteristics(_providerFactory.GetInstance(definition), definition));
                providerResource.Fields = SchemaBuilder.ToSchema(definition.Settings);

                result.Add(providerResource);
            }

            return result;
        }

        private int CreateProvider(TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource);

            if (providerDefinition.Enable)
            {
                Test(providerDefinition);
            }

            providerDefinition = _providerFactory.Create(providerDefinition);

            return providerDefinition.Id;
        }

        private void UpdateProvider(TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource);

            if (providerDefinition.Enable)
            {
                Test(providerDefinition);
            }

            _providerFactory.Update(providerDefinition);
        }

        private TProviderDefinition GetDefinition(TProviderResource providerResource)
        {
            var definition = new TProviderDefinition();

            definition.InjectFrom(providerResource);

            var preset = _providerFactory.GetPresetDefinitions(definition)
                            .Where(v => v.Name == definition.Name)
                            .Select(v => v.Settings)
                            .FirstOrDefault();

            var configContract = ReflectionExtensions.CoreAssembly.FindTypeByName(definition.ConfigContract);
            definition.Settings = (IProviderConfig)SchemaBuilder.ReadFormSchema(providerResource.Fields, configContract, preset);

            Validate(definition);

            return definition;
        }

        private void DeleteProvider(int id)
        {
            _providerFactory.Delete(id);
        }

        private Response GetTemplates()
        {
            var defaultDefinitions = _providerFactory.GetDefaultDefinitions().ToList();

            var result = new List<TProviderResource>(defaultDefinitions.Count());

            foreach (var providerDefinition in defaultDefinitions)
            {
                var providerResource = new TProviderResource();
                providerResource.InjectFrom(providerDefinition);
                providerResource.Fields = SchemaBuilder.ToSchema(providerDefinition.Settings);
                providerResource.InfoLink = String.Format("https://github.com/NzbDrone/NzbDrone/wiki/Supported-{0}#{1}",
                    typeof(TProviderResource).Name.Replace("Resource", "s"),
                    providerDefinition.Implementation.ToLower());

                var presetDefinitions = _providerFactory.GetPresetDefinitions(providerDefinition);

                providerResource.Presets = presetDefinitions.Select(v =>
                {
                    var presetResource = new TProviderResource();
                    presetResource.InjectFrom(v);
                    presetResource.Fields = SchemaBuilder.ToSchema(v.Settings);

                    return presetResource as ProviderResource;
                }).ToList();

                result.Add(providerResource);
            }

            return result.AsResponse();
        }

        private Response Test(TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource);

            Test(providerDefinition);

            return "{}";
        }

        private void Test(TProviderDefinition providerDefinition)
        {
            var result = _providerFactory.Test(providerDefinition);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
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
