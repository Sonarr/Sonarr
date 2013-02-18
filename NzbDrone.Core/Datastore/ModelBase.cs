using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Core.Datastore
{
    public abstract class ModelBase
    {
        [PetaPoco.Ignore]
        [JsonProperty(PropertyName = "id")]
        public int OID { get; set; }
    }
}
