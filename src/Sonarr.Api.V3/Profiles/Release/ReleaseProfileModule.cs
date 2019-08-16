using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Profiles.Releases;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Release
{
    public class ReleaseProfileModule : SonarrRestModule<ReleaseProfileResource>
    {
        private readonly IReleaseProfileService _releaseProfileService;


        public ReleaseProfileModule(IReleaseProfileService releaseProfileService)
        {
            _releaseProfileService = releaseProfileService;

            GetResourceById = GetReleaseProfile;
            GetResourceAll = GetAll;
            CreateResource = Create;
            UpdateResource = Update;
            DeleteResource = DeleteReleaseProfile;

            SharedValidator.RuleFor(d => d).Custom((restriction, context) =>
            {
                if (restriction.Ignored.IsNullOrWhiteSpace() && restriction.Required.IsNullOrWhiteSpace() && restriction.Preferred.Empty())
                {
                    context.AddFailure("'Must contain', 'Must not contain' or 'Preferred' is required");
                }
            });
        }

        private ReleaseProfileResource GetReleaseProfile(int id)
        {
            return _releaseProfileService.Get(id).ToResource();
        }

        private List<ReleaseProfileResource> GetAll()
        {
            return _releaseProfileService.All().ToResource();
        }

        private int Create(ReleaseProfileResource resource)
        {
            return _releaseProfileService.Add(resource.ToModel()).Id;
        }

        private void Update(ReleaseProfileResource resource)
        {
            _releaseProfileService.Update(resource.ToModel());
        }

        private void DeleteReleaseProfile(int id)
        {
            _releaseProfileService.Delete(id);
        }
    }
}
