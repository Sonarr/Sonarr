using System.Collections.Generic;
using NzbDrone.Core.Download;

namespace Sonarr.Api.V3.DownloadClient
{
    public class DownloadClientBulkResource : ProviderBulkResource<DownloadClientBulkResource>
    {
        public bool? Enable { get; set; }
        public int? Priority { get; set; }
        public bool? RemoveCompletedDownloads { get; set; }
        public bool? RemoveFailedDownloads { get; set; }
    }

    public class DownloadClientBulkResourceMapper : ProviderBulkResourceMapper<DownloadClientBulkResource, DownloadClientDefinition>
    {
        public override List<DownloadClientDefinition> UpdateModel(DownloadClientBulkResource resource, List<DownloadClientDefinition> existingDefinitions)
        {
            if (resource == null)
            {
                return new List<DownloadClientDefinition>();
            }

            existingDefinitions.ForEach(existing =>
            {
                existing.Enable = resource.Enable ?? existing.Enable;
                existing.Priority = resource.Priority ?? existing.Priority;
                existing.RemoveCompletedDownloads = resource.RemoveCompletedDownloads ?? existing.RemoveCompletedDownloads;
                existing.RemoveFailedDownloads = resource.RemoveFailedDownloads ?? existing.RemoveFailedDownloads;
            });

            return existingDefinitions;
        }
    }
}
