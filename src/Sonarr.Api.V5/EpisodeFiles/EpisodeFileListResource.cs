using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace Sonarr.Api.V5.EpisodeFiles
{
    public class EpisodeFileListResource
    {
        public List<int> EpisodeFileIds { get; set; } = new ();
        public List<Language>? Languages { get; set; }
        public QualityModel? Quality { get; set; }
        public string? SceneName { get; set; }
        public string? ReleaseGroup { get; set; }
    }
}
