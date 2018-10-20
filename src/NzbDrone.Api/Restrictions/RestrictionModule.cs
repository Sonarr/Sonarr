using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Profiles.Releases;
using Sonarr.Http;
using Sonarr.Http.Mapping;

namespace NzbDrone.Api.Restrictions
{
    public class RestrictionModule : SonarrRestModule<RestrictionResource>
    {
        private readonly IReleaseProfileService _releaseProfileService;


        public RestrictionModule(IReleaseProfileService releaseProfileService)
        {
            _releaseProfileService = releaseProfileService;

            GetResourceById = GetRestriction;
            GetResourceAll = GetAllRestrictions;
            CreateResource = CreateRestriction;
            UpdateResource = UpdateRestriction;
            DeleteResource = DeleteRestriction;

            SharedValidator.Custom(restriction =>
            {
                if (restriction.Ignored.IsNullOrWhiteSpace() && restriction.Required.IsNullOrWhiteSpace())
                {
                    return new ValidationFailure("", "Either 'Must contain' or 'Must not contain' is required");
                }

                return null;
            });
        }

        private RestrictionResource GetRestriction(int id)
        {
            return _releaseProfileService.Get(id).ToResource();
        }

        private List<RestrictionResource> GetAllRestrictions()
        {
            return _releaseProfileService.All().ToResource();
        }

        private int CreateRestriction(RestrictionResource resource)
        {
            return _releaseProfileService.Add(resource.ToModel()).Id;
        }

        private void UpdateRestriction(RestrictionResource resource)
        {
            _releaseProfileService.Update(resource.ToModel());
        }

        private void DeleteRestriction(int id)
        {
            _releaseProfileService.Delete(id);
        }
    }
}
