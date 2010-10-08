using System.ServiceModel.Syndication;
using NzbDrone.Core.Entities.Quality;

namespace NzbDrone.Core.Entities.Episode
{
    public class RemoteEpisode : BasicEpisode
    {
        public QualityTypes Quality { get; set; }
        public SyndicationItem Feed { get; set; }
        public bool Proper { get; set; }
    }
}
