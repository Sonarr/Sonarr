using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Nancy;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3
{
    public abstract class ProviderModuleBase<TProviderResource, TProvider, TProviderDefinition> : SonarrRestModule<TProviderResource>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
        where TProviderResource : ProviderResource, new()
    {
        private readonly IProviderFactory<TProvider, TProviderDefinition> _providerFactory;
        private readonly ProviderResourceMapper<TProviderResource, TProviderDefinition> _resourceMapper;

        protected ProviderModuleBase(IProviderFactory<TProvider, TProviderDefinition> providerFactory, string resource, ProviderResourceMapper<TProviderResource, TProviderDefinition> resourceMapper)
            : base(resource)
        {
            _providerFactory = providerFactory;
            _resourceMapper = resourceMapper;

            Get("schema",  x => GetTemplates());
            Post("test",  x => Test(ReadResourceFromRequest(true)));
            Post("testall",  x => TestAll());
            Post("action/{action}",  x => RequestAction(x.action, ReadResourceFromRequest(true, true)));

            GetResourceAll = GetAll;
            GetResourceById = GetProviderById;
            CreateResource = CreateProvider;
            UpdateResource = UpdateProvider;
            DeleteResource = DeleteProvider;

            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Name).Must((v, c) => !_providerFactory.All().Any(p => p.Name == c && p.Id != v.Id)).WithMessage("Should be unique");
            SharedValidator.RuleFor(c => c.Implementation).NotEmpty();
            SharedValidator.RuleFor(c => c.ConfigContract).NotEmpty();

            PostValidator.RuleFor(c => c.Fields).NotNull();
        }

        private TProviderResource GetProviderById(int id)
        {
            var definition = _providerFactory.Get(id);
            _providerFactory.SetProviderCharacteristics(definition);

            return _resourceMapper.ToResource(definition);
        }

        private List<TProviderResource> GetAll()
        {
            var providerDefinitions = _providerFactory.All().OrderBy(p => p.ImplementationName);

            var result = new List<TProviderResource>(providerDefinitions.Count());

            foreach (var definition in providerDefinitions)
            {
                _providerFactory.SetProviderCharacteristics(definition);

                result.Add(_resourceMapper.ToResource(definition));
            }

            return result.OrderBy(p => p.Name).ToList();
        }

        private int CreateProvider(TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource, true, false, false);

            if (providerDefinition.Enable)
            {
                Test(providerDefinition, false);
            }

            providerDefinition = _providerFactory.Create(providerDefinition);

            return providerDefinition.Id;
        }

        private void UpdateProvider(TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource, true, false, false);
            var forceSave = Request.GetBooleanQueryParameter("forceSave");

            // Only test existing definitions if it is enabled and forceSave isn't set.
            if (providerDefinition.Enable && !forceSave)
            {
                Test(providerDefinition, false);
            }

            _providerFactory.Update(providerDefinition);
        }

        private TProviderDefinition GetDefinition(TProviderResource providerResource, bool validate, bool includeWarnings, bool forceValidate)
        {
            var definition = _resourceMapper.ToModel(providerResource);

            if (validate && (definition.Enable || forceValidate))
            {
                Validate(definition, includeWarnings);
            }

            return definition;
        }

        private void DeleteProvider(int id)
        {
            _providerFactory.Delete(id);
        }

        private object GetTemplates()
        {
            var defaultDefinitions = _providerFactory.GetDefaultDefinitions().OrderBy(p => p.ImplementationName).ToList();

            var result = new List<TProviderResource>(defaultDefinitions.Count());

            foreach (var providerDefinition in defaultDefinitions)
            {
                var providerResource = _resourceMapper.ToResource(providerDefinition);
                var presetDefinitions = _providerFactory.GetPresetDefinitions(providerDefinition);

                providerResource.Presets = presetDefinitions.Select(v =>
                {
                    var presetResource = _resourceMapper.ToResource(v);

                    return presetResource as ProviderResource;
                }).ToList();

                result.Add(providerResource);
            }

            return result;
        }

        private object Test(TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource, true, true, true);

            Test(providerDefinition, true);

            return "{}";
        }

        private object TestAll()
        {
            var providerDefinitions = _providerFactory.All()
                                                      .Where(c => c.Settings.Validate().IsValid && c.Enable)
                                                      .ToList();
            var result = new List<ProviderTestAllResult>();

            foreach (var definition in providerDefinitions)
            {
                var validationFailures = new List<ValidationFailure>();

                validationFailures.AddRange(definition.Settings.Validate().Errors);
                validationFailures.AddRange(_providerFactory.Test(definition).Errors);

                result.Add(new ProviderTestAllResult
                           {
                               Id = definition.Id,
                               ValidationFailures = validationFailures
                });
            }

            return ResponseWithCode(result, result.Any(c => !c.IsValid) ? HttpStatusCode.BadRequest : HttpStatusCode.OK);
        }

        private object RequestAction(string action, TProviderResource providerResource)
        {
            var providerDefinition = GetDefinition(providerResource, false, false, false);

            var query = ((IDictionary<string, object>)Request.Query.ToDictionary()).ToDictionary(k => k.Key, k => k.Value.ToString());

            var data = _providerFactory.RequestAction(providerDefinition, action, query);
            Response resp = data.ToJson();
            resp.ContentType = "application/json";
            return resp;
        }

        private void Validate(TProviderDefinition definition, bool includeWarnings)
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
            var result = new NzbDroneValidationResult(validationResult.Errors);

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
