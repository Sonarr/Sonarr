using System.Linq;
using Eloquera.Client;

namespace NzbDrone.Core.Datastore
{
    public abstract class BaseRepositoryModel
    {
        [ID]
        private long _eqId;

        [PetaPoco.Ignore]
        public int Id { get; set; }
    }
}
