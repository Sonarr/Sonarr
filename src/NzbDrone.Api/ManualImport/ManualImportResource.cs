using System.Collections.Generic;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.REST;
using NzbDrone.Api.Series;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.ManualImport
{
    public class ManualImportResource : RestResource
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public SeriesResource Series { get; set; }
        public int? SeasonNumber { get; set; }
        public List<EpisodeResource> Episodes { get; set; }
        public QualityModel Quality { get; set; }
        public int QualityWeight { get; set; }
        public string DownloadId { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }

        public int Id
        {
            get
            {
                return Path.GetHashCode();
            }
        }
    }
}
