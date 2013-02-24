using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Configuration
{
    public class Config : ModelBase
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}