﻿using NzbDrone.Core.Download;

namespace Sonarr.Api.V3.DownloadClient
{
    public class DownloadClientModule : ProviderModuleBase<DownloadClientResource, IDownloadClient, DownloadClientDefinition>
    {
        public static readonly DownloadClientResourceMapper ResourceMapper = new DownloadClientResourceMapper();

        public DownloadClientModule(IDownloadClientFactory downloadClientFactory)
            : base(downloadClientFactory, "downloadclient", ResourceMapper)
        {
        }
    }
}
