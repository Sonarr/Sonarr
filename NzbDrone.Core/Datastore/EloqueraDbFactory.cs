using System;
using System.IO;
using System.Linq;
using Eloquera.Client;
using NzbDrone.Common;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Datastore
{
    public class EloqueraDbFactory
    {
        private readonly EnvironmentProvider _environmentProvider;

        private readonly string dllPath;

        public EloqueraDbFactory(EnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
            dllPath = _environmentProvider.GetWebBinPath();// this is the path where Eloquera dlls live.
        }

        public EloqueraDb CreateMemoryDb()
        {
            return InternalCreate("server=(local);password=;options=inmemory;uselocalpath=" + dllPath, Guid.NewGuid().ToString());
        }

        public EloqueraDb Create(string dbPath = null)
        {
            if (dbPath == null)
            {
                dbPath = _environmentProvider.GetElqMainDbPath();
            }

            var file = new FileInfo(dbPath);

            return InternalCreate(string.Format("server=(local);password=;usedatapath={0};uselocalpath={1}", file.Directory.FullName, dllPath), file.Name);
        }

        private EloqueraDb InternalCreate(string connectionString, string databaseName)
        {
            var db = new DB(connectionString);
            try
            {
                db.OpenDatabase(databaseName);
            }
            catch (FileNotFoundException)
            {
                db.CreateDatabase(databaseName);
                db.OpenDatabase(databaseName);
            }

            RegisterTypeRules();
            RegisterTypes(db);

            return new EloqueraDb(db);
        }

        private void RegisterTypeRules()
        {
            RootFolder rootFolder = null;
            DB.TypeRules
              .IgnoreProperty(() => rootFolder.FreeSpace)
              .IgnoreProperty(() => rootFolder.UnmappedFolders);
        }

        private void RegisterTypes(DB db)
        {
            db.RegisterType(typeof(RootFolder));
        }
    }
}
