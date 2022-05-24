using NzbDrone.Core.Extras.Metadata;

namespace Sonarr.Api.V3.Metadata
{
    public class MetadataResource : ProviderResource<MetadataResource>
    {
        public bool Enable { get; set; }
    }

    public class MetadataResourceMapper : ProviderResourceMapper<MetadataResource, MetadataDefinition>
    {
        public override MetadataResource ToResource(MetadataDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            var resource = base.ToResource(definition);

            resource.Enable = definition.Enable;

            return resource;
        }

        public override MetadataDefinition ToModel(MetadataResource resource, MetadataDefinition existingDefinition)
        {
            if (resource == null)
            {
                return null;
            }

            var definition = base.ToModel(resource, existingDefinition);

            definition.Enable = resource.Enable;

            return definition;
        }
    }
}
