using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Profiles.Delay;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;
using Sonarr.Http.Validation;
using Swashbuckle.AspNetCore.Annotations;

namespace Sonarr.Api.V3.Profiles.Delay
{
    [V3ApiController]
    public class DelayProfileController : RestController<DelayProfileResource>
    {
        private readonly IDelayProfileService _delayProfileService;

        public DelayProfileController(IDelayProfileService delayProfileService, DelayProfileTagInUseValidator tagInUseValidator)
        {
            _delayProfileService = delayProfileService;

            SharedValidator.RuleFor(d => d.Tags).NotEmpty().When(d => d.Id != 1);
            SharedValidator.RuleFor(d => d.Tags).EmptyCollection<DelayProfileResource, int>().When(d => d.Id == 1);
            SharedValidator.RuleFor(d => d.Tags).SetValidator(tagInUseValidator);
            SharedValidator.RuleFor(d => d.UsenetDelay).GreaterThanOrEqualTo(0);
            SharedValidator.RuleFor(d => d.TorrentDelay).GreaterThanOrEqualTo(0);

            SharedValidator.RuleFor(d => d).Custom((delayProfile, context) =>
            {
                if (!delayProfile.EnableUsenet && !delayProfile.EnableTorrent)
                {
                    context.AddFailure("Either Usenet or Torrent should be enabled");
                }
            });
        }

        [RestPostById]
        [Consumes("application/json")]
        [SwaggerOperation(Summary = "Creates a delay profile.")]
        public ActionResult<DelayProfileResource> Create([FromBody] DelayProfileResource resource)
        {
            var model = resource.ToModel();
            model = _delayProfileService.Add(model);

            return Created(model.Id);
        }

        [RestDeleteById]
        [SwaggerOperation(Summary = "Deletes a delay profile.")]
        public void DeleteProfile(int id)
        {
            if (id == 1)
            {
                throw new MethodNotAllowedException("Cannot delete global delay profile");
            }

            _delayProfileService.Delete(id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [SwaggerOperation(Summary = "Updates a delay profile.")]
        public ActionResult<DelayProfileResource> Update([FromBody] DelayProfileResource resource)
        {
            var model = resource.ToModel();
            _delayProfileService.Update(model);
            return Accepted(model.Id);
        }

        protected override DelayProfileResource GetResourceById(int id)
        {
            return _delayProfileService.Get(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Gets a list of delay profiles.")]
        public List<DelayProfileResource> GetAll()
        {
            return _delayProfileService.All().ToResource();
        }

        [HttpPut("reorder/{id}")]
        [SwaggerOperation(Summary = "Updates order for delay profile.")]
        public List<DelayProfileResource> Reorder([FromRoute] int id, [FromQuery] int? after)
        {
            ValidateId(id);

            return _delayProfileService.Reorder(id, after).ToResource();
        }
    }
}
