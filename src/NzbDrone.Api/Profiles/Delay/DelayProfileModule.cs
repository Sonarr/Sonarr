using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Sonarr.Http.REST;
using NzbDrone.Core.Profiles.Delay;
using Sonarr.Http;
using Sonarr.Http.Validation;

namespace NzbDrone.Api.Profiles.Delay
{
    public class DelayProfileModule : SonarrRestModule<DelayProfileResource>
    {
        private readonly IDelayProfileService _delayProfileService;

        public DelayProfileModule(IDelayProfileService delayProfileService, DelayProfileTagInUseValidator tagInUseValidator)
        {
            _delayProfileService = delayProfileService;

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
            CreateResource = Create;
            DeleteResource = DeleteProfile;

            SharedValidator.RuleFor(d => d.Tags).NotEmpty().When(d => d.Id != 1);
            SharedValidator.RuleFor(d => d.Tags).EmptyCollection<DelayProfileResource, int>().When(d => d.Id == 1);
            SharedValidator.RuleFor(d => d.Tags).SetValidator(tagInUseValidator);
            SharedValidator.RuleFor(d => d.UsenetDelay).GreaterThanOrEqualTo(0);
            SharedValidator.RuleFor(d => d.TorrentDelay).GreaterThanOrEqualTo(0);

            SharedValidator.Custom(delayProfile =>
            {
                if (!delayProfile.EnableUsenet && !delayProfile.EnableTorrent)
                {
                    return new ValidationFailure("", "Either Usenet or Torrent should be enabled");
                }

                return null;
            });
        }

        private int Create(DelayProfileResource resource)
        {
            var model = resource.ToModel();
            model = _delayProfileService.Add(model);

            return model.Id;
        }

        private void DeleteProfile(int id)
        {
            if (id == 1)
            {
                throw new MethodNotAllowedException("Cannot delete global delay profile");
            }

            _delayProfileService.Delete(id);
        }

        private void Update(DelayProfileResource resource)
        {
            var model = resource.ToModel();
            _delayProfileService.Update(model);
        }

        private DelayProfileResource GetById(int id)
        {
            return _delayProfileService.Get(id).ToResource();
        }

        private List<DelayProfileResource> GetAll()
        {
            return _delayProfileService.All().ToResource();
        }
    }
}