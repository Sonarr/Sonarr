using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Indexers
{
    public class Indexer : ModelBase
    {
        public Boolean Enable { get; set; }
        public String Type { get; set; }
        public String Name { get; set; }
    }
}