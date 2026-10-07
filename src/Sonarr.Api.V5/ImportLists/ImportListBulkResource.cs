using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Languages;
using Sonarr.Api.V5.Provider;

namespace Sonarr.Api.V5.ImportLists;

public class ImportListBulkResource : ProviderBulkResource<ImportListBulkResource>
{
    public bool? EnableAutomaticAdd { get; set; }
    public string? RootFolderPath { get; set; }
    public int? QualityProfileId { get; set; }
    public Language? Language { get; set; }
    public TagExistingType? TagExisting { get; set; }
}

public class ImportListBulkResourceMapper : ProviderBulkResourceMapper<ImportListBulkResource, ImportListDefinition>
{
    public override List<ImportListDefinition> UpdateModel(ImportListBulkResource resource, List<ImportListDefinition> existingDefinitions)
    {
        existingDefinitions.ForEach(existing =>
        {
            existing.EnableAutomaticAdd = resource.EnableAutomaticAdd ?? existing.EnableAutomaticAdd;
            existing.RootFolderPath = resource.RootFolderPath ?? existing.RootFolderPath;
            existing.QualityProfileId = resource.QualityProfileId ?? existing.QualityProfileId;
            existing.Language = resource.Language ?? existing.Language;
            existing.TagExisting = resource.TagExisting ?? existing.TagExisting;
        });

        return existingDefinitions;
    }
}
