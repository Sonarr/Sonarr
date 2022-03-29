using NzbDrone.Core.Download;

namespace NzbDrone.Api.DownloadClient
{
    public class DownloadClientModule : ProviderModuleBase<DownloadClientResource, IDownloadClient, DownloadClientDefinition>
    {
        public DownloadClientModule(IDownloadClientFactory downloadClientFactory)
            : base(downloadClientFactory, "downloadclient")
        {
        }

        protected override void MapToResource(DownloadClientResource resource, DownloadClientDefinition definition)
        {
            base.MapToResource(resource, definition);

            resource.Enable = definition.Enable;
            resource.Protocol = definition.Protocol;
            resource.Priority = definition.Priority;
            resource.RemoveCompletedDownloads = definition.RemoveCompletedDownloads;
            resource.RemoveFailedDownloads = definition.RemoveFailedDownloads;
        }

        protected override void MapToModel(DownloadClientDefinition definition, DownloadClientResource resource)
        {
            base.MapToModel(definition, resource);

            definition.Enable = resource.Enable;
            definition.Protocol = resource.Protocol;
            definition.Priority = resource.Priority;
            definition.RemoveCompletedDownloads = resource.RemoveCompletedDownloads;
            definition.RemoveFailedDownloads = resource.RemoveFailedDownloads;
        }
    }
}