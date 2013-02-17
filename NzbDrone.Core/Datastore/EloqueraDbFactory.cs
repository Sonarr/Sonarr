using System;
using System.IO;
using System.Linq;
using Eloquera.Client;
using NzbDrone.Common;
using NzbDrone.Core.Repository;
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
            if (EnvironmentProvider.IsMono)
            {
                return InternalCreate("server=(local);password=;options=inmemory", Guid.NewGuid().ToString());

            }
            return InternalCreate("server=(local);password=;options=inmemory;uselocalpath=" + dllPath, Guid.NewGuid().ToString());

        }

        public EloqueraDb Create(string dbPath = null)
        {
            if (dbPath == null)
            {
                dbPath = _environmentProvider.GetElqMainDbPath();
            }

            var file = new FileInfo(dbPath);

            if (EnvironmentProvider.IsMono)
            {
                return InternalCreate(string.Format("server=(local);password=;usedatapath={0}", file.Directory.FullName), file.Name);
            }
            else
            {
                return InternalCreate(string.Format("server=(local);password=;usedatapath={0};uselocalpath={1}", file.Directory.FullName, dllPath), file.Name);
            }

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

            //This seemse to cause Invalid Cast Exceptions... WTF
            //db.RefreshMode = ObjectRefreshMode.AlwaysReturnUpdatedValues;

            RegisterTypeRules();
            RegisterTypes(db);

            return new EloqueraDb(db, new IdService(new IndexProvider(db)));
        }

        private void RegisterTypeRules()
        {
            RootFolder rootFolder = null;
            DB.TypeRules
                //.SetIDField(() => rootFolder.Id)
              .IgnoreProperty(() => rootFolder.FreeSpace)
              .IgnoreProperty(() => rootFolder.UnmappedFolders);

            //Series series = null;
            //DB.TypeRules
            //  .SetIDField(() => series.Id);
        }

        private void RegisterTypes(DB db)
        {
            db.RegisterType(typeof(RootFolder));
            db.RegisterType(typeof(Series));
            db.RegisterType(typeof(Episode));
        }
    }
}
