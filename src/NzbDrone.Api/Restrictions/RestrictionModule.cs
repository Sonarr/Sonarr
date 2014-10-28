using System;
using System.Collections.Generic;
using NzbDrone.Api.Mapping;
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
        }

        private RestrictionResource Get(Int32 id)
        {
            return _restrictionService.Get(id).InjectTo<RestrictionResource>();
        }

        private List<RestrictionResource> GetAll()
        {
            return ToListResource(_restrictionService.All);
        }

        private Int32 Create(RestrictionResource resource)
        {
            return _restrictionService.Add(resource.InjectTo<Restriction>()).Id;
        }

        private void Update(RestrictionResource resource)
        {
            _restrictionService.Update(resource.InjectTo<Restriction>());
        }

        private void Delete(Int32 id)
        {
            _restrictionService.Delete(id);
        }
    }
}
