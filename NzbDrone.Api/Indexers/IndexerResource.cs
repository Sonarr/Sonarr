using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Indexers
{
    public class IndexerResource : RestResource
    {
        public Boolean Enable { get; set; }
        public String Name { get; set; }
        public String Settings { get; set; }
    }
}