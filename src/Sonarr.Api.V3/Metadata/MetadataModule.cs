using NzbDrone.Core.Extras.Metadata;

namespace Sonarr.Api.V3.Metadata
{
    public class MetadataModule : ProviderModuleBase<MetadataResource, IMetadata, MetadataDefinition>
    {
        public static readonly MetadataResourceMapper ResourceMapper = new MetadataResourceMapper();

        public MetadataModule(IMetadataFactory metadataFactory)
            : base(metadataFactory, "metadata", ResourceMapper)
        {
        }

        protected override void Validate(MetadataDefinition definition, bool includeWarnings)
        {
            if (!definition.Enable) return;
            base.Validate(definition, includeWarnings);
        }
    }
}