using NzbDrone.Core.Download;

namespace NzbDrone.Api.DownloadClient
{
    public class DownloadClientModule : ProviderModuleBase<DownloadClientResource, IDownloadClient, DownloadClientDefinition>
    {
        public DownloadClientModule(IDownloadClientFactory downloadClientFactory)
            : base(downloadClientFactory, "downloadclient")
        {
        }

        protected override void Validate(DownloadClientDefinition definition)
        {
            if (!definition.Enable) return;
            base.Validate(definition);
        }
    }
}