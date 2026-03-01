using NzbDrone.Core.Extras.Metadata;
using Sonarr.Api.V5.Provider;

namespace Sonarr.Api.V5.Metadata;

public class MetadataResource : ProviderResource<MetadataResource>
{
    public bool Enable { get; set; }
}

public class MetadataResourceMapper : ProviderResourceMapper<MetadataResource, MetadataDefinition>
{
    public override MetadataResource ToResource(MetadataDefinition definition)
    {
        var resource = base.ToResource(definition);

        resource.Enable = definition.Enable;

        return resource;
    }

    public override MetadataDefinition ToModel(MetadataResource resource, MetadataDefinition? existingDefinition)
    {
        var definition = base.ToModel(resource, existingDefinition);

        definition.Enable = resource.Enable;

        return definition;
    }
}
