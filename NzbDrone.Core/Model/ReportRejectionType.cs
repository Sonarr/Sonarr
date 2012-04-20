using System.Linq;

namespace NzbDrone.Core.Model
{
    public enum ReportRejectionType
    {
        None = 0,
        WrongSeries = 1,
        QualityNotWanted = 2,
        WrongSeason = 3,
        WrongEpisode = 3,
        Size = 3,
        Retention = 3,
        ExistingQualityIsEqualOrBetter = 4,
        Cutoff = 5,
        AlreadyInQueue = 6,
        DownloadClientFailure = 7,
        Skipped = 8,
        Failure,
    }
}
