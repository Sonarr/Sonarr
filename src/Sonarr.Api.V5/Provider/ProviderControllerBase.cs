using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.ThingiProvider.Events;
using NzbDrone.Core.Validation;
using NzbDrone.SignalR;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.Provider
{
    public abstract class ProviderControllerBase<TProviderResource, TBulkProviderResource, TProvider, TProviderDefinition> : RestControllerWithSignalR<TProviderResource, TProviderDefinition>,
        IHandle<ProviderAddedEvent<TProvider>>,
        IHandle<ProviderUpdatedEvent<TProvider>>,
        IHandle<ProviderDeletedEvent<TProvider>>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
        where TProviderResource : ProviderResource<TProviderResource>, new()
        where TBulkProviderResource : ProviderBulkResource<TBulkProviderResource>, new()
    {
        private readonly IProviderFactory<TProvider, TProviderDefinition> _providerFactory;
        private readonly ProviderResourceMapper<TProviderResource, TProviderDefinition> _resourceMapper;
        private readonly ProviderBulkResourceMapper<TBulkProviderResource, TProviderDefinition> _bulkResourceMapper;

        protected ProviderControllerBase(IBroadcastSignalRMessage signalRBroadcaster,
            IProviderFactory<TProvider,
            TProviderDefinition> providerFactory,
            string resource,
            ProviderResourceMapper<TProviderResource, TProviderDefinition> resourceMapper,
            ProviderBulkResourceMapper<TBulkProviderResource, TProviderDefinition> bulkResourceMapper)
            : base(signalRBroadcaster)
        {
            _providerFactory = providerFactory;
            _resourceMapper = resourceMapper;
            _bulkResourceMapper = bulkResourceMapper;

            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Name).Must((v, c) => !_providerFactory.All().Any(p => p.Name.EqualsIgnoreCase(c) && p.Id != v.Id)).WithMessage("Should be unique");
            SharedValidator.RuleFor(c => c.Implementation).NotEmpty();
            SharedValidator.RuleFor(c => c.ConfigContract).NotEmpty();

            PostValidator.RuleFor(c => c.Fields).NotNull();
        }

        protected override TProviderResource GetResourceById(int id)
        {
            var definition = _providerFactory.Get(id);
            _providerFactory.SetProviderCharacteristics(definition);

            return _resourceMapper.ToResource(definition);
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<TProviderResource>> GetAll()
        {
            var providerDefinitions = _providerFactory.All();

            var result = new List<TProviderResource>(providerDefinitions.Count);

            foreach (var definition in providerDefinitions.OrderBy(p => p.ImplementationName))
            {
                _providerFactory.SetProviderCharacteristics(definition);

                result.Add(_resourceMapper.ToResource(definition));
            }

            return TypedResults.Ok(result.OrderBy(p => p.Name).ToList());
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Results<Created<TProviderResource>, NotFound> CreateProvider([FromBody] TProviderResource providerResource, [FromQuery] bool skipTesting = false, [FromQuery] SkipValidation skipValidation = SkipValidation.None)
        {
            var providerDefinition = GetDefinition(providerResource, null, skipValidation, false);

            if (providerDefinition.Enable && !skipTesting)
            {
                Test(providerDefinition, skipValidation);
            }

            providerDefinition = _providerFactory.Create(providerDefinition);

            return TypedCreated(providerDefinition.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Results<Accepted<TProviderResource>, NotFound> UpdateProvider([FromRoute] int id, [FromBody] TProviderResource providerResource, [FromQuery] bool skipTesting = false, [FromQuery] SkipValidation skipValidation = SkipValidation.None)
        {
            // TODO: Remove fallback to Id from body in next API version bump
            var existingDefinition = _providerFactory.Find(id) ?? _providerFactory.Find(providerResource.Id);

            if (existingDefinition == null)
            {
                return TypedResults.NotFound();
            }

            var providerDefinition = GetDefinition(providerResource, existingDefinition, skipValidation, false);

            // Compare settings separately because they are not serialized with the definition.
            var hasDefinitionChanged = !existingDefinition.Equals(providerDefinition) || !existingDefinition.Settings.Equals(providerDefinition.Settings);

            // Only test existing definitions if it is enabled, skipTesting isn't set and the definition has changed.
            if (providerDefinition.Enable && !skipTesting && hasDefinitionChanged)
            {
                Test(providerDefinition, skipValidation);
            }

            if (hasDefinitionChanged)
            {
                _providerFactory.Update(providerDefinition);
            }

            return TypedAccepted(existingDefinition.Id);
        }

        [HttpPut("bulk")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public virtual Results<Ok<IEnumerable<TProviderResource>>, BadRequest> UpdateProvider([FromBody] TBulkProviderResource providerResource)
        {
            if (!providerResource.Ids.Any())
            {
                throw new BadRequestException("ids must be provided");
            }

            var definitionsToUpdate = _providerFactory.Get(providerResource.Ids).ToList();

            foreach (var definition in definitionsToUpdate)
            {
                _providerFactory.SetProviderCharacteristics(definition);

                if (providerResource.Tags != null)
                {
                    var newTags = providerResource.Tags;
                    var applyTags = providerResource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => definition.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => definition.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            definition.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }
            }

            _bulkResourceMapper.UpdateModel(providerResource, definitionsToUpdate);

            return TypedResults.Ok(_providerFactory.Update(definitionsToUpdate).Select(x => _resourceMapper.ToResource(x)));
        }

        private TProviderDefinition GetDefinition(TProviderResource providerResource, TProviderDefinition? existingDefinition, SkipValidation skipValidation, bool forceValidate)
        {
            var definition = _resourceMapper.ToModel(providerResource, existingDefinition);

            if (skipValidation != SkipValidation.All && (definition.Enable || forceValidate))
            {
                Validate(definition, skipValidation);
            }

            return definition;
        }

        [RestDeleteById]
        public NoContent DeleteProvider(int id)
        {
            _providerFactory.Delete(id);

            return TypedResults.NoContent();
        }

        [HttpDelete("bulk")]
        [Consumes("application/json")]
        public virtual NoContent DeleteProviders([FromBody] TBulkProviderResource resource)
        {
            _providerFactory.Delete(resource.Ids);

            return TypedResults.NoContent();
        }

        [HttpGet("schema")]
        [Produces("application/json")]
        public Ok<List<TProviderResource>> GetTemplates()
        {
            var defaultDefinitions = _providerFactory.GetDefaultDefinitions().OrderBy(p => p.ImplementationName).ToList();

            var result = new List<TProviderResource>(defaultDefinitions.Count);

            foreach (var providerDefinition in defaultDefinitions)
            {
                var providerResource = _resourceMapper.ToResource(providerDefinition);
                var presetDefinitions = _providerFactory.GetPresetDefinitions(providerDefinition);

                providerResource.Presets = presetDefinitions
                    .Select(v => _resourceMapper.ToResource(v))
                    .ToList();

                result.Add(providerResource);
            }

            return TypedResults.Ok(result);
        }

        [SkipValidation(true, false)]
        [HttpPost("test")]
        [Consumes("application/json")]
        public NoContent Test([FromBody] TProviderResource providerResource, [FromQuery] SkipValidation skipValidation = SkipValidation.None)
        {
            var existingDefinition = providerResource.Id > 0 ? _providerFactory.Find(providerResource.Id) : null;
            var providerDefinition = GetDefinition(providerResource, existingDefinition, skipValidation, true);

            Test(providerDefinition, skipValidation);

            return TypedResults.NoContent();
        }

        [HttpPost("testall")]
        [Produces("application/json")]
        public Results<Ok<List<ProviderTestAllResult>>, BadRequest<List<ProviderTestAllResult>>> TestAll()
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

            return result.Any(c => !c.IsValid) ? TypedResults.BadRequest(result) : TypedResults.Ok(result);
        }

        [SkipValidation]
        [HttpPost("action/{name}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Results<ContentHttpResult, BadRequest> RequestAction([FromRoute] string name, [FromBody] TProviderResource providerResource)
        {
            var existingDefinition = providerResource.Id > 0 ? _providerFactory.Find(providerResource.Id) : null;
            var providerDefinition = GetDefinition(providerResource, existingDefinition, SkipValidation.All, false);

            var query = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            var data = _providerFactory.RequestAction(providerDefinition, name, query);

            return TypedResults.Content(data.ToJson(), "application/json");
        }

        [NonAction]
        public virtual void Handle(ProviderAddedEvent<TProvider> message)
        {
            BroadcastResourceChange(ModelAction.Created, message.Definition.Id);
        }

        [NonAction]
        public virtual void Handle(ProviderUpdatedEvent<TProvider> message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Definition.Id);
        }

        [NonAction]
        public virtual void Handle(ProviderDeletedEvent<TProvider> message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.ProviderId);
        }

        private void Validate(TProviderDefinition definition, SkipValidation skipValidation)
        {
            var validationResult = definition.Settings.Validate();

            VerifyValidationResult(validationResult, skipValidation);
        }

        protected virtual void Test(TProviderDefinition definition, SkipValidation skipValidation)
        {
            var validationResult = _providerFactory.Test(definition);

            VerifyValidationResult(validationResult, skipValidation);
        }

        protected void VerifyValidationResult(ValidationResult validationResult, SkipValidation skipValidation)
        {
            var result = validationResult as NzbDroneValidationResult ?? new NzbDroneValidationResult(validationResult.Errors);

            if (skipValidation == SkipValidation.None && (!result.IsValid || result.HasWarnings))
            {
                throw new ValidationException(result.Failures);
            }

            if (skipValidation == SkipValidation.Warnings && !result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
