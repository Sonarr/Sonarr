using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Nancy;
using Sonarr.Http.Extensions;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using Newtonsoft.Json;
using Sonarr.Http;
using Sonarr.Http.ClientSchema;
using Sonarr.Http.Mapping;

namespace NzbDrone.Api
{
    public abstract class ProviderModuleBase<TProviderResource, TProvider, TProviderDefinition> : SonarrRestModule<TProviderResource>
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
            Post["test"] = x => Test(ReadResourceFromRequest(true));
            Post["action/{action}"] = x => RequestAction(x.action, ReadResourceFromRequest(true));

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
            _providerFactory.SetProviderCharacteristics(definition);

            var resource = new TProviderResource();
            MapToResource(resource, definition);

            return resource;
        }

        private List<TProviderResource> GetAll()
        {
            var providerDefinitions = _providerFactory.All().OrderBy(p => p.ImplementationName);

            var result = new List<TProviderResource>(providerDefinitions.Count());

            foreach (var definition in providerDefinitions)
            {
                _providerFactory.SetProviderCharacteristics(definition);

                var providerResource = new TProviderResource();
                MapToResource(providerResource, definition);

                result.Add(providerResource);
            }

            return result.OrderBy(p => p.Name).ToList();
        }

        private int CreateProvider(TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource, false);

            if (providerDefinition.Enable)
            {
                Test(providerDefinition, false);
            }

            providerDefinition = _providerFactory.Create(providerDefinition);

            return providerDefinition.Id;
        }

        private void UpdateProvider(TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource, false);

            _providerFactory.Update(providerDefinition);
        }

        private TProviderDefinition GetDefinition(TProviderResource providerResource, bool includeWarnings = false, bool validate = true)
        {
            var definition = new TProviderDefinition();

            MapToModel(definition, providerResource);

            if (validate)
            {
                Validate(definition, includeWarnings);
            }

            return definition;
        }

        protected virtual void MapToResource(TProviderResource resource, TProviderDefinition definition)
        {
            resource.Id = definition.Id;

            resource.Name = definition.Name;
            resource.ImplementationName = definition.ImplementationName;
            resource.Implementation = definition.Implementation;
            resource.ConfigContract = definition.ConfigContract;
            resource.Message = definition.Message;

            resource.Fields = SchemaBuilder.ToSchema(definition.Settings);

            resource.InfoLink = string.Format("https://github.com/Sonarr/Sonarr/wiki/Supported-{0}#{1}",
                typeof(TProviderResource).Name.Replace("Resource", "s"),
                definition.Implementation.ToLower());
        }

        protected virtual void MapToModel(TProviderDefinition definition, TProviderResource resource)
        {
            definition.Id = resource.Id;

            definition.Name = resource.Name;
            definition.ImplementationName = resource.ImplementationName;
            definition.Implementation = resource.Implementation;
            definition.ConfigContract = resource.ConfigContract;
            definition.Message = resource.Message;

            var configContract = ReflectionExtensions.CoreAssembly.FindTypeByName(definition.ConfigContract);
            definition.Settings = (IProviderConfig)SchemaBuilder.ReadFromSchema(resource.Fields, configContract);
        }

        private void DeleteProvider(int id)
        {
            _providerFactory.Delete(id);
        }

        private Response GetTemplates()
        {
            var defaultDefinitions = _providerFactory.GetDefaultDefinitions().OrderBy(p => p.ImplementationName).ToList();

            var result = new List<TProviderResource>(defaultDefinitions.Count());

            foreach (var providerDefinition in defaultDefinitions)
            {
                var providerResource = new TProviderResource();
                MapToResource(providerResource, providerDefinition);

                var presetDefinitions = _providerFactory.GetPresetDefinitions(providerDefinition);

                providerResource.Presets = presetDefinitions.Select(v =>
                {
                    var presetResource = new TProviderResource();
                    MapToResource(presetResource, v);

                    return presetResource as ProviderResource;
                }).ToList();

                result.Add(providerResource);
            }

            return result.AsResponse();
        }

        private Response Test(TProviderResource providerResource)
        {
            // Don't validate when getting the definition so we can validate afterwards (avoids validation being skipped because the provider is disabled)
            var providerDefinition = GetDefinition(providerResource, true, false);

            Validate(providerDefinition, true);
            Test(providerDefinition, true);

            return "{}";
        }


        private Response RequestAction(string action, TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource, true, false);

            var query = ((IDictionary<string, object>)Request.Query.ToDictionary()).ToDictionary(k => k.Key, k => k.Value.ToString());

            var data = _providerFactory.RequestAction(providerDefinition, action, query);
            Response resp = JsonConvert.SerializeObject(data);
            resp.ContentType = "application/json";
            return resp;
        }

        protected virtual void Validate(TProviderDefinition definition, bool includeWarnings)
        {
            var validationResult = definition.Settings.Validate();

            VerifyValidationResult(validationResult, includeWarnings);
        }

        protected virtual void Test(TProviderDefinition definition, bool includeWarnings)
        {
            var validationResult = _providerFactory.Test(definition);

            VerifyValidationResult(validationResult, includeWarnings);
        }

        protected void VerifyValidationResult(ValidationResult validationResult, bool includeWarnings)
        {
            var result = validationResult as NzbDroneValidationResult;

            if (result == null)
            {
                result = new NzbDroneValidationResult(validationResult.Errors);
            }

            if (includeWarnings && (!result.IsValid || result.HasWarnings))
            {
                throw new ValidationException(result.Failures);
            }

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
