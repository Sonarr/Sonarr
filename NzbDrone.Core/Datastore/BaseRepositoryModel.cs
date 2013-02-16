using System.Linq;
using Eloquera.Client;

namespace NzbDrone.Core.Datastore
{
    public abstract class BaseRepositoryModel
    {
        [ID]
        private long _eqId;

        public int Id { get; set; }
    }
}
