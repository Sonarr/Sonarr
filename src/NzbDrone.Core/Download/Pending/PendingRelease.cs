using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Pending
{
    public class PendingRelease : ModelBase
    {
        public int SeriesId { get; set; }
        public string Title { get; set; }
        public DateTime Added { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public ReleaseInfo Release { get; set; }
        public PendingReleaseReason Reason { get; set; }
        public PendingReleaseAdditionalInfo AdditionalInfo { get; set; }

        // Not persisted
        public RemoteEpisode RemoteEpisode { get; set; }
    }

    public class PendingReleaseAdditionalInfo
    {
        public SeriesMatchType SeriesMatchType { get; set; }
    }
}
