using System;
using NzbDrone.Core.Datastore;
using ServiceStack.DataAnnotations;


namespace NzbDrone.Core.Indexers
{
    [Alias("IndexerDefinitions")]
    public class Indexer : ModelBase
    {
        public Boolean Enable { get; set; }
        public String Type { get; set; }
        public String Name { get; set; }
    }
}