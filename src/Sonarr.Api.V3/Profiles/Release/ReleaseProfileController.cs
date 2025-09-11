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
        private readonly IReleaseProfileService _profileService;
        private readonly IIndexerFactory _indexerFactory;
        private readonly ITagService _tagService;

        public ReleaseProfileController(IReleaseProfileService profileService, IIndexerFactory indexerFactory, ITagService tagService)
        {
            _profileService = profileService;
            _indexerFactory = indexerFactory;
            _tagService = tagService;

            SharedValidator.RuleFor(d => d).Custom((restriction, context) =>
            {
                if (restriction.MapRequired().Empty() && restriction.MapIgnored().Empty())
                {
                    context.AddFailure(nameof(ReleaseProfile.Required), "'Must contain' or 'Must not contain' is required");
                }

                if (restriction.MapRequired().Any(t => t.IsNullOrWhiteSpace()))
                {
                    context.AddFailure(nameof(ReleaseProfile.Required), "'Must contain' should not contain whitespaces or an empty string");
                }

                if (restriction.MapIgnored().Any(t => t.IsNullOrWhiteSpace()))
                {
                    context.AddFailure(nameof(ReleaseProfile.Ignored), "'Must not contain' should not contain whitespaces or an empty string");
                }

                if (restriction.Enabled && restriction.IndexerId != 0 && !_indexerFactory.Exists(restriction.IndexerId))
                {
                    context.AddFailure(nameof(ReleaseProfile.IndexerId), "Indexer does not exist");
                }
            });

            SharedValidator.RuleFor(d => d.Tags.Intersect(d.ExcludedTags))
                .Empty()
                .WithName("ExcludedTags")
                .WithMessage(d => $"'{string.Join(", ", _tagService.GetTags(d.Tags.Intersect(d.ExcludedTags)).Select(t => t.Label))}' cannot be in both 'Tags' and 'Excluded Tags'");
        }

        [RestPostById]
        public ActionResult<ReleaseProfileResource> Create([FromBody] ReleaseProfileResource resource)
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
        public ActionResult<ReleaseProfileResource> Update([FromBody] ReleaseProfileResource resource)
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
