using NzbDrone.Core.Datastore;
using ServiceStack.DataAnnotations;

namespace NzbDrone.Core.Configuration
{
    public class Config : ModelBase
    {
        [Index(Unique = true)]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}