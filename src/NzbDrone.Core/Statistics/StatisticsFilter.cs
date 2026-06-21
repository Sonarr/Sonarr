using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Statistics;

public class StatisticsFilter
{
    public List<string> RootFolderPaths { get; set; }
    public bool RootFolderPathsNot { get; set; }
    public List<int> TagIds { get; set; }
    public bool TagIdsNot { get; set; }
    public List<int> QualityProfileIds { get; set; }
    public bool QualityProfileIdsNot { get; set; }
    public bool? Monitored { get; set; }
    public List<SeriesTypes> SeriesTypes { get; set; }
    public bool SeriesTypesNot { get; set; }
}
