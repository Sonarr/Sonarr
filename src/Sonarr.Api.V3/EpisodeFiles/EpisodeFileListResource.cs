using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace Sonarr.Api.V3.EpisodeFiles
{
    public class EpisodeFileListResource
    {
        public List<int> EpisodeFileIds { get; set; }
        public Language Language { get; set; }
        public QualityModel Quality { get; set; }
    }
}
