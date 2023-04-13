using System.Collections.Generic;

namespace NzbDrone.Core.Parser.Model
{
    public class GrabbedReleaseInfo
    {
        public string Title { get; set; }
        public string Indexer { get; set; }
        public long Size { get; set; }

        public List<int> EpisodeIds { get; set; }
    }
}
