using System;
using NzbDrone.Core.Datastore;


namespace NzbDrone.Core.Indexers
{
    public class NewznabDefinition : ModelBase
    {
        public Boolean Enable { get; set; }
        public String Name { get; set; }
        public String Url { get; set; }
        public String ApiKey { get; set; }
        public bool BuiltIn { get; set; }
    }
}