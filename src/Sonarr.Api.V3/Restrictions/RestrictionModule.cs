using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Restrictions;
using Sonarr.Http;

namespace Sonarr.Api.V3.Restrictions
{
    public class RestrictionModule : SonarrRestModule<RestrictionResource>
    {
        private readonly IRestrictionService _restrictionService;


        public RestrictionModule(IRestrictionService restrictionService)
        {
            _restrictionService = restrictionService;

            GetResourceById = Get;
            GetResourceAll = GetAll;
            CreateResource = Create;
            UpdateResource = Update;
            DeleteResource = Delete;

            SharedValidator.Custom(restriction =>
            {
                if (restriction.Ignored.IsNullOrWhiteSpace() && restriction.Required.IsNullOrWhiteSpace())
                {
                    return new ValidationFailure("", "Either 'Must contain' or 'Must not contain' is required");
                }

                return null;
            });
        }

        private RestrictionResource Get(int id)
        {
            return _restrictionService.Get(id).ToResource();
        }

        private List<RestrictionResource> GetAll()
        {
            return _restrictionService.All().ToResource();
        }

        private int Create(RestrictionResource resource)
        {
            return _restrictionService.Add(resource.ToModel()).Id;
        }

        private void Update(RestrictionResource resource)
        {
            _restrictionService.Update(resource.ToModel());
        }

        private void Delete(int id)
        {
            _restrictionService.Delete(id);
        }
    }
}
