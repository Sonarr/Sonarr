using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using Sonarr.Api.V5.Provider;

namespace Sonarr.Api.V5.DownloadClient;

public class DownloadClientResource : ProviderResource<DownloadClientResource>
{
    public bool Enable { get; set; }
    public DownloadProtocol Protocol { get; set; }
    public int Priority { get; set; }
    public bool RemoveCompletedDownloads { get; set; }
    public bool RemoveFailedDownloads { get; set; }
}

public class DownloadClientResourceMapper : ProviderResourceMapper<DownloadClientResource, DownloadClientDefinition>
{
    public override DownloadClientResource ToResource(DownloadClientDefinition definition)
    {
        var resource = base.ToResource(definition);

        resource.Enable = definition.Enable;
        resource.Protocol = definition.Protocol;
        resource.Priority = definition.Priority;
        resource.RemoveCompletedDownloads = definition.RemoveCompletedDownloads;
        resource.RemoveFailedDownloads = definition.RemoveFailedDownloads;

        return resource;
    }

    public override DownloadClientDefinition ToModel(DownloadClientResource resource, DownloadClientDefinition? existingDefinition)
    {
        var definition = base.ToModel(resource, existingDefinition);

        definition.Enable = resource.Enable;
        definition.Protocol = resource.Protocol;
        definition.Priority = resource.Priority;
        definition.RemoveCompletedDownloads = resource.RemoveCompletedDownloads;
        definition.RemoveFailedDownloads = resource.RemoveFailedDownloads;

        return definition;
    }
}
