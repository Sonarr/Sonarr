using System;
using System.ComponentModel.DataAnnotations;
using NzbDrone.Core.Datastore;
using PetaPoco;

namespace NzbDrone.Core.Indexers
{
    public class NewznabDefinition : ModelBase
    {
        public Boolean Enabled { get; set; }
        public String Name { get; set; }
        public String Url { get; set; }
        public String ApiKey { get; set; }
        public bool BuiltIn { get; set; }
    }
}