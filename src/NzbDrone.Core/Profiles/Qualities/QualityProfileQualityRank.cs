using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class QualityProfileQualityRank : ModelBase
    {
        public int ProfileId { get; set; }
        public int QualityId { get; set; }
        public double Score { get; set; }
    }
}
