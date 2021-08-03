using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.Validation;
using Sonarr.Http;

namespace Sonarr.Api.V3.ImportLists
{
    public class ImportListExclusionModule : SonarrRestModule<ImportListExclusionResource>
    {
        private readonly IImportListExclusionService _importListExclusionService;

        public ImportListExclusionModule(IImportListExclusionService importListExclusionService,
                                         ImportListExclusionExistsValidator importListExclusionExistsValidator)
        {
            _importListExclusionService = importListExclusionService;

            GetResourceById = GetImportListExclusion;
            GetResourceAll = GetImportListExclusions;
            CreateResource = AddImportListExclusion;
            UpdateResource = UpdateImportListExclusion;
            DeleteResource = DeleteImportListExclusionResource;

            SharedValidator.RuleFor(c => c.TvdbId).NotEmpty().SetValidator(importListExclusionExistsValidator);
            SharedValidator.RuleFor(c => c.Title).NotEmpty();
        }

        private ImportListExclusionResource GetImportListExclusion(int id)
        {
            return _importListExclusionService.Get(id).ToResource();
        }

        private List<ImportListExclusionResource> GetImportListExclusions()
        {
            return _importListExclusionService.All().ToResource();
        }

        private int AddImportListExclusion(ImportListExclusionResource resource)
        {
            var customFilter = _importListExclusionService.Add(resource.ToModel());

            return customFilter.Id;
        }

        private void UpdateImportListExclusion(ImportListExclusionResource resource)
        {
            _importListExclusionService.Update(resource.ToModel());
        }

        private void DeleteImportListExclusionResource(int id)
        {
            _importListExclusionService.Delete(id);
        }
    }
}
