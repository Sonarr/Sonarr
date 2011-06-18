using System;
using System.Collections.Generic;
using System.Reflection;
using NLog;

namespace NzbDrone.Core.Datastore
{
    public class MigrationsHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static readonly Dictionary<String, String> _migrated = new Dictionary<string, string>();

        public static void Run(string connetionString, bool trace)
        {
            if (_migrated.ContainsKey(connetionString)) return;
            _migrated.Add(connetionString, string.Empty);

            Logger.Info("Preparing run database migration");

            try
            {
                Migrator.Migrator migrator;
                if (trace)
                {
                    migrator = new Migrator.Migrator("Sqlite", connetionString, Assembly.GetAssembly(typeof(MigrationsHelper)), true, new MigrationLogger());
                }
                else
                {
                    migrator = new Migrator.Migrator("Sqlite", connetionString, Assembly.GetAssembly(typeof(MigrationsHelper)));
                }



                migrator.MigrateToLastVersion();

                //ForceSubSonicMigration(Connection.CreateSimpleRepository(connetionString));

                Logger.Info("Database migration completed");


            }
            catch (Exception e)
            {
                Logger.FatalException("An error has occured while migrating database", e);
            }
        }


    }


}