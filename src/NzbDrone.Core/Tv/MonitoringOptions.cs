using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public class MonitoringOptions : IEmbeddedDocument
    {
        public bool IgnoreEpisodesWithFiles { get; set; }
        public bool IgnoreEpisodesWithoutFiles { get; set; }
        public MonitorTypes Monitor { get; set; }
    }

    public enum MonitorTypes
    {
        Unknown,
        All,
        Future,
        Missing,
        Existing,
        FirstSeason,
        LastSeason,

        [Obsolete]
        LatestSeason,

        Pilot,
        Recent,
        MonitorSpecials,
        UnmonitorSpecials,
        None
    }

    public enum NewItemMonitorTypes
    {
        All,
        None
    }
}
