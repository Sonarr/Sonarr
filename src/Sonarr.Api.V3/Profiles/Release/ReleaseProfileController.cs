using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles.Releases;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Profiles.Release
{
    [V3ApiController]
    public class ReleaseProfileController : RestController<ReleaseProfileResource>
    {
        private readonly IReleaseProfileService _profileService;
        private readonly IIndexerFactory _indexerFactory;

        public ReleaseProfileController(IReleaseProfileService profileService, IIndexerFactory indexerFactory)
        {
            _profileService = profileService;
            _indexerFactory = indexerFactory;

            SharedValidator.RuleFor(d => d).Custom((restriction, context) =>
            {
                if (restriction.MapIgnored().Empty() && restriction.MapRequired().Empty() && restriction.Preferred.Empty())
                {
                    context.AddFailure("'Must contain', 'Must not contain' or 'Preferred' is required");
                }

                if (restriction.Enabled && restriction.IndexerId != 0 && !_indexerFactory.Exists(restriction.IndexerId))
                {
                    context.AddFailure(nameof(ReleaseProfile.IndexerId), "Indexer does not exist");
                }

                if (restriction.Preferred.Any(p => p.Key.IsNullOrWhiteSpace()))
                {
                    context.AddFailure("Preferred", "Term cannot be empty or consist of only spaces");
                }
            });
        }

        [RestPostById]
        public ActionResult<ReleaseProfileResource> Create(ReleaseProfileResource resource)
        {
            var model = resource.ToModel();
            model = _profileService.Add(model);
            return Created(model.Id);
        }

        [RestDeleteById]
        public void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        [RestPutById]
        public ActionResult<ReleaseProfileResource> Update(ReleaseProfileResource resource)
        {
            var model = resource.ToModel();

            _profileService.Update(model);

            return Accepted(model.Id);
        }

        protected override ReleaseProfileResource GetResourceById(int id)
        {
            return _profileService.Get(id).ToResource();
        }

        [HttpGet]
        public List<ReleaseProfileResource> GetAll()
        {
            return _profileService.All().ToResource();
        }
    }
}
