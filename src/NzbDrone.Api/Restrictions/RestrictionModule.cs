using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Api.Mapping;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Restrictions;

namespace NzbDrone.Api.Restrictions
{
    public class RestrictionModule : NzbDroneRestModule<RestrictionResource>
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
            return _restrictionService.Get(id).InjectTo<RestrictionResource>();
        }

        private List<RestrictionResource> GetAll()
        {
            return ToListResource(_restrictionService.All);
        }

        private int Create(RestrictionResource resource)
        {
            return _restrictionService.Add(resource.InjectTo<Restriction>()).Id;
        }

        private void Update(RestrictionResource resource)
        {
            _restrictionService.Update(resource.InjectTo<Restriction>());
        }

        private void Delete(int id)
        {
            _restrictionService.Delete(id);
        }
    }
}
