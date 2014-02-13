using NzbDrone.Core.Metadata;

namespace NzbDrone.Api.Metadata
{
    public class MetadataModule : ProviderModuleBase<MetadataResource, IMetadata, MetadataDefinition>
    {
        public MetadataModule(IMetadataFactory metadataFactory)
            : base(metadataFactory, "metadata")
        {
        }

        protected override void Validate(MetadataDefinition definition)
        {
            if (!definition.Enable) return;
            base.Validate(definition);
        }
    }
}