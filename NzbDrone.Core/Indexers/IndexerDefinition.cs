using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Indexers
{
    public class IndexerDefinition : ModelBase
    {
        public Boolean Enable { get; set; }
        public String Name { get; set; }
        public String Settings { get; set; }
        public String Implementation { get; set; }
    }
}