using System;
using System.Data.Common;
using System.Data.SqlServerCe;
using Db4objects.Db4o;
using Db4objects.Db4o.IO;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Config;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace NzbDrone.Core.Datastore
{
    class DbProviderFactory : System.Data.Common.DbProviderFactory
    {
        public Boolean IsProfiled { get; set; }

        public override DbConnection CreateConnection()
        {
            var sqliteConnection = new SqlCeConnection();
            DbConnection connection = sqliteConnection;

            if (IsProfiled)
            {
                connection = new ProfiledDbConnection(sqliteConnection, MiniProfiler.Current);
            }

            return connection;
        }
    }


    public class ObjectDbSessionFactory
    {
        public IObjectDbSession Create(IStorage storage = null)
        {
            if (storage == null)
            {
                storage = new FileStorage();
            }

            var config = Db4oEmbedded.NewConfiguration();
            config.File.Storage = storage;
           

            var objectContainer = Db4oEmbedded.OpenFile(config, "nzbdrone.db4o");
            return new ObjectDbSession((ObjectContainerBase)objectContainer);
        }
    }
}
