using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace Sonarr.Api.V3.Series
{
    public class SeriesEditorResource
    {
        public List<int> SeriesIds { get; set; }
        public bool? Monitored { get; set; }
        public NewItemMonitorTypes? MonitorNewItems { get; set; }
        public int? QualityProfileId { get; set; }
        public SeriesTypes? SeriesType { get; set; }
        public bool? SeasonFolder { get; set; }
        public string RootFolderPath { get; set; }
        public List<int> Tags { get; set; }
        public ApplyTags ApplyTags { get; set; }
        public bool MoveFiles { get; set; }
        public bool DeleteFiles { get; set; }
        public bool AddImportListExclusion { get; set; }
    }
}
