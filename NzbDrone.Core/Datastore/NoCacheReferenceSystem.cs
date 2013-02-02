using System.Linq;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.References;

namespace NzbDrone.Core.Datastore
{
    public class NoCacheReferenceSystem : HashcodeReferenceSystem
    {
        public override ObjectReference ReferenceForId(int id)
        {
            //never return an in memory instance of objects as query result. always go to db.
            return null;
        }
    }
}
