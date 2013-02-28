using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Core.Datastore
{
    [DebuggerDisplay("{GetType()} ID = {Id}")]
    public abstract class ModelBase
    {
        [JsonIgnore]
        public int Id
        {
            get { return OID; }
            set { OID = value; }
        }

        [JsonProperty(PropertyName = "id")]
        public int OID { get; set; }
    }
}
