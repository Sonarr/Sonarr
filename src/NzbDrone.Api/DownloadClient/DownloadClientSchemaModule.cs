using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Core.Download;
using Omu.ValueInjecter;

namespace NzbDrone.Api.DownloadClient
{
    public class DownloadClientSchemaModule : NzbDroneRestModule<DownloadClientResource>
    {
        private readonly IDownloadClientFactory _downloadClientFactory;

        public DownloadClientSchemaModule(IDownloadClientFactory downloadClientFactory)
            : base("downloadclient/schema")
        {
            _downloadClientFactory = downloadClientFactory;
            GetResourceAll = GetSchema;
        }

        private List<DownloadClientResource> GetSchema()
        {
            var downloadClients = _downloadClientFactory.Templates();

            var result = new List<DownloadClientResource>(downloadClients.Count);

            foreach (var downloadClient in downloadClients)
            {
                var downloadClientResource = new DownloadClientResource();
                downloadClientResource.InjectFrom(downloadClient);
                downloadClientResource.Fields = SchemaBuilder.ToSchema(downloadClient.Settings);

                result.Add(downloadClientResource);
            }

            return result;
        }
    }
}