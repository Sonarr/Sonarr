using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Pending
{
    public class PendingRelease : ModelBase
    {
        public Int32 SeriesId { get; set; }
        public String Title { get; set; }
        public DateTime Added { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public ReleaseInfo Release { get; set; }

        //Not persisted
        public RemoteEpisode RemoteEpisode { get; set; }
    }
}
