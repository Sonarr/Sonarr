using System;
using System.ServiceModel.Syndication;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class LocalEpisode : Episode
    {
        public String Path { get; set; }
    }
}