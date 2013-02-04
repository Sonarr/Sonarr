using System;
using System.IO;
using System.Linq;
using Eloquera.Client;

namespace NzbDrone.Core.Datastore
{
    public class EloqueraDbFactory
    {
        public EloqueraDb CreateMemoryDb()
        {
            return InternalCreate("server=(local);password=;options=inmemory;",Guid.NewGuid().ToString());
        }

        public EloqueraDb Create(string dbPath)
        {
            var file = new FileInfo(dbPath).Name;
            return InternalCreate(string.Format("server=(local);database={0};usedatapath={1};password=;", file, dbPath),file);
        }

        private EloqueraDb InternalCreate(string connectionString, string databaseName)
        {
            var db = new DB(connectionString);
            db.CreateDatabase(databaseName);
            db.OpenDatabase(databaseName);
            return new EloqueraDb(db);
        }


    }
}
