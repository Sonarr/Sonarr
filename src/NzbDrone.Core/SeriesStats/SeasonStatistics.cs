using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.SeriesStats
{
    public class SeasonStatistics : ResultSet
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public string NextAiringString { get; set; }
        public string PreviousAiringString { get; set; }
        public int EpisodeFileCount { get; set; }
        public int EpisodeCount { get; set; }
        public int AvailableEpisodeCount { get; set; }
        public int TotalEpisodeCount { get; set; }
        public long SizeOnDisk { get; set; }
        public string ReleaseGroupsString { get; set; }

        public DateTime? NextAiring
        {
            get
            {
                DateTime nextAiring;

                try
                {
                    if (!DateTime.TryParse(NextAiringString, out nextAiring))
                    {
                        return null;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    // GHI 3518: Can throw on mono (6.x?) despite being a Try*
                    return null;
                }

                return nextAiring;
            }
        }

        public DateTime? PreviousAiring
        {
            get
            {
                DateTime previousAiring;

                try
                {
                    if (!DateTime.TryParse(PreviousAiringString, out previousAiring))
                    {
                        return null;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    // GHI 3518: Can throw on mono (6.x?) despite being a Try*
                    return null;
                }

                return previousAiring;
            }
        }

        public List<string> ReleaseGroups
        {
            get
            {
                var releasegroups = new List<string>();

                if (ReleaseGroupsString.IsNotNullOrWhiteSpace())
                {
                    releasegroups = ReleaseGroupsString
                        .Split('|')
                        .Distinct()
                        .Where(rg => rg.IsNotNullOrWhiteSpace())
                        .OrderBy(rg => rg)
                        .ToList();
                }

                return releasegroups;
            }
        }
    }
}
