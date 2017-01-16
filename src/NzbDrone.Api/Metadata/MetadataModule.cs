using NzbDrone.Core.Extras.Metadata;

namespace NzbDrone.Api.Metadata
{
    public class MetadataModule : ProviderModuleBase<MetadataResource, IMetadata, MetadataDefinition>
    {
        public MetadataModule(IMetadataFactory metadataFactory)
            : base(metadataFactory, "metadata")
        {
        }

        protected override void MapToResource(MetadataResource resource, MetadataDefinition definition)
        {
            base.MapToResource(resource, definition);

            resource.Enable = definition.Enable;
        }

        protected override void MapToModel(MetadataDefinition definition, MetadataResource resource)
        {
            base.MapToModel(definition, resource);

            definition.Enable = resource.Enable;
        }

        protected override void Validate(MetadataDefinition definition, bool includeWarnings)
        {
            if (!definition.Enable) return;
            base.Validate(definition, includeWarnings);
        }
    }
}