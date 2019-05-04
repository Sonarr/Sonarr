using System.Collections.Generic;
using Sonarr.Api.V3.Episodes;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.ManualImport
{
    public class ManualImportReprocessResource : RestResource
    {
        public string Path { get; set; }
        public int SeriesId { get; set; }
        public int? SeasonNumber { get; set; }
        public List<EpisodeResource> Episodes { get; set; }
        public string DownloadId { get; set; }
    }
}
