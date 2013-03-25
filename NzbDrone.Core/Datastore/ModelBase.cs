using System.Data;
using System.Diagnostics;
using Marr.Data;

namespace NzbDrone.Core.Datastore
{
    [DebuggerDisplay("{GetType()} ID = {Id}")]
    public abstract class ModelBase
    {
        public int Id { get; set; }
    }
}
