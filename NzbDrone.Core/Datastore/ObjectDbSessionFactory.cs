using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.IO;
using Db4objects.Db4o.Internal;

namespace NzbDrone.Core.Datastore
{
    public class ObjectDbSessionFactory
    {
        public IObjectDbSession Create(IStorage storage = null, string dbName = "nzbdrone.db4o")
        {
            if (storage == null)
            {
                storage = new FileStorage();
            }

            var config = Db4oEmbedded.NewConfiguration();
            config.File.Storage = storage;


            var objectContainer = Db4oEmbedded.OpenFile(config, dbName);
            return new ObjectDbSession((ObjectContainerBase)objectContainer);
        }
    }
}