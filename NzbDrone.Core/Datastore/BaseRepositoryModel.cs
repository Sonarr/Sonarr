using System.Linq;

namespace NzbDrone.Core.Datastore
{
    public abstract class BaseRepositoryModel
    {
        [PetaPoco.Ignore]
        public int OID { get; set; }
    }
}
