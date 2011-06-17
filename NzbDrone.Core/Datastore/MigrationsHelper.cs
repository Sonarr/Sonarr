using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Migrator.Framework;
using NLog;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Extensions;
using SubSonic.Repository;
using SubSonic.Schema;

namespace NzbDrone.Core.Datastore
{
    public class MigrationsHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static bool IsMigrated { get; private set; }

        public static void Run(string connetionString, bool trace)
        {
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

                ForceSubSonicMigration(Connection.CreateSimpleRepository(connetionString));

                Logger.Info("Database migration completed");


            }
            catch (Exception e)
            {
                Logger.FatalException("An error has occured while migrating database", e);
            }
        }

        public static void ForceSubSonicMigration(IRepository repository)
        {
            repository.Single<QualityProfile>(1);
            repository.Single<IndexerSetting>(1);
            repository.Single<SceneMapping>(1);
        }


        public static void RemoveDeletedColumns(ITransformationProvider transformationProvider)
        {
            var provider = new RepositoryProvider();
            var repoTypes = provider.GetRepositoryTypes();

            foreach (var repoType in repoTypes)
            {
                var typeSchema = provider.GetSchemaFromType(repoType);

                if (transformationProvider.TableExists(typeSchema.Name))
                {
                    var dbColumns = provider.GetColumnsFromDatabase(transformationProvider, typeSchema.Name);

                    var deletedColumns = provider.GetDeletedColumns(typeSchema, dbColumns);

                    foreach (var deletedColumn in deletedColumns)
                    {
                        Logger.Info("Removing column '{0}' from '{1}'", deletedColumn.Name, repoType.Name);
                        transformationProvider.RemoveColumn(typeSchema.Name, deletedColumn.Name);
                    }
                }
            }

        }

        public static void AddNewColumns(ITransformationProvider transformationProvider)
        {
            var provider = new RepositoryProvider();
            var repoTypes = provider.GetRepositoryTypes();

            foreach (var repoType in repoTypes)
            {
                var typeSchema = provider.GetSchemaFromType(repoType);
                if (transformationProvider.TableExists(typeSchema.Name))
                {
                    var dbColumns = provider.GetColumnsFromDatabase(transformationProvider, typeSchema.Name);

                    var newColumns = provider.GetNewColumns(typeSchema, dbColumns);

                    foreach (var newColumn in newColumns)
                    {
                        Logger.Info("Adding column '{0}' to '{1}'", newColumn.Name, repoType.Name);
                        transformationProvider.AddColumn(typeSchema.Name, newColumn);
                    }
                }

            }

        }

    }


}