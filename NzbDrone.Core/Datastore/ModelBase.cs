using System.Diagnostics;
using ServiceStack.DataAnnotations;

namespace NzbDrone.Core.Datastore
{
    [DebuggerDisplay("{GetType()} ID = {Id}")]
    public abstract class ModelBase
    {
        [AutoIncrement]
        public int Id { get; set; }
    }
}
