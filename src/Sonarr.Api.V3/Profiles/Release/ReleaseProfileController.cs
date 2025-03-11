using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Tags;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Profiles.Release
{
    [V3ApiController]
    public class ReleaseProfileController : RestController<ReleaseProfileResource>
    {
        private readonly IReleaseProfileService _releaseProfileService;

        public ReleaseProfileController(IReleaseProfileService releaseProfileService, IIndexerFactory indexerFactory, ITagService tagService)
        {
            _releaseProfileService = releaseProfileService;

            SharedValidator.RuleFor(d => d).Custom((restriction, context) =>
            {
                if (restriction.MapRequired().Empty() && restriction.MapIgnored().Empty())
                {
                    context.AddFailure(nameof(ReleaseProfileResource.Required), "'Must contain' or 'Must not contain' is required");
                }

                if (restriction.MapRequired().Any(t => t.IsNullOrWhiteSpace()))
                {
                    context.AddFailure(nameof(ReleaseProfileResource.Required), "'Must contain' should not contain whitespaces or an empty string");
                }

                if (restriction.MapIgnored().Any(t => t.IsNullOrWhiteSpace()))
                {
                    context.AddFailure(nameof(ReleaseProfileResource.Ignored), "'Must not contain' should not contain whitespaces or an empty string");
                }

                if (restriction.Enabled && restriction.IndexerId != 0 && !indexerFactory.Exists(restriction.IndexerId))
                {
                    context.AddFailure(nameof(ReleaseProfileResource.IndexerId), "Indexer does not exist");
                }
            });

            SharedValidator.RuleFor(d => d.Tags.Intersect(d.ExcludedTags))
                .Empty()
                .WithName("ExcludedTags")
                .WithMessage(d => $"'{string.Join(", ", tagService.GetTags(d.Tags.Intersect(d.ExcludedTags)).Select(t => t.Label))}' cannot be in both 'Tags' and 'Excluded Tags'");
        }

        [RestPostById]
        public ActionResult<ReleaseProfileResource> Create([FromBody] ReleaseProfileResource resource)
        {
            var model = resource.ToModel();
            model = _releaseProfileService.Add(model);
            return Created(model.Id);
        }

        [RestDeleteById]
        public void DeleteProfile(int id)
        {
            _releaseProfileService.Delete(id);
        }

        [RestPutById]
        public ActionResult<ReleaseProfileResource> Update([FromBody] ReleaseProfileResource resource)
        {
            var model = resource.ToModel();

            _releaseProfileService.Update(model);

            return Accepted(model.Id);
        }

        protected override ReleaseProfileResource GetResourceById(int id)
        {
            return _releaseProfileService.Get(id).ToResource();
        }

        [HttpGet]
        public List<ReleaseProfileResource> GetAll()
        {
            return _releaseProfileService.All().ToResource();
        }
    }
}
