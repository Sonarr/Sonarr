using System;
using System.ServiceModel.Syndication;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class RemoteEpisode : Episode
    {
        [SubSonicIgnore]
        public SyndicationItem Feed { get; set; }
    }
}