using System;
using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Indexers
{
    public class IndexerResource : RestResource
    {
        public Boolean Enable { get; set; }
        public String Name { get; set; }

        public List<Field> Fields { get; set; }
    }
}