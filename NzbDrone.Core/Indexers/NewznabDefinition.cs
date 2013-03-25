using System;
using NzbDrone.Core.Datastore;
using ServiceStack.DataAnnotations;


namespace NzbDrone.Core.Indexers
{
    [Alias("NewznabDefinitions")]
    public class NewznabDefinition : ModelBase
    {
        public Boolean Enable { get; set; }
        public String Name { get; set; }
        public String Url { get; set; }
        public String ApiKey { get; set; }
        public bool BuiltIn { get; set; }
    }
}