using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Tv;

namespace Sonarr.Api.V3.ImportLists
{
    public class ImportListResource : ProviderResource
    {
        public bool EnableAutomaticAdd { get; set; }
        public MonitorTypes ShouldMonitor { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public int LanguageProfileId { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public bool SeasonFolder { get; set; }
        public ImportListType ListType { get; set; }
        public int ListOrder { get; set; }
    }

    public class ImportListResourceMapper : ProviderResourceMapper<ImportListResource, ImportListDefinition>
    {
        public override ImportListResource ToResource(ImportListDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            var resource = base.ToResource(definition);

            resource.EnableAutomaticAdd = definition.EnableAutomaticAdd;
            resource.ShouldMonitor = definition.ShouldMonitor;
            resource.RootFolderPath = definition.RootFolderPath;
            resource.QualityProfileId = definition.QualityProfileId;
            resource.LanguageProfileId = definition.LanguageProfileId;
            resource.SeriesType = definition.SeriesType;
            resource.SeasonFolder = definition.SeasonFolder;
            resource.ListType = definition.ListType;
            resource.ListOrder = (int)definition.ListType;

            return resource;
        }

        public override ImportListDefinition ToModel(ImportListResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            var definition = base.ToModel(resource);

            definition.EnableAutomaticAdd = resource.EnableAutomaticAdd;
            definition.ShouldMonitor = resource.ShouldMonitor;
            definition.RootFolderPath = resource.RootFolderPath;
            definition.QualityProfileId = resource.QualityProfileId;
            definition.LanguageProfileId = resource.LanguageProfileId;
            definition.SeriesType = resource.SeriesType;
            definition.SeasonFolder = resource.SeasonFolder;
            definition.ListType = resource.ListType;

            return definition;
        }
    }
}
