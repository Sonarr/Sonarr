using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using NzbDrone.Core.Repository.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository.Episode
{
    public class RemoteEpisode : Episode
    {
        public QualityTypes Quality { get; set; }
        public SyndicationItem Feed { get; set; }
        public bool Proper { get; set; }
    }
}
