using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;

namespace Sonarr.Api.V3.DownloadClient
{
    public class DownloadClientResource : ProviderResource
    {
        public bool Enable { get; set; }
        public DownloadProtocol Protocol { get; set; }
    }

    public class DownloadClientResourceMapper : ProviderResourceMapper<DownloadClientResource, DownloadClientDefinition>
    {
        public override DownloadClientResource ToResource(DownloadClientDefinition definition)
        {
            if (definition == null) return null;

            var resource = base.ToResource(definition);

            resource.Enable = definition.Enable;
            resource.Protocol = definition.Protocol;

            return resource;
        }

        public override DownloadClientDefinition ToModel(DownloadClientResource resource)
        {
            if (resource == null) return null;

            var definition = base.ToModel(resource);

            definition.Enable = resource.Enable;
            definition.Protocol = resource.Protocol;

            return definition;
        }
    }
}