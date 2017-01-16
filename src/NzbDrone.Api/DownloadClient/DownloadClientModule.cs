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
        }

        protected override void MapToModel(DownloadClientDefinition definition, DownloadClientResource resource)
        {
            base.MapToModel(definition, resource);

            definition.Enable = resource.Enable;
            definition.Protocol = resource.Protocol;
        }

        protected override void Validate(DownloadClientDefinition definition, bool includeWarnings)
        {
            if (!definition.Enable) return;
            base.Validate(definition, includeWarnings);
        }
    }
}