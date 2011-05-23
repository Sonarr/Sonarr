using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Migrator.Framework;
using NLog;
using SubSonic.Extensions;
using SubSonic.Schema;

namespace NzbDrone.Core.Datastore
{
    public class Migrations
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Run()
        {
            Logger.Info("Preparing to migrate databse");

            try
            {
                var mig = new Migrator.Migrator("Sqlite", Connection.MainConnectionString,
                                                      Assembly.GetAssembly(typeof(Migrations)), true, new MigrationLogger());

                mig.MigrateToLastVersion();

                Logger.Info("Database migration completed");
            }
            catch (Exception e)
            {
                Logger.FatalException("An error has occured while migrating database", e);
            }
        }


        public static void RemoveDeletedColumns(ITransformationProvider transformationProvider)
        {
            var provider = new RepositoryProvider();
            var repoTypes = provider.GetRepositoryTypes();

            foreach (var repoType in repoTypes)
            {
                var typeSchema = provider.GetSchemaFromType(repoType);
                var dbColumns = provider.GetColumnsFromDatabase(Connection.MainConnectionString, typeSchema.Name);

                var deletedColumns = provider.GetDeletedColumns(typeSchema, dbColumns);

                foreach (var deletedColumn in deletedColumns)
                {
                    Logger.Info("Removing column '{0}' from '{1}'", deletedColumn.Name, repoType.Name);
                    transformationProvider.RemoveColumn(typeSchema.Name, deletedColumn.Name);
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
                var dbColumns = provider.GetColumnsFromDatabase(Connection.MainConnectionString, typeSchema.Name);

                var newColumns = provider.GetNewColumns(typeSchema, dbColumns);

                foreach (var newColumn in newColumns)
                {
                    Logger.Info("Adding column '{0}' to '{1}'", newColumn.Name, repoType.Name);
                    transformationProvider.AddColumn(typeSchema.Name, newColumn);
                }

            }

        }

    }

    [Migration(20110523)]
    public class Migration20110523 : Migration
    {
        public override void Up()
        {
            Migrations.RemoveDeletedColumns(Database);
            Migrations.AddNewColumns(Database);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}