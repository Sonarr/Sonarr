using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Core.Datastore
{
    [DebuggerDisplay("ID = {OID}")]
    public abstract class ModelBase
    {
        [PetaPoco.Ignore]
        [JsonProperty(PropertyName = "id")]
        public int OID { get; set; }
    }
}
