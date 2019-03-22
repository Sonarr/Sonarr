using System.Collections.Generic;
using NzbDrone.Core.CustomFilters;
using Sonarr.Http;

namespace Sonarr.Api.V3.CustomFilters
{
    public class CustomFilterModule : SonarrRestModule<CustomFilterResource>
    {
        private readonly ICustomFilterService _customFilterService;

        public CustomFilterModule(ICustomFilterService customFilterService)
        {
            _customFilterService = customFilterService;

            GetResourceById = GetCustomFilter;
            GetResourceAll = GetCustomFilters;
            CreateResource = AddCustomFilter;
            UpdateResource = UpdateCustomFilter;
            DeleteResource = DeleteCustomResource;
        }

        private CustomFilterResource GetCustomFilter(int id)
        {
            return _customFilterService.Get(id).ToResource();
        }

        private List<CustomFilterResource> GetCustomFilters()
        {
            return _customFilterService.All().ToResource();
        }

        private int AddCustomFilter(CustomFilterResource resource)
        {
            var customFilter = _customFilterService.Add(resource.ToModel());

            return customFilter.Id;
        }

        private void UpdateCustomFilter(CustomFilterResource resource)
        {
            _customFilterService.Update(resource.ToModel());
        }

        private void DeleteCustomResource(int id)
        {
            _customFilterService.Delete(id);
        }
    }
}
