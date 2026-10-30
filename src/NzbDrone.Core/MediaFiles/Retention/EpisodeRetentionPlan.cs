using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Retention
{
    public class EpisodeRetentionPlan
    {
        public int SeriesId { get; set; }
        public bool Enabled { get; set; }
        public int KeepEpisodes { get; set; }
        public string PreviewToken { get; set; }
        public List<Episode> Episodes { get; set; } = new List<Episode>();
        public List<Episode> RetainedEpisodes { get; set; } = new List<Episode>();
        public List<EpisodeFile> FilesToDelete { get; set; } = new List<EpisodeFile>();
        public List<Episode> EpisodesToUnmonitor { get; set; } = new List<Episode>();
        public List<Episode> OlderMissingEpisodesToUnmonitor { get; set; } = new List<Episode>();
    }
}
